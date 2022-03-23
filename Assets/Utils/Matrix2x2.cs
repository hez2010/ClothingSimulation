using UnityEngine;

namespace Assets.Utils
{
    struct Matrix2x2
    {
        public const float Epsilon = 1e-6f;
        private float _m00, _m01, _m10, _m11;

        public Matrix2x2(float m00, float m01, float m10, float m11)
        {
            _m00 = m00;
            _m01 = m01;
            _m10 = m10;
            _m11 = m11;
        }

        public Matrix2x2(Matrix2x2 matrix)
        {
            _m00 = matrix[0];
            _m01 = matrix[1];
            _m10 = matrix[2];
            _m11 = matrix[3];
        }

        public float this[int i, int j]
        {
            get => (i, j) switch
            {
                (0, 0) => _m00,
                (0, 1) => _m01,
                (1, 0) => _m10,
                (1, 1) => _m11,
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
                    case (1, 0):
                        _m10 = value;
                        break;
                    case (1, 1):
                        _m11 = value;
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
                2 => _m10,
                3 => _m11,
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
                        _m10 = value;
                        break;
                    case 3:
                        _m11 = value;
                        break;
                }
            }
        }
        public void SetValue(float m00, float m01, float m10, float m11)
        {
            this[0, 0] = m00;
            this[0, 1] = m01;
            this[1, 0] = m10;
            this[1, 1] = m11;
        }

        public void SetValue(float m00, float m11)
        {
            SetValue(m00, 0, 0, m11);
        }

        public void SetValue(Matrix2x2 m)
        {
            SetValue(m[0, 0], m[0, 1], m[1, 0], m[1, 1]);
        }

        public void SetValue(float value)
        {
            SetValue(value, value, value, value);
        }


        public void Normalize()
        {
            for (int row = 0; row < 2; row++)
            {
                float l = 0;
                for (int column = 0; column < 2; column++)
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

        public Matrix2x2 Transpose()
        {
            return new Matrix2x2(this[0, 0], this[1, 0], this[0, 1], this[1, 1]);
        }

        public Matrix2x2 Inverse()
        {
            float det = Determinant();
            return new Matrix2x2(this[1, 1] / det, -this[0, 1] / det, -this[1, 0] / det, this[0, 0] / det);
        }

        public Matrix2x2 Cofactor()
        {
            return new Matrix2x2(this[1, 1], -this[1, 0], -this[0, 1], this[0, 0]);
        }

        public float Determinant()
        {
            return (this[0, 0] * this[1, 1]) - (this[0, 1] * this[1, 0]);
        }

        public float FrobeniusInnerProduct(Matrix2x2 m)
        {
            float prod = 0;
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    prod += this[i, j] * m[i, j];
                }
            }
            return prod;
        }

        //Matrix * Matrix
        public void DiagProduct(Vector2 v)
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                    this[i, j] *= v[i];
            }
        }

        //Matrix * Matrix^-1
        public void DiagProductInv(Vector2 v)
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                    this[i, j] /= v[i];
            }
        }

        //Matrix - Matrix
        public void DiagDifference(float c)
        {
            for (int i = 0; i < 2; i++)
                this[i, i] -= c;
        }

        public void DiagDifference(Vector2 v)
        {
            for (int i = 0; i < 2; i++)
                this[i, i] -= v[i];
        }

        //Matrix + Matrix
        public void DiagSum(float c)
        {
            for (int i = 0; i < 2; i++)
                this[i, i] += c;
        }

        public void DiagSum(Vector2 v)
        {
            for (int i = 0; i < 2; i++)
                this[i, i] += v[i];
        }

        public void LoadIdentity()
        {
            SetValue(1, 0, 0, 1);
        }

        public static Matrix2x2 Identity()
        {
            return new Matrix2x2(1, 0, 0, 1);
        }

        public void SVD(ref Matrix2x2 w, ref Matrix2x2 e, ref Matrix2x2 v)
        {
            // If it is diagonal, SVD is trivial
            if (Mathf.Abs(this[1, 0] - this[0, 1]) < Epsilon && Mathf.Abs(this[1, 0]) < Epsilon)
            {
                w.SetValue(this[0, 0] < 0 ? -1 : 1, 0, 0, this[1, 1] < 0 ? -1 : 1);
                e.SetValue(Mathf.Abs(this[0, 0]), Mathf.Abs(this[1, 1]));
                v.LoadIdentity();
            }

            // Otherwise, we need to compute A^T*A
            else
            {
                float j = (this[0, 0] * this[0, 0]) + (this[1, 0] * this[1, 0]),
                    k = (this[0, 1] * this[0, 1]) + (this[1, 1] * this[1, 1]),
                    v_c = (this[0, 0] * this[0, 1]) + (this[1, 0] * this[1, 1]);
                // Check to see if A^T*A is diagonal
                if (Mathf.Abs(v_c) < Epsilon)
                {
                    float s1 = Mathf.Sqrt(j), s2 = Mathf.Abs(j - k) < Epsilon ? s1 : Mathf.Sqrt(k);
                    e.SetValue(s1, s2);
                    v.LoadIdentity();
                    w.SetValue(this[0, 0] / s1, this[0, 1] / s2, this[1, 0] / s1, this[1, 1] / s2);
                }
                // Otherwise, solve quadratic for eigenvalues
                else
                {
                    float jmk = j - k,
                        jpk = j + k,
                        root = Mathf.Sqrt((jmk * jmk) + (4 * v_c * v_c)),
                        eig = (jpk + root) / 2,
                        s1 = Mathf.Sqrt(eig),
                        s2 = Mathf.Abs(root) < Epsilon ? s1 : Mathf.Sqrt((jpk - root) / 2);

                    e.SetValue(s1, s2);

                    // Use eigenvectors of A^T*A as V
                    float v_s = eig - j, len = Mathf.Sqrt((v_s * v_s) + (v_c * v_c));
                    v_c /= len;
                    v_s /= len;
                    v.SetValue(v_c, -v_s, v_s, v_c);
                    // Compute w matrix as Av/s
                    w.SetValue(
                        ((this[0, 0] * v_c) + (this[0, 1] * v_s)) / s1,
                        ((this[0, 1] * v_c) - (this[0, 0] * v_s)) / s2,
                        ((this[1, 0] * v_c) + (this[1, 1] * v_s)) / s1,
                        ((this[1, 1] * v_c) - (this[1, 0] * v_s)) / s2
                    );
                }
            }
        }

        public static Matrix2x2 operator +(Matrix2x2 left, float right)
        {
            var result = new Matrix2x2(left);
            for (int index = 0; index < 4; index++)
            {
                result[index] += right;
            }
            return result;
        }

        public static Matrix2x2 operator +(float left, Matrix2x2 right)
        {
            var result = new Matrix2x2(right);
            for (int index = 0; index < 4; index++)
            {
                result[index] += left;
            }
            return result;
        }

        public static Matrix2x2 operator -(Matrix2x2 left, float right)
        {
            var result = new Matrix2x2(left);
            for (int index = 0; index < 4; index++)
            {
                result[index] -= right;
            }
            return result;
        }

        public static Matrix2x2 operator *(Matrix2x2 left, float right)
        {
            var result = new Matrix2x2(left);
            for (int index = 0; index < 4; index++)
            {
                result[index] *= right;
            }
            return result;
        }

        public static Matrix2x2 operator *(float left, Matrix2x2 right)
        {
            var result = new Matrix2x2(right);
            for (int index = 0; index < 4; index++)
            {
                result[index] *= left;
            }
            return result;
        }

        public static Matrix2x2 operator /(Matrix2x2 left, float right)
        {
            var result = new Matrix2x2(left);
            for (int index = 0; index < 4; index++)
            {
                result[index] /= right;
            }
            return result;
        }

        public static Matrix2x2 operator +(Matrix2x2 left, Matrix2x2 right)
        {
            var result = new Matrix2x2(left);
            for (int row = 0; row < 2; row++)
            {
                for (int column = 0; column < 2; column++)
                {
                    result[row, column] += right[row, column];
                }
            }
            return result;
        }

        public static Matrix2x2 operator -(Matrix2x2 left, Matrix2x2 right)
        {
            var result = new Matrix2x2(left);
            for (int row = 0; row < 2; row++)
            {
                for (int column = 0; column < 2; column++)
                {
                    result[row, column] -= right[row, column];
                }
            }
            return result;
        }

        public static Matrix2x2 operator *(Matrix2x2 left, Matrix2x2 right)
        {
            var result = new Matrix2x2(left);
            for (int row = 0; row < 2; row++)
            {
                for (int column = 0; column < 2; column++)
                {

                    result[row, column] = left[row, 0] * right[0, column];
                    for (int i = 1; i < 2; i++)
                    {
                        result[row, column] += left[row, i] * right[i, column];
                    }

                }
            }
            return result;
        }

        public static Vector2 operator *(Matrix2x2 left, Vector2 right)
        {
            return new Vector2(
                    (left[0, 0] * right[0]) + (left[0, 1] * right[1]),
                    (left[1, 0] * right[0]) + (left[1, 1] * right[1])
                );
        }
    }
}