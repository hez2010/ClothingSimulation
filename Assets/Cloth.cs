using Assets;
using Assets.Collisions;
using Assets.Contraints;
using Assets.Forces;
using System.Collections.Generic;
using UnityEngine;

public class Cloth : MonoBehaviour
{
    private MeshFilter _meshFilter;
    private Simulator _simulator;

    private const int _simulationIterNum = 5;

    void Start()
    {
        Application.targetFrameRate = 60;
        _meshFilter = GetComponent<MeshFilter>();
        var gen = new TestMeshGenerator();
        var mesh = gen.Generate(40, 20, new(-10, 10, -10));
        _meshFilter.mesh = mesh;
        //cloth.AddFEMTriangleConstraints();

        _simulator = new(CreateClothComponent(mesh), _simulationIterNum, GetCollisionObjects(), transform);
        _simulator.Forces.Add(new GravityForce());
        //_simulator.Cloth.Constraints.Add(new FixedPointConstraint(_simulator.Cloth, mesh.triangles[200], mesh.vertices[mesh.triangles[200]]));
    }

    private ClothComponent CreateClothComponent(Mesh mesh)
    {
        var cloth = new ClothComponent(1f, mesh.vertices, mesh.triangles);
        return cloth;
    }

    private CollisionObject[] GetCollisionObjects()
    {
        var collisionObjects = new List<CollisionObject>();

        foreach (var c in FindObjectsOfType<SphereCollider>(false))
        {
            collisionObjects.Add(new SphereCollisionObject(c));
        }

        foreach (var c in FindObjectsOfType<BoxCollider>(false))
        {
            collisionObjects.Add(new BoxCollisionObject(c));
        }

        foreach (var c in FindObjectsOfType<CapsuleCollider>(false))
        {
            collisionObjects.Add(new CapsuleCollisionObject(c));
        }

        return collisionObjects.ToArray();
    }

    void Update()
    {
        foreach (var c in _simulator.Cloth.Constraints)
        {
            if (c is FixedPointConstraint fc)
            {
                fc.Position.z += Mathf.Sin(Time.time) / 4;
                fc.Position.x -= Mathf.Sin(Time.time) / 4;
            }
        }

        for (var i = 0; i < _simulationIterNum; i++)
        {
            _simulator.Simulate(1.0f / 60 / _simulationIterNum);
        }

        _meshFilter.mesh.vertices = _simulator.Cloth.Positions;
    }
}
