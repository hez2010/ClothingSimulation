using UnityEngine;

namespace Assets.Forces
{
    class LocalForce : ForceBase
    {
        private readonly int _index;
        private readonly Vector3 _force;
        public LocalForce(int index, Vector3 force)
        {
            _force = force;
            _index = index;
        }

        public override void Apply(ClothComponent cloth, float dt)
        {
            cloth.Velocities[_index] += _force * dt;
        }
    }
}
