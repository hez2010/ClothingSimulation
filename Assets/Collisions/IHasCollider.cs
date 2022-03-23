using UnityEngine;

namespace Assets.Collisions
{
    interface IHasCollider
    {
        Collider Collider { get; }
    }
}
