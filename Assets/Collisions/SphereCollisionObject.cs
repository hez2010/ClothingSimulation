using Assets.Utils;
using UnityEngine;

namespace Assets.Collisions
{
    class SphereCollisionObject : CollisionObject
    {
        private readonly SphereCollider _collider;
        public SphereCollisionObject(SphereCollider collider)
        {
            _collider = collider;
            UpdateBounds();
        }

        public override bool Hit(Vector3 position, Vector3 velocity, float time, out CollisionResult? result)
        {
            var localPosition = _collider.transform.InverseTransformPoint(position);
            var localVelocity = _collider.transform.InverseTransformVector(velocity);

            if (HitDetector.HitSphere(_collider.center, _collider.radius, localPosition, localVelocity, time, out var reachTime))
            {
                var hitPosition = position + (velocity * reachTime);
                var localHitPosition = localPosition + (localVelocity * reachTime);
                result = new CollisionResult(this, hitPosition, _collider.transform.TransformDirection((localHitPosition - _collider.center) / _collider.radius), reachTime);
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
