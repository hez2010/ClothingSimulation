using Assets.Contraints;
using Assets.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets
{
    class ClothComponent
    {
        public readonly Vector3[] Positions;
        public readonly Vector3[] Predicts;
        public readonly Vector3[] Velocities;
        public readonly Vector3[] NoDampingVelocities;
        public readonly float[] Masses;
        public readonly List<ConstraintBase> Constraints;
        public readonly float Damping;
        public readonly int[] Triangles;

        public ClothComponent(float damping, float stiffness, Vector3[] vertices, int[] triangles)
        {
            Positions = new Vector3[vertices.Length];
            Predicts = new Vector3[vertices.Length];
            Velocities = new Vector3[vertices.Length];
            NoDampingVelocities = new Vector3[vertices.Length];
            Constraints = new List<ConstraintBase>();
            Damping = damping;
            Triangles = triangles;
            Masses = new float[vertices.Length];

            //var count = triangles.Length / 3;
            //for (var i = 0; i < vertices.Length; i++)
            //{
            //    var adjointTriangles = new HashSet<int>();
            //    for (var j = 0; j < count; j++)
            //    {
            //        if (i == triangles[j * 3] ||
            //            i == triangles[(j * 3) + 1] ||
            //            i == triangles[(j * 3) + 2])
            //        {
            //            adjointTriangles.Add(triangles[j * 3]);
            //            adjointTriangles.Add(triangles[(j * 3) + 1]);
            //            adjointTriangles.Add(triangles[(j * 3) + 2]);
            //        }
            //    }
            //    AdjointTriangles[i] = adjointTriangles.ToArray();
            //}

            for (var i = 0; i < vertices.Length; i++)
            {
                Predicts[i] = Positions[i] = vertices[i];
            }

            ComputeMasses();
            //AddBendingConstraints();
            AddDistanceConstraints(stiffness);
        }

        private void ComputeMasses()
        {
            var count = Triangles.Length / 3;
            for (var i = 0; i < count; i++)
            {
                var p1 = Positions[Triangles[i * 3]];
                var p2 = Positions[Triangles[(i * 3) + 1]];
                var p3 = Positions[Triangles[(i * 3) + 2]];
                var area = (p2 - p1).Cross(p3 - p1).magnitude / 2;
                var m = area / 3;
                Masses[Triangles[i * 3]] = m;
                Masses[Triangles[(i * 3) + 1]] = m;
                Masses[Triangles[(i * 3) + 2]] = m;
            }
        }

        private void AddDistanceConstraints(float stiffness)
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
                Constraints.Add(new DistanceConstraint(this, stiffness, a, b));
            }
        }

        //private void AddBendingConstraints()
        //{
        //    var count = Triangles.Length / 3;
        //    var dict = new Dictionary<(int A, int B), int>();
        //    var edges = new HashSet<(int A, int B)>();

        //    for (var i = 0; i < count; i++)
        //    {
        //        var t1 = Triangles[i * 3];
        //        var t2 = Triangles[(i * 3) + 1];
        //        var t3 = Triangles[(i * 3) + 2];
        //        dict[(t1, t2)] = t3;
        //        dict[(t1, t3)] = t2;
        //        dict[(t2, t3)] = t1;
        //        edges.Add(t1 > t2 ? (t2, t1) : (t1, t2));
        //        edges.Add(t2 > t3 ? (t3, t2) : (t2, t3));
        //        edges.Add(t1 > t3 ? (t3, t1) : (t1, t3));
        //    }

        //    foreach (var (a, b) in edges)
        //    {
        //        if (dict.ContainsKey((a, b)) && dict.ContainsKey((b, a)))
        //        {
        //            var c = dict[(a, b)];
        //            var d = dict[(b, a)];
        //            Constraints.Add(new BendingConstraint(this, 1.0f, a, b, c, d));
        //        }
        //    }
        //}

        //public void AddFEMTriangleConstraints()
        //{
        //    var count = Triangles.Length / 3;

        //    for (var i = 0; i < count; i++)
        //    {
        //        Constraints.Add(new FEMTriangleConstraint(this, 1.0f, 0.3f, (Triangles[i * 3], Triangles[(i * 3) + 1], Triangles[(i * 3) + 2])));
        //    }
        //}
    }
}
