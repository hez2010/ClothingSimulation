using UnityEngine;

namespace Assets.Utils
{
    static class VectorExtensions
    {
        public static Vector3 Cross(this Vector3 left, Vector3 right)
        {
            var x = (left.y * right.z) - (left.z * right.y);
            var y = (left.z * right.x) - (left.x * right.z);
            var z = (left.x * right.y) - (left.y * right.x);
            var v = new Vector3(x, y, z);
            return v;
        }

        public static float Norm(this Vector3 vec)
        {
            return Mathf.Sqrt((vec.x * vec.x) + (vec.y * vec.y) + (vec.z * vec.z));
        }

        public static float Dot(this Vector3 left, Vector3 right)
        {
            return (left.x * right.x) + (left.y * right.y) + (left.z * right.z);
        }

        public static Vector3 GetDirection(this Vector4 vec)
        {
            return new Vector3(vec.x, vec.y, vec.z);
        }
    }
}
