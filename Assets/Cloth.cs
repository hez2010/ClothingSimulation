using Assets;
using Assets.Collisions;
using Assets.Contraints;
using Assets.Forces;
using Assets.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Cloth : MonoBehaviour
{
    private MeshFilter _meshFilter;
    private Simulator _simulator;
    private SphereCollider[] _sphereColliders;
    private Vector3 _center;
    public bool StartSimulation;
    public int SimulationIteration;
    public float Stiffness;
    public float Damping;
    private int _count;
    //private List<Vector3> _fixedPositions = new();

    void Start()
    {
        //Application.targetFrameRate = 60;
        _meshFilter = GetComponent<MeshFilter>();
        // var shape = new FrustumShape(3, 1, 3, MeshPivot.Center);
        var gen = new TestClothMeshGenerator();
        var mesh = gen.Generate(30, 30, 0.25f, new());
        _meshFilter.mesh = mesh;

        _simulator = new(CreateClothComponent(_meshFilter.mesh), SimulationIteration, GetCollisionObjects(), transform);
        Debug.Log(_simulator.Cloth.Constraints.Count);
        _simulator.Forces.Add(new GravityForce());
        //_simulator.Cloth.Constraints.Add(new FixedPointConstraint(_simulator.Cloth, 0, _meshFilter.mesh.vertices[0]));
    }

    private ClothComponent CreateClothComponent(Mesh mesh)
    {
        var vertices = mesh.vertices;

        var maxY = vertices.Max(i => i.y);
        var fixedVertices = new List<int>();

        fixedVertices.Add(11 * 5 + 11);
        //fixedVertices = fixedVertices.OrderBy(i => vertices[i].x).ToList();
        //var step = Mathf.PI * 2 / fixedVertices.Count;
        //var radius = 1f;
        //var center = new Vector3(fixedVertices.Sum(u => vertices[u].x) / fixedVertices.Count,
        //    fixedVertices.Sum(u => vertices[u].y) / fixedVertices.Count,
        //    fixedVertices.Sum(u => vertices[u].z) / fixedVertices.Count);
        //for (var i = 0; i < fixedVertices.Count; i++)
        //{
        //    var angle = step * i;
        //    var dir = new Vector3(1, 0, 1).magnitude;
        //    var k1 = 1;
        //    var k2 = 1;
        //    if (angle is >= Mathf.PI / 2 and < Mathf.PI)
        //    {
        //        k2 = -1;
        //    }
        //    else if (angle is >= Mathf.PI and < Mathf.PI * 3 / 2)
        //    {
        //        k1 = -1;
        //        k2 = -1;
        //    }
        //    else
        //    {
        //        k1 = -1;
        //    }
        //    var pos = new Vector3(k1 * radius * Mathf.Cos(angle), 0, k2 * radius * Mathf.Sin(angle));
        //    vertices[fixedVertices[i]] = center + pos;
        //}

        var cloth = new ClothComponent(Damping, Stiffness, vertices, mesh.triangles);
        for (var i = 0; i < fixedVertices.Count; i++)
        {
            cloth.Constraints.Add(new FixedPointConstraint(cloth, fixedVertices[i], vertices[fixedVertices[i]]));
            //_fixedPositions.Add(vertices[fixedVertices[i]]);
        }
        return cloth;
    }

    private CollisionObject[] GetCollisionObjects()
    {
        var collisionObjects = new List<CollisionObject>();
        //var sphereColliders = new List<SphereCollider>();
        foreach (var c in FindObjectsOfType<SphereCollider>(false))
        {
            collisionObjects.Add(new SphereCollisionObject(c));
            //sphereColliders.Add(c);
        }
        //_sphereColliders = sphereColliders.ToArray();
        //_center = (transform.TransformPoint(_sphereColliders[0].center) + transform.TransformPoint(_sphereColliders[1].center)) / 2;
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

        //var offset = transform.InverseTransformVector(((transform.TransformPoint(_sphereColliders[0].center) + transform.TransformPoint(_sphereColliders[1].center)) / 2) - _center);
        //var count = 0;
        //foreach (var c in _simulator.Cloth.Constraints)
        //{
        //    if (c is FixedPointConstraint fc)
        //    {
        //        fc.Position = _fixedPositions[count++] + offset;
        //    }
        //}
        if (StartSimulation)
        {
            _simulator.Simulate(1.0f / 60 / SimulationIteration);
            _count++;
            if (_count % 60 == 0)
            {
                Debug.Log(1 / Time.deltaTime);
            }
        }
        _meshFilter.mesh.vertices = _simulator.Cloth.Positions;
    }
}
