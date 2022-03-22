using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Collisions
{
    class BVHCollisionObject : CollisionObject
    {
        public CollisionObject Left, Right;

        public BVHCollisionObject(CollisionObject[] colliders)
        {
            var axis = (int)MathF.Floor(Random.value * 3);
            switch (axis)
            {
                case 0:
                    Array.Sort(colliders, (a, b) => a.Bounds.min.x < b.Bounds.min.x ? -1 : a.Bounds.min.x == b.Bounds.min.x ? 0 : 1);
                    break;
                case 1:
                    Array.Sort(colliders, (a, b) => a.Bounds.min.y < b.Bounds.min.y ? -1 : a.Bounds.min.y == b.Bounds.min.y ? 0 : 1);
                    break;
                case 2:
                    Array.Sort(colliders, (a, b) => a.Bounds.min.z < b.Bounds.min.z ? -1 : a.Bounds.min.z == b.Bounds.min.z ? 0 : 1);
                    break;
            }

            if (colliders.Length == 1)
            {
                Left = Right = colliders[0];
            }
            else if (colliders.Length == 2)
            {
                Left = colliders[0];
                Right = colliders[1];
            }
            else
            {
                Left = new BVHCollisionObject(colliders[0..(colliders.Length / 2)]);
                Right = new BVHCollisionObject(colliders[(colliders.Length / 2)..colliders.Length]);
            }

            var leftBounds = Left.Bounds;
            var rightBounds = Right.Bounds;

            var center = (leftBounds.center + rightBounds.center) / 2;
            var size = new Vector3(
                Mathf.Max(leftBounds.max.x, rightBounds.max.x) - Mathf.Min(leftBounds.min.x, rightBounds.min.x),
                Mathf.Max(leftBounds.max.y, rightBounds.max.y) - Mathf.Min(leftBounds.min.y, rightBounds.min.y),
                Mathf.Max(leftBounds.max.z, rightBounds.max.z) - Mathf.Min(leftBounds.min.z, rightBounds.min.z)
            );

            Bounds = new Bounds(center, size);
        }

        public override bool Hit(int index, Vector3 position, Vector3 velocity, float time, out CollisionResult? result)
        {
            for (var a = 0; a < 3; a++)
            {
                var invD = 1.0f / velocity.normalized[a];
                var t0 = (Bounds.min[a] - position[a]) * invD;
                var t1 = (Bounds.max[a] - position[a]) * invD;
                if (invD < 0f)
                {
                    (t0, t1) = (t1, t0);
                }
                var tmin = Mathf.Max(0, t0);
                var tmax = Mathf.Min(time, t1);
                if (tmax <= tmin)
                {
                    result = null;
                    return false;
                }
            }

            var leftHit = Left.Hit(index, position, velocity, time, out var leftResult);
            var rightHit = Left.Hit(index, position, velocity, time, out var rightResult);

            if (leftHit && rightHit)
            {
                result = leftResult.Value.ReachTime < rightResult.Value.ReachTime ? leftResult : rightResult;
                return true;
            }
            else if (leftHit)
            {
                result = leftResult;
                return true;
            }
            else if (rightHit)
            {
                result = rightResult;
                return true;
            }

            result = null;
            return false;
        }
    }
}
