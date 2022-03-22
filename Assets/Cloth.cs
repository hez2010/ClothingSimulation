using Assets;
using Assets.Collisions;
using Assets.Contraints;
using Assets.Forces;
using System.Collections.Generic;
using UnityEngine;

public class Cloth : MonoBehaviour
{
    private MeshFilter meshFilter;
    private Simulator simulator;

    void Start()
    {
        Application.targetFrameRate = 60;
        meshFilter = GetComponent<MeshFilter>();
        var gen = new TestMeshGenerator();
        var mesh = gen.Generate(20, 12, new(-10, 10, -10));
        meshFilter.mesh = mesh;
        var cloth = new ClothComponent(1f, 0.25f, mesh.vertices, mesh.triangles);
        cloth.AddDistanceConstraints();
        // cloth.AddFEMTriangleConstraints();
        var colliders = new List<CollisionObject>();
        var objects = FindObjectsOfType<GameObject>();
        foreach (var obj in objects)
        {
            if (obj.name.StartsWith("Sphere"))
            {
                var position = transform.InverseTransformPoint(obj.transform.position);
                var size = obj.GetComponent<MeshRenderer>().bounds.size.x / 2;
                colliders.Add(new SphereCollisionObject(position, size));
            }
        }

        simulator = new(cloth, 3, 2, colliders.ToArray());
        simulator.Forces.Add(new GravityForce());
    }

    void Update()
    {
        foreach (var c in simulator.Cloth.Constraints)
        {
            if (c is FixedPointConstraint fc)
            {
                fc.Position.z += Mathf.Sin(Time.time) / 4;
                fc.Position.x -= Mathf.Sin(Time.time) / 4;
            }
        }

        for (var i = 0; i < 3; i++)
        {
            simulator.Simulate(1.0f / 60 / 3);
        }

        meshFilter.mesh.vertices = simulator.Cloth.Positions;
    }
}
