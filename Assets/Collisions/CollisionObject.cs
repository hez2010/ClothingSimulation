using UnityEngine;

namespace Assets.Collisions
{
    abstract class CollisionObject
    {
        public Bounds Bounds;
        public abstract bool Hit(int index, Vector3 position, Vector3 velocity, float time, out CollisionResult? result);
    }
}
