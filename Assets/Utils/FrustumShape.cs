using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Utils
{
    public enum MeshPivot
    {
        /// <summary>
        /// 中心
        /// </summary>
        Center,
        /// <summary>
        /// 上
        /// </summary>
        Top,
        /// <summary>
        /// 下
        /// </summary>
        Bottom,
        /// <summary>
        /// 左
        /// </summary>
        Left,
        /// <summary>
        /// 右
        /// </summary>
        Right,
        /// <summary>
        /// 前
        /// </summary>
        Front,
        /// <summary>
        /// 后
        /// </summary>
        Back,
    }

    public class FrustumShape
    {
        protected MeshPivot _meshPivot;//轴心的位置
        protected string _meshName;

        protected Vector3 _vertexOffset;//顶点的偏移量
        protected Vector3[] _vertices;//顶点数据
        protected Vector3[] _normals;//法线数据
        protected int[] _triangles;//三角面顶点的索引数据
        protected Vector2[] _uvs;//uv坐标数据

        protected Mesh _shapeMesh;//保存生成的mesh

        #region private members
        private readonly float _height;//高度
        private readonly float _topRadius;//上顶面半径
        private readonly float _bottomRadius;//下顶面的半径

        private readonly int _circularSideCount;//保存圆分割的边数
        #endregion
        #region public properties
        /// <summary>
        /// 轴心的位置
        /// </summary>
        public MeshPivot MeshPivot => _meshPivot;

        /// <summary>
        /// 顶点数据
        /// </summary>
        public Vector3[] Vertices
        {
            get
            {
                if (_vertices == null) _vertices = GetVertices();
                return _vertices;
            }
        }

        /// <summary>
        /// 法线数据
        /// </summary>
        public Vector3[] Normals
        {
            get
            {
                if (_normals == null) _normals = GetNormals();
                return _normals;
            }
        }

        /// <summary>
        /// 三角面顶点的索引数据
        /// </summary>
        public int[] Triangles
        {
            get
            {
                if (_triangles == null) _triangles = GetTriangles();
                return _triangles;
            }
        }

        /// <summary>
        /// uv坐标数据
        /// </summary>
        public Vector2[] UVs
        {
            get
            {
                if (_uvs == null) _uvs = GetUVs();
                return _uvs;
            }
        }

        /// <summary>
        /// 生成的mesh数据
        /// </summary>
        public Mesh ShapeMesh
        {
            get
            {
                if (_shapeMesh == null) _shapeMesh = GenerateMesh();
                return _shapeMesh;
            }
        }
        #endregion
        /// <summary>
        /// 生成mesh
        /// </summary>
        /// <returns></returns>
        protected Mesh GenerateMesh()
        {
            var mesh = new Mesh { name = _meshName };

            _vertices = GetVertices();
            _normals = GetNormals();
            _triangles = GetTriangles();
            _uvs = GetUVs();

            mesh.vertices = _vertices;
            mesh.normals = _normals;
            mesh.triangles = _triangles;
            mesh.uv = _uvs;
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            mesh.Optimize();
            return mesh;
        }

        /// <summary>
        /// 获得圆心切割的边数(按照固定长度)
        /// </summary>
        /// <param name="radius">半径</param>
        /// <param name="arcLen">弧度的长度</param>
        /// <returns></returns>
        protected static int GetCircularSideCount(float radius, float arcLen = 0.1f)
        {
            const int minSideCount = 3;
            const int maxSideCount = 3000;

            var sideCount = Mathf.RoundToInt(Mathf.PI * 2 * radius / arcLen);
            sideCount = Mathf.Min(Mathf.Max(minSideCount, sideCount), maxSideCount);
            return sideCount;
        }

        #region ctor
        public FrustumShape(float height, float topRadius, float bottomRadius, MeshPivot meshPivot)
        {
            _height = height;
            _topRadius = topRadius;
            _bottomRadius = bottomRadius;

            _vertexOffset = GetVertexOffset();
            _circularSideCount = GetCircularSideCount(_bottomRadius < _topRadius ? _bottomRadius : _topRadius);
        }
        #endregion

        #region protected override functions
        /// <summary>
        /// 根据mesh中心点的位置，获得顶点位置的偏移量
        /// </summary>
        /// <returns></returns>
        protected Vector3 GetVertexOffset()
        {
            return _meshPivot switch
            {
                MeshPivot.Center => Vector3.zero,
                MeshPivot.Top => new Vector3(0, _height, 0) * 0.5f,
                MeshPivot.Bottom => new Vector3(0, -_height, 0) * 0.5f,
                MeshPivot.Left => new Vector3(-(_topRadius + _bottomRadius), 0, 0) * 0.5f,
                MeshPivot.Right => new Vector3(_topRadius + _bottomRadius, 0, 0) * 0.5f,
                MeshPivot.Front => new Vector3(0, 0, _topRadius + _bottomRadius) * 0.5f,
                MeshPivot.Back => new Vector3(0, 0, -(_topRadius + _bottomRadius)) * 0.5f,
                _ => Vector3.zero
            };
        }

        /// <summary>
        /// 获得顶点的数据集合
        /// </summary>
        /// <returns></returns>
        protected Vector3[] GetVertices()
        {
            var bottomPoints = new Vector3[_circularSideCount];
            var topPoints = new Vector3[_circularSideCount];
            for (var i = 0; i < _circularSideCount; i++)
            {
                var rad = i * 1.0f / _circularSideCount * Mathf.PI * 2;
                var bottomCos = Mathf.Cos(rad) * _bottomRadius;
                var bottomSin = Mathf.Sin(rad) * _bottomRadius;
                var topCos = Mathf.Cos(rad) * _topRadius;
                var topSin = Mathf.Sin(rad) * _topRadius;
                bottomPoints[i] = new Vector3(bottomCos, -_height * 0.5f, bottomSin) - _vertexOffset;
                topPoints[i] = new Vector3(topCos, _height * 0.5f, topSin) - _vertexOffset;
            }

            var curIndex = 0;
            var arrayLen = (_circularSideCount + 1) * 2 + (_circularSideCount + 1) * 2;
            var vertices = new Vector3[arrayLen];
            //底面
            vertices[curIndex++] = new Vector3(0, -_height * 0.5f, 0) - _vertexOffset;
            for (var i = 0; i < _circularSideCount; i++)
            {
                vertices[curIndex++] = bottomPoints[i];
            }
            //顶面
            vertices[curIndex++] = new Vector3(0, _height * 0.5f, 0) - _vertexOffset;
            for (var i = 0; i < _circularSideCount; i++)
            {
                vertices[curIndex++] = topPoints[i];
            }
            //侧面
            for (var i = 0; i < _circularSideCount; i++)
            {
                vertices[curIndex++] = bottomPoints[i];
                vertices[curIndex++] = topPoints[i];
            }
            vertices[curIndex++] = bottomPoints[0];
            vertices[curIndex] = topPoints[0];
            return vertices;
        }

        /// <summary>
        /// 获得法线方向的数据集合
        /// </summary>
        /// <returns></returns>
        protected Vector3[] GetNormals()
        {
            var curIndex = 0;
            var arrayLen = (_circularSideCount + 1) * 2 + (_circularSideCount + 1) * 2;
            var normals = new Vector3[arrayLen];
            //底面
            for (var i = 0; i <= _circularSideCount; i++)
            {
                normals[curIndex++] = Vector3.down;
            }
            //顶面
            for (var i = 0; i <= _circularSideCount; i++)
            {
                normals[curIndex++] = Vector3.up;
            }
            //侧面
            for (var i = 0; i <= _circularSideCount; i++)
            {
                var rad = i * 1.0f / _circularSideCount * Mathf.PI * 2;
                var cos = Mathf.Cos(rad);
                var sin = Mathf.Sin(rad);
                normals[curIndex++] = new Vector3(cos, 0, sin);
                normals[curIndex++] = new Vector3(cos, 0, sin);
            }
            return normals;
        }

        /// <summary>
        /// 获得三角面顶点的索引
        /// </summary>
        /// <returns></returns>
        protected int[] GetTriangles()
        {
            var curIndex = 0;
            var arrayLen = _circularSideCount * 3 + _circularSideCount * 3 + _circularSideCount * 3 * 2;
            var triangles = new int[arrayLen];
            //底面
            for (var i = 1; i < _circularSideCount; i++)
            {
                triangles[curIndex++] = 0;
                triangles[curIndex++] = i;
                triangles[curIndex++] = i + 1;
            }
            triangles[curIndex++] = 0;
            triangles[curIndex++] = _circularSideCount;
            triangles[curIndex++] = 1;
            //顶面
            for (var i = _circularSideCount + 2; i < _circularSideCount * 2 + 1; i++)
            {
                triangles[curIndex++] = i;
                triangles[curIndex++] = _circularSideCount + 1;
                triangles[curIndex++] = i + 1;
            }
            triangles[curIndex++] = _circularSideCount * 2 + 1;
            triangles[curIndex++] = _circularSideCount + 1;
            triangles[curIndex++] = _circularSideCount + 2;
            //侧面
            var startIndex = _circularSideCount * 2 + 2;
            for (var i = 0; i < _circularSideCount; i++)
            {
                var index = i * 2;
                triangles[curIndex++] = startIndex + index;
                triangles[curIndex++] = startIndex + index + 1;
                triangles[curIndex++] = startIndex + index + 3;

                triangles[curIndex++] = startIndex + index + 3;
                triangles[curIndex++] = startIndex + index + 2;
                triangles[curIndex++] = startIndex + index;
            }
            return triangles;
        }

        /// <summary>
        /// 获得UV坐标的数据集合
        /// </summary>
        /// <returns></returns>
        protected Vector2[] GetUVs()
        {
            var curIndex = 0;
            var arrayLen = (_circularSideCount + 1) * 2 + (_circularSideCount + 1) * 2;
            var uvs = new Vector2[arrayLen];
            //底面
            uvs[curIndex++] = new Vector2(0.5f, 0.5f);//圆心
            for (var i = 0; i < _circularSideCount; i++)
            {
                var rad = i * 1.0f / _circularSideCount * Mathf.PI * 2;
                var cos = Mathf.Cos(rad);
                var sin = Mathf.Sin(rad);
                uvs[curIndex++] = new Vector2(cos, sin) * 0.5f + new Vector2(0.5f, 0.5f);
            }
            //顶面
            uvs[curIndex++] = new Vector2(0.5f, 0.5f);
            for (var i = 0; i < _circularSideCount; i++)
            {
                var rad = i * 1.0f / _circularSideCount * Mathf.PI * 2;
                var cos = Mathf.Cos(rad);
                var sin = Mathf.Sin(rad);
                uvs[curIndex++] = new Vector2(cos, sin) * 0.5f + new Vector2(0.5f, 0.5f);
            }
            //侧面
            var value = 1.0f / _circularSideCount;
            for (var i = 0; i <= _circularSideCount; i++)
            {
                uvs[curIndex++] = new Vector2(value * i, 0);
                uvs[curIndex++] = new Vector2(value * i, 1);
            }
            return uvs;
        }
        #endregion

    }
}
