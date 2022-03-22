using UnityEngine;

namespace Assets.Forces
{
    class GlobalForce : ForceBase
    {
        private readonly Vector3 _force;
        public GlobalForce(Vector3 force)
        {
            _force = force;
        }

        public override void Apply(ClothComponent cloth, float dt)
        {
            for (var i = 0; i < cloth.Velocities.Length; i++)
            {
                cloth.Velocities[i] += _force * dt;
            }
        }
    }
}
