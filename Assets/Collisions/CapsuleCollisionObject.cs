using Assets.Utils;
using UnityEngine;

namespace Assets.Collisions
{
    class CapsuleCollisionObject : CollisionObject
    {
        private readonly CapsuleCollider _collider;
        private Vector3 _velocity;
        private Vector3 _lastPosition;
        public CapsuleCollisionObject(CapsuleCollider collider)
        {
            _collider = collider;
            UpdateBounds(0);
        }

        public override Vector3 Velocity => _velocity;

        public override bool Hit(Vector3 position, Vector3 velocity, float time, out CollisionResult? result)
        {
            var localPosition = _collider.transform.InverseTransformPoint(position);
            var localVelocity = _collider.transform.InverseTransformVector(velocity);

            var ray = new Ray(position, velocity.normalized);
            if (_collider.Raycast(ray, out var hitInfo, time * velocity.magnitude))
            {
                result = new CollisionResult(this, hitInfo.point, hitInfo.normal, (hitInfo.point - position).magnitude / velocity.magnitude);
                return true;
            }

            //var cHeight = _collider.height - (_collider.radius * 2);
            //var pA = _collider.center;
            //var pB = _collider.center;
            //pA[_collider.direction] += cHeight / 2;
            //pB[_collider.direction] -= cHeight / 2;
            //if (HitDetector.HitCapsule(pA, pB, _collider.radius, localPosition, localVelocity, time, out var normal, out var reachTime))
            //{
            //    result = new CollisionResult(this, position + (velocity * reachTime), _collider.transform.TransformDirection(normal), reachTime);
            //    return true;
            //}

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
