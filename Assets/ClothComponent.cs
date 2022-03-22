using Assets.Contraints;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    class ClothComponent
    {
        public Vector3[] Positions { get; }
        public Vector3[] NewPositions { get; }
        public Vector3[] Velocities { get; }
        public Vector3[] NoDampingVelocities { get; }
        public List<ConstraintBase> Constraints { get; }
        public float Damping { get; set; }
        public float Mass { get; set; }
        public int[] Triangles { get; set; }
        public (Vector3 Vec, int Count)[] Delta { get; set; }

        public ClothComponent(float damping, float mass, Vector3[] vertices, int[] triangles)
        {
            Positions = new Vector3[vertices.Length];
            NewPositions = new Vector3[vertices.Length];
            Velocities = new Vector3[vertices.Length];
            NoDampingVelocities = new Vector3[vertices.Length];
            Constraints = new List<ConstraintBase>();
            Delta = new (Vector3 Vec, int Count)[vertices.Length];
            Damping = damping;
            Mass = mass;
            Triangles = triangles;

            for (var i = 0; i < vertices.Length; i++)
            {
                NewPositions[i] = Positions[i] = vertices[i];
            }
        }

        public void AddDistanceConstraints()
        {
            var count = Triangles.Length / 3;
            var edges = new HashSet<(int A, int B)>();

            for (var i = 0; i < count; i++)
            {
                var t1 = Triangles[i * 3];
                var t2 = Triangles[(i * 3) + 1];
                var t3 = Triangles[(i * 3) + 2];
                edges.Add(t1 > t2 ? (t2, t1) : (t1, t2));
                edges.Add(t2 > t3 ? (t3, t2) : (t2, t3));
                edges.Add(t1 > t3 ? (t3, t1) : (t1, t3));
            }

            foreach (var (a, b) in edges)
            {
                Constraints.Add(new DistanceConstraint(this, 1.0f, a, b));
            }
        }

        //public void AddFEMTriangleConstraints()
        //{
        //    var count = Triangles.Length / 3;

        //    for (var i = 0; i < count; i++)
        //    {
        //        Constraints.Add(new FEMTriangleConstraint(this, 1.0f, 0.3f, (Triangles[i * 3], Triangles[(i * 3) + 1], Triangles[(i * 3) + 2])));
        //    }
        //}

        public void Project(float dt)
        {
            Array.Clear(Delta, 0, Delta.Length);
            foreach (var constraint in Constraints)
            {
                constraint.Resolve(dt);
            }
            for (var i = 0; i < Delta.Length; i++)
            {
                if (Delta[i].Count > 0)
                {
                    NewPositions[i] += Delta[i].Vec / Delta[i].Count;
                }
            }
        }
    }
}
