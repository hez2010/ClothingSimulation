using Assets.Collisions;
using Assets.Forces;
using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{
    class Simulator
    {
        private readonly int _collisionIterNum;
        private readonly int _simulationIterNum;
        private readonly Transform _transform;

        public readonly ClothComponent Cloth;
        public readonly List<ForceBase> Forces = new();
        public readonly CollisionObject Colliders;

        public Simulator(ClothComponent cloth, int simulationIterNum, int collisionIterNum, CollisionObject[] colliders, Transform transform)
        {
            _simulationIterNum = simulationIterNum;
            _collisionIterNum = collisionIterNum;
            _transform = transform;
            Cloth = cloth;
            Colliders = new BVHCollisionObject(colliders);
            Array.Clear(Cloth.NoDampingVelocities, 0, Cloth.NoDampingVelocities.Length);
        }

        class CollisionTaskState
        {
            public readonly ChannelReader<CollisionResult> Channel;
            public readonly TaskCompletionSource<bool> CompletionSource;
            public readonly Dictionary<int, CollisionResult> CollidedTriangles;

            public CollisionTaskState(ChannelReader<CollisionResult> channel, TaskCompletionSource<bool> completionSource, Dictionary<int, CollisionResult> collidedTriangles)
            {
                Channel = channel;
                CompletionSource = completionSource;
                CollidedTriangles = collidedTriangles;
            }
        }

        public void Simulate(float dt)
        {
            for (var i = 0; i < Cloth.Positions.Length; i++)
            {
                Cloth.Velocities[i] -= Cloth.Damping * dt * (Cloth.Velocities[i] - Cloth.NoDampingVelocities[i]);
            }
            Array.Clear(Cloth.NoDampingVelocities, 0, Cloth.NoDampingVelocities.Length);

            foreach (var force in Forces)
            {
                force.Apply(Cloth, dt);
            }

            for (var i = 0; i < Cloth.Positions.Length; i++)
            {
                Cloth.NewPositions[i] = Cloth.Positions[i] + (dt * Cloth.Velocities[i]);
            }

            for (var i = 0; i < _simulationIterNum; i++)
            {
                Cloth.Project(1.0f / _simulationIterNum);
            }

            Colliders.UpdateBounds();
            for (var i = 0; i < _collisionIterNum; i++)
            {
                var collisions = new List<CollisionResult>();
                for (var j = 0; j < Cloth.Positions.Length; j++)
                {
                    var p1 = _transform.TransformPoint(Cloth.NewPositions[j]);
                    var p2 = _transform.TransformPoint(Cloth.Positions[j]);
                    var dx = p1 - p2;
                    var v = dx / dt;

                    if (Colliders.Hit(j, p2, v, dt, out var result))
                    {
                        collisions.Add(result.Value);
                    }
                }

                for (var c = 0; c < collisions.Count; c++)
                {
                    var collision = collisions[c];
                    for (var t = 0; t < Cloth.AdjointTriangles[collision.Index].Length; t++)
                    {
                        var index = Cloth.AdjointTriangles[collision.Index][t];
                        Cloth.NewPositions[index] = Cloth.Positions[index];
                    }
                }
            }

            for (var i = 0; i < Cloth.Positions.Length; i++)
            {
                var dx = Cloth.NewPositions[i] - Cloth.Positions[i];
                Cloth.Velocities[i] = dx / dt;
                Cloth.Positions[i] = Cloth.NewPositions[i];
            }
        }
    }
}
