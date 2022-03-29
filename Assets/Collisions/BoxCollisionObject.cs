using Assets.Utils;
using UnityEngine;

namespace Assets.Collisions
{
    class BoxCollisionObject : CollisionObject
    {
        private readonly BoxCollider _collider;
        private Vector3 _velocity;
        private Vector3 _lastPosition;
        public BoxCollisionObject(BoxCollider collider)
        {
            _collider = collider;
            UpdateBounds(0);
        }

        public override Vector3 Velocity => _velocity;

        public override bool Hit(Vector3 position, Vector3 velocity, float time, out CollisionResult? result)
        {
            var localPosition = _collider.transform.InverseTransformPoint(position);
            var localVelocity = _collider.transform.InverseTransformVector(velocity);

            if (HitDetector.HitAABB(
                _collider.center - _collider.size / 2,
                _collider.center + _collider.size / 2,
                localPosition,
                localVelocity,
                time,
                out var normal,
                out var reachTime))
            {
                result = new CollisionResult(this, position + (reachTime * velocity), normal, reachTime);
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
