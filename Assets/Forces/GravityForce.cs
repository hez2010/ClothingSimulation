using UnityEngine;

namespace Assets.Forces
{
    class GravityForce : ForceBase
    {
        private static readonly Vector3 _force = new (0, -9.8f, 0);

        public GravityForce() { }

        public override void Apply(ClothComponent cloth, float dt)
        {
            for (var i = 0; i < cloth.Velocities.Length; i++)
            {
                cloth.Velocities[i] += _force * dt;
                cloth.NoDampingVelocities[i] += _force * dt;
            }
        }
    }
}
