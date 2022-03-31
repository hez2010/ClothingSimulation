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
            var ray = new Ray(position, velocity.normalized);
            if (_collider.Raycast(ray, out var hitInfo, time * velocity.magnitude))
            {
                result = new CollisionResult(this, hitInfo.point, hitInfo.normal, (hitInfo.point - position).magnitude / velocity.magnitude);
                return true;
            }

            var min = _collider.center - _collider.size * 0.5f;
            var dx = new Vector3(_collider.size.x, 0, 0);
            var dy = new Vector3(0, _collider.size.y, 0);
            var dz = new Vector3(0, 0, _collider.size.z);
            var ax = dx.normalized.WithLength(dx.magnitude);
            var ay = dy.normalized.WithLength(dy.magnitude);
            var az = dz.normalized.WithLength(dz.magnitude);

            if (HitDetector.InsideAABB(min, ax, ay, az, localPosition) &&
                HitDetector.GetAABBClosestSurfacePoint(min, ax, ay, az, localPosition, out var n, out var p))
            {
                result = new CollisionResult(this, _collider.transform.TransformPoint(p), _collider.transform.TransformDirection(n), 0);
                return true;
            }

            //if (HitDetector.HitAABB(
            //    _collider.center - (_collider.size / 2),
            //    _collider.center + (_collider.size / 2),
            //    localPosition,
            //    localVelocity,
            //    time,
            //    out var normal,
            //    out var reachTime))
            //{
            //    result = new CollisionResult(this, position + (reachTime * velocity), normal, reachTime);
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
