using System.Collections.Generic;
using UnityEngine;

namespace Assets
{
    class TestClothMeshGenerator
    {
        public Mesh Generate(float width, float height, float scale, Vector3 position)
        {
            var mesh = new Mesh();
            var columns = (int)(width + 1);
            var rows = (int)(height + 1);
            var verticlesNum = columns * rows;
            var verticles = new List<Vector3>(verticlesNum);
            var normals = new List<Vector3>(verticlesNum);
            var tangents = new List<Vector4>(verticlesNum);
            var uvs = new List<Vector2>(verticlesNum);
            var triangleVerts = new List<int>();

            for (var i = 0; i < rows; i++)
            {
                for (var j = 0; j < columns; j++)
                {
                    var localPos = new Vector3(i * scale, 0, j * scale) + position;
                    var index = (i * columns) + j;
                    verticles.Add(localPos);
                    normals.Add(Vector3.back);
                    tangents.Add(new Vector4(-1, 0, 0, -1));
                    uvs.Add(new Vector2(1.0f / width * j, 1.0f / height * i));
                    if (i < height && j < width)
                    {
                        triangleVerts.Add(index);
                        triangleVerts.Add(index + 1);
                        triangleVerts.Add(index + columns + 1);
                        triangleVerts.Add(index);
                        triangleVerts.Add(index + columns + 1);
                        triangleVerts.Add(index + columns);
                    }
                }
            }
            mesh.vertices = verticles.ToArray();
            mesh.normals = normals.ToArray();
            mesh.tangents = tangents.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.triangles = triangleVerts.ToArray();
            mesh.bounds = new Bounds(new Vector3(0, 0, 0), new Vector3(width, height, 0));

            return mesh;
        }
    }
}
