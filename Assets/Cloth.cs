using Assets;
using Assets.Collisions;
using Assets.Contraints;
using Assets.Forces;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Cloth : MonoBehaviour
{
    private MeshFilter _meshFilter;
    private Simulator _simulator;
    private SphereCollider[] _sphereColliders;
    private Vector3 _center;
    private readonly List<Vector3> _fixedPositions = new();

    private const int _simulationIterNum = 3;

    void Start()
    {
        Application.targetFrameRate = 60;
        _meshFilter = GetComponent<MeshFilter>();
        //var gen = new TestClothMeshGenerator();
        //var mesh = gen.Generate(40, 20, 0.5f, new(-10, 10, -10));
        //_meshFilter.mesh = mesh;

        _simulator = new(CreateClothComponent(_meshFilter.mesh), _simulationIterNum, GetCollisionObjects(), transform);
        _simulator.Forces.Add(new GravityForce());
        //_simulator.Cloth.Constraints.Add(new FixedPointConstraint(_simulator.Cloth, 0, _meshFilter.mesh.vertices[0]));
    }

    private ClothComponent CreateClothComponent(Mesh mesh)
    {
        var vertices = mesh.vertices;
        var cloth = new ClothComponent(1f, vertices, mesh.triangles);

        var maxY = vertices.Max(i => i.y);
        var count = 0;
        for (var i = 0; i < vertices.Length; i++)
        {
            if (Mathf.Abs(vertices[i].y - maxY) < float.Epsilon)
            {
                count++;
                cloth.Constraints.Add(new FixedPointConstraint(cloth, i, vertices[i]));
                _fixedPositions.Add(vertices[i]);
            }
        }
        Debug.Log(count);
        return cloth;
    }

    private CollisionObject[] GetCollisionObjects()
    {
        var collisionObjects = new List<CollisionObject>();
        var sphereColliders = new List<SphereCollider>();
        foreach (var c in FindObjectsOfType<SphereCollider>(false))
        {
            collisionObjects.Add(new SphereCollisionObject(c));
            sphereColliders.Add(c);
        }
        _sphereColliders = sphereColliders.ToArray();
        _center = (_sphereColliders[0].center + _sphereColliders[1].center) / 2;
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
        var offset = ((_sphereColliders[0].center + _sphereColliders[1].center) / 2) - _center;
        var count = 0;
        foreach (var c in _simulator.Cloth.Constraints)
        {
            if (c is FixedPointConstraint fc)
            {
                fc.Position = _fixedPositions[count++] + offset;
                //fc.Position.x += Mathf.Sin(Time.time / 2) / 8;
            }
        }

        _simulator.Simulate(1.0f / 60 / _simulationIterNum);
        _meshFilter.mesh.vertices = _simulator.Cloth.Positions;
    }
}
