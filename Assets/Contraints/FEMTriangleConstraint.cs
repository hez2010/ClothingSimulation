using Assets.Utils;
using System;
using UnityEngine;

namespace Assets.Contraints
{
    class FEMTriangleConstraint : ConstraintBase
    {
        private readonly (int A, int B, int C) _triangle;
        private readonly float _stiffness, _poissonRatio, _area;
        private readonly Matrix2x2 _restMat;

        public FEMTriangleConstraint(ClothComponent cloth, float stiffness, float poissonRatio, (int A, int B, int C) triangle) : base(cloth)
        {
            _triangle = triangle;
            _stiffness = stiffness;
            _poissonRatio = poissonRatio;

            var p0 = _cloth.Positions[triangle.A];
            var p1 = _cloth.Positions[triangle.B];
            var p2 = _cloth.Positions[triangle.C];
            var n = (p1 - p0).Cross(p2 - p0);
            _area = n.Norm() * 0.5f;
            var axis1 = (p1 - p0).normalized;
            var axis2 = n.Cross(axis1).normalized;
            var P0 = new Vector2(p0.Dot(axis2), p0.Dot(axis1));
            var P1 = new Vector2(p1.Dot(axis2), p1.Dot(axis1));
            var P2 = new Vector2(p2.Dot(axis2), p2.Dot(axis2));
            var P = new Matrix2x2(P0.x - P2.x, P1.x - P2.x, P0.y - P2.y, P1.y - P2.y);
            var det = P.Determinant();
            if (Mathf.Abs(det) > 1e-6f)
            {
                _restMat = P.Inverse();
            }
            else
            {
                ThrowInvalidConstraint();
            }
        }

        private void ThrowInvalidConstraint()
        {
            throw new InvalidOperationException("Invalid FEM triangle constraint.");
        }


        public override void Resolve(float dt)
        {
            var p0 = _cloth.Predicts[_triangle.A];
            var p1 = _cloth.Predicts[_triangle.B];
            var p2 = _cloth.Predicts[_triangle.C];

            // Orthotropic elasticity tensor
            var C = new Matrix3x3();
            C[0, 0] = C[1, 1] = _stiffness / (1.0f - (_poissonRatio * _poissonRatio));
            C[0, 1] = C[1, 0] = _stiffness * _poissonRatio / (1.0f - (_poissonRatio * _poissonRatio));
            C[2, 2] = 1.0f;

            // Determine \partial x/\partial m_i
            var p13 = p0 - p2;
            var p23 = p1 - p2;
            var F = new Matrix3x2(
                (p13[0] * _restMat[0, 0]) + (p23[0] * _restMat[1, 0]),
                (p13[0] * _restMat[0, 1]) + (p23[0] * _restMat[1, 1]),
                (p13[1] * _restMat[0, 0]) + (p23[1] * _restMat[1, 0]),
                (p13[1] * _restMat[0, 1]) + (p23[1] * _restMat[1, 1]),
                (p13[2] * _restMat[0, 0]) + (p23[2] * _restMat[1, 0]),
                (p13[2] * _restMat[0, 1]) + (p23[2] * _restMat[1, 1])
            );

            // epsilon = 0.5(F^T * F - I)
            var epsilon = new Matrix2x2();
            epsilon[0, 0] = 0.5f * ((F[0, 0] * F[0, 0]) + (F[1, 0] * F[1, 0]) + (F[2, 0] * F[2, 0]) - 1.0f);
            epsilon[1, 1] = 0.5f * ((F[0, 1] * F[0, 1]) + (F[1, 1] * F[1, 1]) + (F[2, 1] * F[2, 1]) - 1.0f);
            epsilon[0, 1] = 0.5f * ((F[0, 0] * F[0, 1]) + (F[1, 0] * F[1, 1]) + (F[2, 0] * F[2, 1]));
            epsilon[1, 0] = epsilon[0, 1];


            // P(F) = det(F) * C*E * F^-T => E = green strain
            var stress = new Matrix2x2();
            stress[0, 0] = (C[0, 0] * epsilon[0, 0]) + (C[0, 1] * epsilon[1, 1]) + (C[0, 2] * epsilon[0, 1]);
            stress[1, 1] = (C[1, 0] * epsilon[0, 0]) + (C[1, 1] * epsilon[1, 1]) + (C[1, 2] * epsilon[0, 1]);
            stress[0, 1] = (C[2, 0] * epsilon[0, 0]) + (C[2, 1] * epsilon[1, 1]) + (C[2, 2] * epsilon[0, 1]);
            stress[1, 0] = stress[0, 1];

            var piolaKirchhoffStres = F * stress;

            var psi = 0.0f;
            for (var j = 0; j < 2; j++)
                for (var k = 0; k < 2; k++)
                    psi += epsilon[j, k] * stress[j, k];
            psi = 0.5f * psi;
            var energy = _area * psi;

            // compute gradient
            var H = piolaKirchhoffStres * _restMat.Transpose() * _area;
            var gradC = new Vector3[3];
            for (var j = 0; j < 3; ++j)
            {
                gradC[0][j] = H[j, 0];
                gradC[1][j] = H[j, 1];
            }
            gradC[2] = -gradC[0] - gradC[1];

            var sumNormGradC = _cloth.Masses[0] * gradC[0].Norm() * gradC[0].Norm();
            sumNormGradC += _cloth.Masses[1] * gradC[1].Norm() * gradC[1].Norm();
            sumNormGradC += _cloth.Masses[2] * gradC[2].Norm() * gradC[2].Norm();

            if (Mathf.Abs(sumNormGradC) > 1e-6f)
            {
                // compute scaling factor
                var s = energy / sumNormGradC;

                // update positions
                var correction0 = -(s * _cloth.Masses[0]) * dt * gradC[0];
                var correction1 = -(s * _cloth.Masses[1]) * dt * gradC[1];
                var correction2 = -(s * _cloth.Masses[2]) * dt * gradC[2];

                _cloth.Predicts[_triangle.A] += correction0;
                _cloth.Predicts[_triangle.B] += correction1;
                _cloth.Predicts[_triangle.C] += correction2;
            }
        }
    }
}
