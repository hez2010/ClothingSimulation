using Assets.Utils;
using UnityEngine;

namespace Assets.Collisions
{
    class BoxCollisionObject : CollisionObject, IHasCollider
    {
        private readonly BoxCollider _collider;
        public BoxCollisionObject(BoxCollider collider)
        {
            _collider = collider;
        }

        public Collider Collider => _collider;

        public override bool Hit(int index, Vector3 position, Vector3 velocity, float time, out CollisionResult? result)
        {
            var localPosition = _collider.transform.InverseTransformPoint(position);
            var localVelocity = _collider.transform.InverseTransformVector(velocity);

            if (HitDetector.HitBox(
                _collider.transform.InverseTransformPoint(_collider.bounds.min), 
                _collider.transform.InverseTransformPoint(_collider.bounds.max),
                localPosition,
                localVelocity,
                time,
                out var axis,
                out var reachTime))
            {
                result = new CollisionResult(index, this, position + (reachTime * velocity), axis switch
                {
                    0 => new Vector3(1, 0, 0),
                    1 => new Vector3(0, 1, 0),
                    2 => new Vector3(0, 0, 1),
                    _ => default
                }, reachTime);

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
