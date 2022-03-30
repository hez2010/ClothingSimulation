using Assets.Collisions;
using Assets.Contraints;
using Assets.Forces;
using Assets.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{
    class Simulator
    {
        private readonly int _simulationIterNum;
        private readonly Transform _transform;

        public readonly ClothComponent Cloth;
        public readonly List<ForceBase> Forces = new();
        public readonly CollisionObject Colliders;

        public Simulator(ClothComponent cloth, int simulationIterNum, CollisionObject[] colliders, Transform transform)
        {
            _simulationIterNum = simulationIterNum;
            _transform = transform;
            Cloth = cloth;
            Colliders = new BVHCollisionObject(colliders);
            Array.Clear(Cloth.NoDampingVelocities, 0, Cloth.NoDampingVelocities.Length);
        }

        private void DampingVelocities(float dt)
        {
            for (var i = 0; i < Cloth.Positions.Length; i++)
            {
                Cloth.Velocities[i] -= Cloth.Damping * dt * (Cloth.Velocities[i] - Cloth.NoDampingVelocities[i]);
            }
            Array.Clear(Cloth.NoDampingVelocities, 0, Cloth.NoDampingVelocities.Length);
        }

        private void PredictPositions(float dt)
        {
            foreach (var force in Forces)
            {
                force.Apply(Cloth, dt);
            }

            for (var i = 0; i < Cloth.Positions.Length; i++)
            {
                Cloth.Predicts[i] = Cloth.Positions[i] + (dt * Cloth.Velocities[i]);
            }
        }

        private void SolveConstraints()
        {
            for (var i = 0; i < _simulationIterNum; i++)
            {
                foreach (var constraint in Cloth.Constraints)
                {
                    constraint.Resolve(1.0f /*/ _simulationIterNum*/);
                }
            }
            Cloth.Constraints.RemoveAll(c => c is CollisionConstraint);
        }

        private void SolveCollisions(float dt)
        {
            Colliders.UpdateBounds(dt);
            for (var j = 0; j < Cloth.Positions.Length; j++)
            {
                var p1 = _transform.TransformPoint(Cloth.Predicts[j]);
                var p2 = _transform.TransformPoint(Cloth.Positions[j]);
                var dx = p1 - p2;
                var v = dx / dt;

                if (Colliders.Hit(p2, v, dt, out var result))
                {
                    var position = _transform.InverseTransformPoint(result.Value.Position);
                    var normal = _transform.InverseTransformDirection(result.Value.Normal);
                    var velocity = _transform.InverseTransformVector(result.Value.Collider.Velocity);

                    Cloth.Constraints.Add(new CollisionConstraint(Cloth, j, position, normal, velocity));
                }
            }
        }

        public void Simulate(float dt)
        {
            DampingVelocities(dt);
            PredictPositions(dt);
            SolveCollisions(dt);
            SolveConstraints();

            for (var i = 0; i < Cloth.Positions.Length; i++)
            {
                if (Cloth.UpdatedVelocities[i])
                {
                    Cloth.UpdatedVelocities[i] = false;
                }
                else
                {
                    var dx = Cloth.Predicts[i] - Cloth.Positions[i];
                    Cloth.Velocities[i] = dx / dt;
                }
                Cloth.Positions[i] = Cloth.Predicts[i];
            }
        }
    }
}
