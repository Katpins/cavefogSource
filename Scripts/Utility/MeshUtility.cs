using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace caveFog
{
    public enum CoordinateType { Vertical, Horizontal };

    public static class MeshUtility
    {
        public static int[] FindClosestEdgeVerts(Chunk in_chunk, Mesh in_mesh, Connection in_connection)
        {
            CoordinateType in_type = CoordinateType.Vertical;

            switch (in_connection._connectedSide)
            {
                case Side.Left:
                case Side.Right:
                    in_type = CoordinateType.Horizontal;
                    break;

                default:
                    break;
            }

            Vector3[] _verts = in_mesh.vertices;

            Transform _meshParent = in_chunk._chunkObject.transform;

            List<int> _vertsIndx = new List<int>();

            for (int i = 0; i < in_chunk.EdgeVerticies.Length; i++)
            {
                switch (in_type)
                {
                    case CoordinateType.Horizontal:
                        if (math.abs(_meshParent.TransformPoint(_verts[in_chunk.EdgeVerticies[i]]).x - in_connection._point.x) < 0.05f)
                        {
                            _vertsIndx.Add(in_chunk.EdgeVerticies[i]);
                        }
                        break;
                    case CoordinateType.Vertical:
                        if (math.abs(_meshParent.TransformPoint(_verts[in_chunk.EdgeVerticies[i]]).z - in_connection._point.z) < 0.05f)
                        {
                            _vertsIndx.Add(in_chunk.EdgeVerticies[i]);
                        }
                        break;

                }
            }

            return _vertsIndx.ToArray();
        }

        public static int[] FindClosestEdgeVerts(Chunk in_chunk, NativeArray<VertexData> in_verts, Connection in_connection)
        {
            CoordinateType in_type = CoordinateType.Vertical;

            switch (in_connection._connectedSide)
            {
                case Side.Left:
                case Side.Right:
                    in_type = CoordinateType.Horizontal;
                    break;

                default:
                    break;
            }

            //Vector3[] _verts = in_mesh.vertices;

            Transform _meshParent = in_chunk._chunkObject.transform;

            List<int> _vertsIndx = new List<int>();

            for (int i = 0; i < in_chunk.EdgeVerticies.Length; i++)
            {
                switch (in_type)
                {
                    case CoordinateType.Horizontal:
                        if (math.abs(_meshParent.TransformPoint(in_verts[in_chunk.EdgeVerticies[i]]._position).x - in_connection._point.x) < 0.05f)
                        {
                            _vertsIndx.Add(in_chunk.EdgeVerticies[i]);
                        }
                        break;
                    case CoordinateType.Vertical:
                        if (math.abs(_meshParent.TransformPoint(in_verts[in_chunk.EdgeVerticies[i]]._position).z - in_connection._point.z) < 0.05f)
                        {
                            _vertsIndx.Add(in_chunk.EdgeVerticies[i]);
                        }
                        break;

                }
            }

            return _vertsIndx.ToArray();
        }
        public static Vector3 GetVertPos(ChunkObject in_chunkObject, Mesh in_mesh, int _indx)
        {
            if (_indx < 0 || _indx > in_mesh.vertexCount)
            {
                Debug.LogError("Indx out of bounds!");
                return Vector3.zero;
            }

            return in_chunkObject.transform.TransformPoint(in_mesh.vertices[_indx]);
        }

        public static void SetVert(ChunkObject in_chunkObject, Mesh in_mesh, int _indx, Vector3 _worldPos)
        {
            if (_indx < 0 || _indx > in_mesh.vertexCount)
            {
                Debug.LogError("Indx out of bounds!");
                return;
            }

            Vector3[] _verts = in_mesh.vertices;

            //DEBUG
            // if (_entranceVerts.Find(x => x == _verts[_indx]) != null)
            // {
            //     _entranceVerts[_entranceVerts.IndexOf(_entranceVerts.Find(x => x == _verts[_indx]))] = transform.InverseTransformPoint(_worldPos);
            // }
            //

            _verts[_indx] = in_chunkObject.transform.InverseTransformPoint(_worldPos);

            in_mesh.SetVertices(_verts);
        }

        public static void SetVerts(Mesh in_mesh, Vector3[] in_verts)
        {
            in_mesh.SetVertices(in_verts, 0, in_verts.Length, UnityEngine.Rendering.MeshUpdateFlags.DontValidateIndices);
        }

        public static void SetNormal(Mesh in_mesh, int _indx, Vector3 _normal)
        {
            if (_indx < 0 || _indx > in_mesh.vertexCount)
            {
                Debug.LogError("Indx out of bounds!");
                return;
            }

            Vector3[] _normals = in_mesh.normals;

            _normals[_indx] = _normal;

            in_mesh.SetNormals(_normals);
        }

        public static void SetNormals(Mesh in_mesh, Vector3[] in_normals)
        {
            in_mesh.SetNormals(in_normals, 0, in_normals.Length, UnityEngine.Rendering.MeshUpdateFlags.Default);
        }

        public static Vector3 GetNormal(Mesh in_mesh, int _indx)
        {
            if (_indx < 0 || _indx > in_mesh.vertexCount)
            {
                Debug.LogError("Indx out of bounds!");
                return Vector3.zero;
            }

            return in_mesh.normals[_indx];
        }
    }

}
