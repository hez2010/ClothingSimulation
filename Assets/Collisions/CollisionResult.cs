using UnityEngine;

namespace Assets.Collisions
{
    struct CollisionResult
    {
        public readonly CollisionObject Collider;
        public readonly Vector3 Position;
        public readonly Vector3 Normal;
        public readonly float ReachTime;

        public CollisionResult(CollisionObject collider, Vector3 position, Vector3 normal, float reachTime)
        {
            Collider = collider;
            Position = position;
            Normal = normal;
            ReachTime = reachTime;
        }
    }
}
