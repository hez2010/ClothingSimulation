using Assets.Utils;
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

            switch (colliders.Length)
            {
                case 0:
                    return;
                case 1:
                    Left = Right = colliders[0];
                    break;
                case 2:
                    Left = colliders[0];
                    Right = colliders[1];
                    break;
                default:
                    Left = new BVHCollisionObject(colliders[0..(colliders.Length / 2)]);
                    Right = new BVHCollisionObject(colliders[(colliders.Length / 2)..colliders.Length]);
                    break;
            }

            UpdateBounds();
        }

        public override bool Hit(int index, Vector3 position, Vector3 velocity, float time, out CollisionResult? result)
        {
            if (!HitDetector.HitBox(Bounds.min, Bounds.max, position, velocity, time, out _, out _))
            {
                result = null;
                return false;
            }

            var leftHit = Left.Hit(index, position, velocity, time, out var leftResult);
            var rightHit = Right.Hit(index, position, velocity, time, out var rightResult);

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

        public override void UpdateBounds()
        {
            Left.UpdateBounds();
            Right.UpdateBounds();

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
    }
}
