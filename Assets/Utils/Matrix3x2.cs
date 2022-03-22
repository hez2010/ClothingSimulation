namespace Assets.Utils
{
    class Matrix3x2
    {
        public const float Epsilon = 1e-6f;
        private float _m00, _m01, _m10, _m11, _m20, _m21;

        public Matrix3x2(float m00, float m01, float m10, float m11, float m20, float m21)
        {
            _m00 = m00;
            _m01 = m01;
            _m10 = m10;
            _m11 = m11;
            _m20 = m20;
            _m21 = m21;
        }

        public Matrix3x2(Matrix3x2 matrix)
        {
            _m00 = matrix[0];
            _m01 = matrix[1];
            _m10 = matrix[2];
            _m11 = matrix[3];
            _m20 = matrix[4];
            _m21 = matrix[5];
        }

        public float this[int i, int j]
        {
            get => (i, j) switch
            {
                (0, 0) => _m00,
                (0, 1) => _m01,
                (1, 0) => _m10,
                (1, 1) => _m11,
                (2, 0) => _m20,
                (2, 1) => _m21,
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
                    case (2, 0):
                        _m20 = value;
                        break;
                    case (2, 1):
                        _m21 = value;
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
                4 => _m20,
                5 => _m21,
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
                    case 4:
                        _m20 = value;
                        break;
                    case 5:
                        _m21 = value;
                        break;
                }
            }
        }

        public static Matrix3x2 operator *(Matrix3x2 left, Matrix2x2 right)
        {
            return new Matrix3x2(
                left[0, 0] * right[0, 0] + left[0, 1] * right[1, 0],
                left[0, 0] * right[0, 1] + left[0, 1] * right[1, 1],
                left[1, 0] * right[0, 0] + left[1, 1] * right[1, 0],
                left[1, 0] * right[0, 1] + left[1, 1] * right[1, 1],
                left[2, 0] * right[0, 0] + left[2, 1] * right[1, 0],
                left[2, 0] * right[0, 1] + left[2, 1] * right[1, 1]
            );
        }
        public static Matrix3x2 operator *(Matrix3x2 left, float right)
        {
            var m = new Matrix3x2(left);
            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 2; j++)
                {
                    m[i, j] *= right;
                }
            }
            return m;
        }
    }
}
