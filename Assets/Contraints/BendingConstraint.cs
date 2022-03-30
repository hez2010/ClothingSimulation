using Assets.Utils;
using UnityEngine;

namespace Assets.Contraints
{
    class BendingConstraint : ConstraintBase
    {
        private readonly float _stiffness;
        private readonly int _index1, _index2, _index3, _index4;
        private readonly float _angle;

        public BendingConstraint(ClothComponent cloth, float stiffness, int index1, int index2, int index3, int index4) : base(cloth)
        {
            _stiffness = stiffness;
            _index1 = index1;
            _index2 = index2;
            _index3 = index3;
            _index4 = index4;
            _angle = 1f / 6;
        }

        public override void Resolve(float dt)
        {
            var p1 = _cloth.Predicts[_index1];
            var p2 = _cloth.Predicts[_index2] - p1;
            var p3 = _cloth.Predicts[_index3] - p1;
            var p4 = _cloth.Predicts[_index4] - p1;
            p1 = new();
            var n1 = p2.Cross(p3).normalized;
            var n2 = p2.Cross(p4).normalized;

            var d = n1.Dot(n2);

            var p23Len = p2.Cross(p3).magnitude;
            var p24Len = p2.Cross(p4).magnitude;

            var q3 = (p2.Cross(n2) + (n1.Cross(p2) * d)) / p23Len;
            var q4 = (p2.Cross(n1) + (n2.Cross(p2) * d)) / p24Len;
            var q2 = (-(p3.Cross(n2) + (n1.Cross(p3) * d)) / p23Len)
            - ((p4.Cross(n1) + (n2.Cross(p4) * d)) / p24Len);
            var q1 = -q2 - q3 - q4;

            var w1 = 1 / _cloth.Masses[_index1];
            var w2 = 1 / _cloth.Masses[_index2];
            var w3 = 1 / _cloth.Masses[_index3];
            var w4 = 1 / _cloth.Masses[_index4];

            var sum = (w1 * q1.sqrMagnitude)
                + (w2 * q2.sqrMagnitude)
                + (w3 * q3.sqrMagnitude)
                + (w4 * q4.sqrMagnitude);

            sum = Mathf.Max(0.01f, sum);

            var s = -(Mathf.Acos(d) - _angle) * Mathf.Sqrt(1 - (d * d)) / sum;

            if (float.IsFinite(s))
            {
                var dp1 = _stiffness * dt * s * w1 * q1;
                var dp2 = _stiffness * dt * s * w2 * q2;
                var dp3 = _stiffness * dt * s * w3 * q3;
                var dp4 = _stiffness * dt * s * w4 * q4;
                _cloth.Predicts[_index1] += dp1;
                _cloth.Predicts[_index2] += dp2;
                _cloth.Predicts[_index3] += dp3;
                _cloth.Predicts[_index4] += dp4;
            }
        }
    }
}
