using UnityEngine;

namespace Assets.Collisions
{
    abstract class CollisionObject
    {
        public Bounds Bounds;
        public abstract Vector3 Velocity { get; }
        public abstract void UpdateBounds(float dt);
        public abstract bool Hit(Vector3 position, Vector3 velocity, float time, out CollisionResult? result);
    }
}
