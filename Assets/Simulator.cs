using Assets.Collisions;
using Assets.Forces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{
    class Simulator
    {
        private readonly int _collisionIterNum;
        private readonly int _simulationIterNum;

        public readonly ClothComponent Cloth;
        public readonly List<ForceBase> Forces = new();
        public readonly CollisionObject Colliders;

        private const int _collisionTimeout = 1000;

        public Simulator(ClothComponent cloth, int simulationIterNum, int collisionIterNum, CollisionObject[] colliders)
        {
            _simulationIterNum = simulationIterNum;
            _collisionIterNum = collisionIterNum;
            Cloth = cloth;
            Colliders = new BVHCollisionObject(colliders);
            Array.Clear(Cloth.NoDampingVelocities, 0, Cloth.NoDampingVelocities.Length);
        }

        class CollisionTaskState
        {
            public readonly ChannelReader<CollisionResult> Channel;
            public readonly TaskCompletionSource<bool> CompletionSource;
            public readonly CancellationToken CancellationToken;
            public readonly HashSet<int> CollidedTriangles;

            public CollisionTaskState(ChannelReader<CollisionResult> channel, TaskCompletionSource<bool> completionSource, CancellationToken cancellationToken, HashSet<int> collidedTriangles)
            {
                Channel = channel;
                CompletionSource = completionSource;
                CancellationToken = cancellationToken;
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


            for (var i = 0; i < _collisionIterNum; i++)
            {
                var channel = Channel.CreateUnbounded<CollisionResult>();
                var tasks = new List<Task>();
                for (var j = 0; j < Cloth.Positions.Length; j++)
                {
                    var index = j;
                    tasks.Add(Task.Run(async () =>
                    {
                        var dx = Cloth.NewPositions[index] - Cloth.Positions[index];
                        var v = dx / dt;
                        if (Colliders.Hit(index, Cloth.Positions[index], v, dt, out var result))
                        {
                            await channel.Writer.WriteAsync(result.Value);
                        }
                    }));
                }

                var tcs = new TaskCompletionSource<bool>();
                Task.WhenAll(tasks).ContinueWith(_ => channel.Writer.Complete());

                using var cts = new CancellationTokenSource();
                var collidedTriangles = new HashSet<int>();

                if (ThreadPool.UnsafeQueueUserWorkItem(async state =>
                {
                    if (state is CollisionTaskState ctState)
                    {
                        try
                        {
                            while (await ctState.Channel.WaitToReadAsync(ctState.CancellationToken))
                            {
                                var collision = await ctState.Channel.ReadAsync(ctState.CancellationToken);
                                var count = Cloth.Triangles.Length / 3;
                                for (var t = 0; t < count; t++)
                                {
                                    if (Cloth.Triangles[t * 3] == collision.Index ||
                                        Cloth.Triangles[(t * 3) + 1] == collision.Index ||
                                        Cloth.Triangles[(t * 3) + 2] == collision.Index)
                                    {
                                        ctState.CollidedTriangles.Add(Cloth.Triangles[t * 3]);
                                        ctState.CollidedTriangles.Add(Cloth.Triangles[(t * 3) + 1]);
                                        ctState.CollidedTriangles.Add(Cloth.Triangles[(t * 3) + 2]);
                                    }
                                }
                            }
                            ctState.CompletionSource.SetResult(false);
                        }
                        catch (TaskCanceledException)
                        {
                            // ignored
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError(ex.ToString());
                        }
                    }
                }, new CollisionTaskState(channel.Reader, tcs, cts.Token, collidedTriangles)))
                {
                    if (!tcs.Task.Wait(_collisionTimeout))
                    {
                        cts.Cancel();
                    }

                    foreach (var triangle in collidedTriangles)
                    {
                        Cloth.NewPositions[triangle] = Cloth.Positions[triangle];
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
