using UnityEngine;

namespace Assets.Utils
{
    struct Matrix3x3
    {
        public const float Epsilon = 1e-6f;
        private float _m00, _m01, _m02, _m10, _m11, _m12, _m20, _m21, _m22;

        public Matrix3x3(float m00, float m01, float m02, float m10, float m11, float m12, float m20, float m21, float m22)
        {
            _m00 = m00;
            _m01 = m01;
            _m02 = m02;
            _m10 = m10;
            _m11 = m11;
            _m12 = m12;
            _m20 = m20;
            _m21 = m21;
            _m22 = m22;
        }

        public Matrix3x3(Matrix3x3 matrix)
        {
            _m00 = matrix[0];
            _m01 = matrix[1];
            _m02 = matrix[2];
            _m10 = matrix[3];
            _m11 = matrix[4];
            _m12 = matrix[5];
            _m20 = matrix[6];
            _m21 = matrix[7];
            _m22 = matrix[8];
        }

        public float this[int i, int j]
        {
            get => (i, j) switch
            {
                (0, 0) => _m00,
                (0, 1) => _m01,
                (0, 2) => _m02,
                (1, 0) => _m10,
                (1, 1) => _m11,
                (1, 2) => _m12,
                (2, 0) => _m20,
                (2, 1) => _m21,
                (2, 2) => _m22,
                _ => default
            };
            set
            {
                switch (i, j)
                {
                    case (0, 0):
                        _m00 = value;
                        break;
                    case (0, 1):
                        _m01 = value;
                        break;
                    case (0, 2):
                        _m02 = value;
                        break;
                    case (1, 0):
                        _m10 = value;
                        break;
                    case (1, 1):
                        _m11 = value;
                        break;
                    case (1, 2):
                        _m12 = value;
                        break;
                    case (2, 0):
                        _m20 = value;
                        break;
                    case (2, 1):
                        _m21 = value;
                        break;
                    case (2, 2):
                        _m22 = value;
                        break;

                }
            }
        }

        public float this[int index]
        {
            get => index switch
            {
                0 => _m00,
                1 => _m01,
                2 => _m02,
                3 => _m10,
                4 => _m11,
                5 => _m12,
                6 => _m20,
                7 => _m21,
                8 => _m22,
                _ => default
            };
            set
            {
                switch (index)
                {
                    case 0:
                        _m00 = value;
                        break;
                    case 1:
                        _m01 = value;
                        break;
                    case 2:
                        _m02 = value;
                        break;
                    case 3:
                        _m10 = value;
                        break;
                    case 4:
                        _m11 = value;
                        break;
                    case 5:
                        _m12 = value;
                        break;
                    case 6:
                        _m20 = value;
                        break;
                    case 7:
                        _m21 = value;
                        break;
                    case 8:
                        _m22 = value;
                        break;
                }
            }
        }

        public void SetValue(float m00, float m01, float m02, float m10, float m11, float m12, float m20, float m21, float m22)
        {
            this[0, 0] = m00;
            this[0, 1] = m01;
            this[0, 2] = m02;
            this[1, 0] = m10;
            this[1, 1] = m11;
            this[1, 2] = m12;
            this[2, 0] = m20;
            this[2, 1] = m21;
            this[2, 2] = m22;
        }

        public void SetValue(float m00, float m11, float m22)
        {
            SetValue(m00, 0, 0, 0, m11, 0, 0, 0, m22);
        }

        public void SetValue(Matrix3x3 m)
        {
            SetValue(m[0, 0], m[0, 1], m[0, 2], m[1, 0], m[1, 1], m[1, 2], m[2, 0], m[2, 1], m[2, 2]);
        }

        public void SetValue(float value)
        {
            SetValue(value, value, value, value, value, value, value, value, value);
        }

        public void Normalize()
        {
            for (int row = 0; row < 3; row++)
            {
                float l = 0;
                for (int column = 0; column < 3; column++)
                {
                    l += this[row, column] * this[row, column];
                }

                l = Mathf.Sqrt(l);

                for (int column = 0; column < 2; column++)
                {
                    this[row, column] /= l;
                }
            }
        }

        public Matrix3x3 Transpose()
        {
            return new Matrix3x3(this[0, 0], this[1, 0], this[2, 0], this[0, 1], this[1, 1], this[2, 1], this[0, 2], this[1, 2], this[2, 2]);
        }

        public Matrix3x3 Inverse()
        {
            var invMatrix = new Matrix3x3((_m11 * _m22) - (_m12 * _m21), (_m02 * _m21) - (_m01 * _m22), (_m01 * _m12) - (_m02 * _m11),
                (_m00 * _m22) - (_m02 * _m20), (_m00 * _m22) - (_m02 * _m20), (_m02 * _m10) - (_m00 * _m12),
                (_m10 * _m21) - (_m11 * _m20), (_m01 * _m20) - (_m00 * _m21), (_m00 * _m11) - (_m01 * _m10));
            var iterator = (_m00 * invMatrix[0, 0]) + (_m01 * invMatrix[1, 0]) + (_m02 * invMatrix[2, 0]);

            if (Mathf.Abs(iterator) <= Epsilon)
            {
                return new Matrix3x3(1, 0, 0, 0, 1, 0, 0, 0, 1);
            }

            float fInvDet = (float)(1.0f / iterator);
            invMatrix[0, 0] *= fInvDet; invMatrix[0, 1] *= fInvDet; invMatrix[0, 2] *= fInvDet;
            invMatrix[1, 0] *= fInvDet; invMatrix[1, 1] *= fInvDet; invMatrix[1, 2] *= fInvDet;
            invMatrix[2, 0] *= fInvDet; invMatrix[2, 1] *= fInvDet; invMatrix[2, 2] *= fInvDet;
            return invMatrix;
        }

        public Matrix2x2 Cofactor()
        {
            return new Matrix2x2(this[1, 1], -this[1, 0], -this[0, 1], this[0, 0]);
        }

        public float Determinant()
        {
            return (this[0, 0] * (this[1, 1] * this[2, 2])) - (this[1, 2] * this[2, 1])
                - (this[0, 1] * ((this[1, 0] * this[2, 2]) - (this[1, 2] * this[2, 0])))
                + (this[0, 2] * ((this[1, 0] * this[2, 1]) - (this[1, 1] * this[2, 0])));
        }

        public float FrobeniusInnerProduct(Matrix2x2 m)
        {
            float prod = 0;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    prod += this[i, j] * m[i, j];
                }
            }
            return prod;
        }

        //Matrix * Matrix
        public void DiagProduct(Vector3 v)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                    this[i, j] *= v[i];
            }
        }

        //Matrix * Matrix^-1
        public void DiagProductInv(Vector3 v)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                    this[i, j] /= v[i];
            }
        }

        //Matrix - Matrix
        public void DiagDifference(float c)
        {
            for (int i = 0; i < 3; i++)
                this[i, i] -= c;
        }

        public void DiagDifference(Vector3 v)
        {
            for (int i = 0; i < 3; i++)
                this[i, i] -= v[i];
        }

        //Matrix + Matrix
        public void DiagSum(float c)
        {
            for (int i = 0; i < 3; i++)
                this[i, i] += c;
        }

        public void DiagSum(Vector3 v)
        {
            for (int i = 0; i < 3; i++)
                this[i, i] += v[i];
        }

        public void LoadIdentity()
        {
            SetValue(1, 0, 0, 0, 1, 0, 0, 0, 1);
        }

        public static Matrix3x3 Identity()
        {
            return new Matrix3x3(1, 0, 0, 0, 1, 0, 0, 0, 1);
        }

    }
}