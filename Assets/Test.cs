using Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private int _count;
    private float _time;
    // Start is called before the first frame update
    void Start()
    {
        var gen = new TestClothMeshGenerator();
        var component = GetComponent<SkinnedMeshRenderer>();
        component.sharedMesh = gen.Generate(30, 30, 2, new Vector3());
    }

    // Update is called once per frame
    void Update()
    {
        _count++;
        if (_count % 60 == 0)
        {
            Debug.Log(1 / Time.deltaTime);
        }
    }
}
