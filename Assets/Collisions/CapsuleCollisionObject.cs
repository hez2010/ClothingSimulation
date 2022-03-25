using Assets.Utils;
using UnityEngine;

namespace Assets.Collisions
{
    class CapsuleCollisionObject : CollisionObject
    {
        private readonly CapsuleCollider _collider;
        public CapsuleCollisionObject(CapsuleCollider collider)
        {
            _collider = collider;
        }

        public override bool Hit(Vector3 position, Vector3 velocity, float time, out CollisionResult? result)
        {
            var localPosition = _collider.transform.InverseTransformPoint(position);
            var localVelocity = _collider.transform.InverseTransformVector(velocity);

            var cHeight = _collider.height - (_collider.radius * 2);
            var pA = _collider.center;
            var pB = _collider.center;
            pA[_collider.direction] += cHeight / 2;
            pB[_collider.direction] -= cHeight / 2;
            if (HitDetector.HitCapsule(pA, pB, _collider.radius, localPosition, localVelocity, time, out var normal, out var reachTime))
            {
                result = new CollisionResult(this, position + (velocity * reachTime), _collider.transform.TransformDirection(normal), reachTime);
                return true;
            }

            result = null;
            return false;
        }

        public override void UpdateBounds()
        {
            Bounds = new Bounds(_collider.bounds.center, _collider.bounds.size);
        }
    }
}
