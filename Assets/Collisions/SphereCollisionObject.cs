using Assets.Utils;
using UnityEngine;

namespace Assets.Collisions
{
    class SphereCollisionObject : CollisionObject
    {
        private readonly SphereCollider _collider;
        private Vector3 _velocity;
        private Vector3 _lastPosition;

        public SphereCollisionObject(SphereCollider collider)
        {
            _collider = collider;
            UpdateBounds(0);
        }

        public override Vector3 Velocity => _velocity;

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

            if (HitDetector.InsideSphere(_collider.center, _collider.radius, localPosition) &&
                HitDetector.GetSphereClosestSurfacePoint(_collider.center, _collider.radius, localPosition, out var n, out var p))
            {
                result = new CollisionResult(this, _collider.transform.TransformPoint(p), _collider.transform.TransformDirection(n), 0);
                return true;
            }

            result = null;
            return false;
        }

        public override void UpdateBounds(float dt)
        {
            if (dt == 0)
            {
                _velocity = default;
            }
            else
            {
                _velocity = _collider.bounds.center - _lastPosition;
            }

            Bounds = new Bounds(_collider.bounds.center, _collider.bounds.size);
            _lastPosition = _collider.bounds.center;
        }
    }
}
