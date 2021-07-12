using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;
using System;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;


namespace caveFog

{
    public struct Triangle
    {
        public float3 p1, p2, p3;
    }

    public struct VertexData
    {
        public float3 _position;
        public float3 _normal;

        public VertexData(float3 in_position, float3 in_normal)
        {
            _position = in_position;
            _normal = in_normal;
        }

        public static readonly VertexAttributeDescriptor[] VertexDataBufferLayout =
        {
           new VertexAttributeDescriptor(VertexAttribute.Position),
           new VertexAttributeDescriptor(VertexAttribute.Normal)
        };
    }
    public class MeshGenerator : MonoBehaviour
    {
        [SerializeField]
        private ComputeShader _marcherShader = default;

        public Action _MeshGenDone;

        [SerializeField]
        private Chunk _currentChunk;
        private int _mainMarchKernel;
        private Mesh _mesh;

        //private NativeArray<Triangle> _trianglesRawNative;
        private NativeArray<VertexData> _verticiesRawNative;
        private NativeArray<ushort> _trianglesNative;

        //private NativeArray<float3> _verticiesOutNative;
        private NativeArray<VertexData> _verticiesOutNative;

        private int _vertCount;
        private Vector3[] _verticiesArray;
        private int[] _trianglesArray;

        private float4 _maxValuesForEdges;

        private Coroutine _generatorCoroutine;

        private Triangle[] _rawTriangles;
        private void Start()
        {

        }

        public void GenerateMesh(Chunk in_chunk)
        {
            _currentChunk = in_chunk;
            _mesh = new Mesh();

            _generatorCoroutine = StartCoroutine(CO_GenerateMesh());

        }

        //spacing out generation process
        private IEnumerator CO_GenerateMesh()
        {
            //yield return new WaitForFixedUpdate();
            //yield return new WaitForEndOfFrame();

            GetVerticies();
            //yield return null;
            //yield return new WaitForFixedUpdate();
            //yield return new WaitForEndOfFrame();
            yield return null;

            RemapTriangles();

            //yield return new WaitForFixedUpdate();
            yield return null;

            ConnectToPreviousChunk();

            //yield return new WaitForEndOfFrame();

            SubMeshDescriptor subMesh = new SubMeshDescriptor(0, 0);


            _mesh.SetVertexBufferParams(_vertCount, VertexData.VertexDataBufferLayout);
            _mesh.SetIndexBufferParams(_trianglesNative.Length, IndexFormat.UInt16);

            _mesh.SetVertexBufferData(_verticiesOutNative, 0, 0, _vertCount, 0, MeshUpdateFlags.DontValidateIndices);
            _mesh.SetIndexBufferData(_trianglesNative, 0, 0, _trianglesNative.Length, MeshUpdateFlags.DontValidateIndices);

            _mesh.subMeshCount = 1;
            subMesh.indexCount = _vertCount;
            _mesh.SetSubMesh(0, subMesh);

            _trianglesNative.Dispose();

            //stupid stupid thing but without it shader doesnt work
            _mesh.uv = new Vector2[_vertCount];
            
            //yield return new WaitForEndOfFrame();

            _verticiesOutNative.Dispose();

            _MeshGenDone.Invoke();
        }
        public Mesh GetMesh()
        {

            // Debug.Log(" Max X: " + _maxValuesForEdges.x +
            //             " Min X: " + _maxValuesForEdges.y +
            //             " Max Z: " + _maxValuesForEdges.z +
            //             " Min Z: " + _maxValuesForEdges.w);
            //_mesh.vertex = _verticies;
            //_mesh.triangles = _triangles;

            //_mesh.RecalculateNormals();
            _mesh.RecalculateBounds();
            _mesh.RecalculateTangents();
            _mesh.UploadMeshData(false);


            return _mesh;
        }

        //getting "raw" separate triangles with marching scuared algorithm
        private void GetVerticies()
        {
            _mainMarchKernel = _marcherShader.FindKernel("CSMain");

            Vector3 _startPos = _currentChunk._startPos - transform.position;

            int width = _currentChunk._settings._widthVoxels;
            int length = _currentChunk._settings._lengthVoxels;
            int height = _currentChunk._settings._heightVoxels;

            //float[] _map1D = _currentChunk._VoxelMap1D;

            ComputeBuffer _valuesBuffer = new ComputeBuffer(width * length * height, sizeof(float), ComputeBufferType.Default);
            _valuesBuffer.SetData(_currentChunk._VoxelMap1D);
            _marcherShader.SetBuffer(_mainMarchKernel, "_values", _valuesBuffer);

            ComputeBuffer _trianglesBuffer = new ComputeBuffer(width * length * height * 5, sizeof(float) * 9, ComputeBufferType.Append);
            _trianglesBuffer.SetCounterValue(0);
            _marcherShader.SetBuffer(_mainMarchKernel, "_triangles", _trianglesBuffer);

            // ComputeBuffer _vertexBuffer = new ComputeBuffer(_map1D.Length * 5 * 3, sizeof(float) * 3, ComputeBufferType.Default);
            // _marcherShader.SetBuffer(_mainMarchKernel, "outVertex", _vertexBuffer);

            ComputeBuffer _triangleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

            _marcherShader.SetFloats("START_POS", new float[3] { _startPos.x, _startPos.y, _startPos.z });
            _marcherShader.SetFloats("OFFSETS", new float[3] { _currentChunk._settings._widthOffset, _currentChunk._settings._heightOffset, _currentChunk._settings._lengthOffset });
            _marcherShader.SetFloats("DIMENSIONS", new float[3] { width, height, length });
            _marcherShader.SetFloat("SURFACE", _currentChunk._settings._surface);

            _marcherShader.Dispatch(_mainMarchKernel, width / 4, height / 4, length / 4);

            _valuesBuffer.Release();
            _valuesBuffer.Dispose();

            //getting triangles back
            ComputeBuffer.CopyCount(_trianglesBuffer, _triangleCountBuffer, 0);

            int[] _count = new int[1];

            _triangleCountBuffer.GetData(_count);
            _triangleCountBuffer.Release();
            _triangleCountBuffer.Dispose();

            int _triangleCount = _count[0];
            _count = null;

            _rawTriangles = new Triangle[_triangleCount];

            _trianglesBuffer.GetData(_rawTriangles, 0, 0, _triangleCount);
            _trianglesBuffer.Release();
            _trianglesBuffer.Dispose();

            _vertCount = _triangleCount * 3;

            _verticiesRawNative = new NativeArray<VertexData>(_vertCount, Allocator.TempJob);
            NativeArray<Triangle> _trianglesRawNative = new NativeArray<Triangle>(_triangleCount, Allocator.TempJob);
            _trianglesRawNative.CopyFrom(_rawTriangles);


            _rawTriangles = null;

            GetVertexFromTrianglesJob _getVertsJob = new GetVertexFromTrianglesJob
            {
                InTriangleStructs = _trianglesRawNative,
                OutVertexData = _verticiesRawNative
            };

            JobHandle _jobHandle = _getVertsJob.Schedule(_triangleCount, 128);
            _jobHandle.Complete();

            _trianglesRawNative.Dispose();
            //transfering triangle data into verticies + triangle indecies
            // for (int i = 0; i < _triangleCount; i++)
            // {
            //     //_duplicatesAmount = 0;
            //     _vertCount = _verticiesRawNative.Length;

            //     _verticiesRawNative[i * 3] = _rawTriangles[i].p1;
            //     _verticiesRawNative[i * 3 + 1] = _rawTriangles[i].p2;
            //     _verticiesRawNative[i * 3 + 2] = _rawTriangles[i].p3;
            // }




        }
        private void OnDrawGizmos()
        {
            // if (_verticiesRawNative == null) return;
            // for (int i = 0; i < _verticiesRawNative.Length; i++)
            // {
            //     Gizmos.DrawSphere(_verticiesRawNative[i]._position, 0.05f);

            // }
        }
        //remove duplicates and find edges
        private void RemapTriangles()
        {

            ///////////////////////Remap//////////////////////////

            _trianglesNative = new NativeArray<ushort>(_vertCount, Allocator.Persistent);

            // NativeCounter _duplicateCounter = new NativeCounter(Allocator.TempJob);

            RemapJob _singleRemapJob = new RemapJob
            {
                InVerticies = _verticiesRawNative,
                OutTriangles = _trianglesNative,
                //OutVerticies = _verticiesOutNative,
                _vertexCount = (ushort)_vertCount

            };

            JobHandle _jobHandleRemap = _singleRemapJob.Schedule(_verticiesRawNative.Length, 200);
            _jobHandleRemap.Complete();


            ////////////////////////Asign/////////////////////////

            _verticiesOutNative = new NativeArray<VertexData>(_verticiesRawNative.Length, Allocator.TempJob);

            NativeArray<ushort> _edgeVerticies = new NativeArray<ushort>(_vertCount, Allocator.TempJob);
            NativeCounter _edgeVertexCounter = new NativeCounter(Allocator.TempJob);
            //_edgeVertexCounter.Count = 1;

            _maxValuesForEdges = new float4(_currentChunk._settings._widthWU / 2,
                                            -_currentChunk._settings._widthWU / 2,
                                            _currentChunk._settings._lengthWU / 2,
                                            -_currentChunk._settings._lengthWU / 2);

            AssignVertexJob _singleAssignJob = new AssignVertexJob
            {
                InVerticies = _verticiesRawNative,
                _edgeCounter = _edgeVertexCounter,
                MaxValues = _maxValuesForEdges,
                OutTriangles = _trianglesNative,
                OutVerticies = _verticiesOutNative,
                EdgeVertecies = _edgeVerticies,
                _vertexCount = (ushort)_vertCount

            };

            JobHandle _jobHandleAssign = _singleAssignJob.Schedule(_verticiesRawNative.Length, 128);

            //yield return new WaitForEndOfFrame();
            _jobHandleAssign.Complete();

            int[] _edgeVertexArray = new int[_edgeVertexCounter.Count];

            for (int i = 0; i < _edgeVertexCounter.Count; i++)
            {
                _edgeVertexArray[i] = _edgeVerticies[i];
            }


            _currentChunk.EdgeVerticies = _edgeVertexArray;

            //JobHandle.CompleteAll(ref _jobHandleRemap, ref _jobHandleAssign);

            //_verticiesArray = new Vector3[_vertCount];

            // for (int i = 0; i < _trianglesNative.Length; i++)
            // {
            //     if (i < _verticiesOutNative.Length && _verticiesOutNative[_trianglesNative[i]].Equals(default))
            //         _verticiesOutNative[_trianglesNative[i]] = _verticiesRawNative[_trianglesNative[i]];
            // }
            _edgeVertexArray = null;

            _edgeVertexCounter.Dispose();
            _edgeVerticies.Dispose();
            _verticiesRawNative.Dispose();

            //Debug.Log(_maxValues);
        }



        private void ConnectToPreviousChunk()
        {
            //check if entrance has a neighbour
            //_mesh.RecalculateNormals();

            Chunk _prevChunk = _currentChunk.FindNeighbour(_currentChunk._entrance);

            if (_prevChunk == null) return;

            //find arrays of proper positions
            int[] _entrIndx = MeshUtility.FindClosestEdgeVerts(_currentChunk, _verticiesOutNative, _currentChunk._entrance);
            int[] _exitIndx = MeshUtility.FindClosestEdgeVerts(_prevChunk, _prevChunk._chunkObject.ChunkMesh, _currentChunk._entrance);

            Vector3 _exitVert;
            Vector3 _entranceVert;
            Vector3 _closestVert;
            Vector3 _closestVertNormal;

            int _exitVertIndx = 0;
            int _entranceVertIndx = 0;

            float _smalestDistance;
            float _distance;

            Transform _prevChunkTransform = _prevChunk._chunkObject.transform;

            //verticies which we gonna modify
            Vector3[] _exitVerts = _prevChunk._chunkObject.ChunkMesh.vertices;
            Vector3[] _exitNormals = _prevChunk._chunkObject.ChunkMesh.normals;
            //Vector3[] _exitNormals = _prevChunk._chunkObject.ChunkMesh.normals;

            bool[] _chosedVerticies = new bool[_exitIndx.Length];

            //go through each entrance vert and put it onto the closest exit vert + copy normals
            for (int i = 0; i < _entrIndx.Length; i++)
            {
                _closestVert = Vector3.zero;
                _closestVertNormal = Vector3.zero;
                _smalestDistance = 1000;
                _entranceVert = transform.TransformPoint(_verticiesOutNative[_entrIndx[i]]._position);

                //compare vert disteance to current entrance chunk vert and choose the closest one
                for (int j = 0; j < _exitIndx.Length; j++)
                {
                    _exitVert = _prevChunkTransform.TransformPoint(_exitVerts[_exitIndx[j]]);
                    //_closestVertNormal = _exitNormals[_exitIndx[j]];

                    _distance = (_exitVert - _entranceVert).sqrMagnitude;

                    if (_smalestDistance > _distance)
                    {
                        _smalestDistance = _distance;
                        _closestVert = _exitVert;
                        _exitVertIndx = j;
                    }
                }

                _chosedVerticies[_exitVertIndx] = true;
                //assign new position
                //_currentChunk._chunkObject.SetVert(_entrIndx[i], _prevChunk._chunkObject.GetVertPos(_closestVert));
                _verticiesOutNative[_entrIndx[i]] = new VertexData(transform.InverseTransformPoint(_closestVert), math.normalize(_currentChunk._entrance._point - _closestVert));
                _exitNormals[_exitIndx[_exitVertIndx]] = math.normalize(_currentChunk._entrance._point - _closestVert);
            }


            //go through the exit verts which dont have connected pair
            for (int i = 0; i < _exitIndx.Length; i++)
            {
                //if
                if (!_chosedVerticies[i])
                {
                    _exitVert = _prevChunkTransform.TransformPoint(_exitVerts[_exitIndx[i]]);
                    _smalestDistance = 1000;
                    _closestVert = Vector3.zero;

                    for (int j = 0; j < _entrIndx.Length; j++)
                    {
                        _entranceVert = transform.TransformPoint(_verticiesOutNative[_entrIndx[j]]._position);
                        //_closestVertNormal = _exitNormals[_exitIndx[j]];

                        _distance = (_exitVert - _entranceVert).sqrMagnitude;

                        if (_smalestDistance > _distance)
                        {
                            _smalestDistance = _distance;
                            _closestVert = _entranceVert;
                            _entranceVertIndx = j;
                        }
                    }

                    _exitVerts[_exitIndx[i]] = _prevChunkTransform.InverseTransformPoint((Vector3)_closestVert);
                    _exitNormals[_exitIndx[i]] = math.normalize(_currentChunk._entrance._point - _closestVert);
                }

            }



            MeshUtility.SetVerts(_prevChunk._chunkObject.ChunkMesh, _exitVerts);
            MeshUtility.SetNormals(_prevChunk._chunkObject.ChunkMesh, _exitNormals);

            _prevChunk._chunkObject.UpdateCollider();

        }

        [BurstCompile]
        public struct AssignVertexJob : IJobParallelFor
        {
            public ushort _vertexCount;

            public NativeCounter _edgeCounter;
            [ReadOnly, NativeDisableParallelForRestriction] private NativeArray<VertexData> _inVerticies;

            public NativeArray<VertexData> InVerticies { get => _inVerticies; set => _inVerticies = value; }

            //remapped indecies
            [ReadOnly] private NativeArray<ushort> _outTriangles;

            public NativeArray<ushort> OutTriangles { get => _outTriangles; set => _outTriangles = value; }

            //updated verticies
            [NativeDisableParallelForRestriction] private NativeArray<VertexData> _outVerticies;

            public NativeArray<VertexData> OutVerticies { get => _outVerticies; set => _outVerticies = value; }

            [ReadOnly] private float4 _maxValues;

            public float4 MaxValues { set => _maxValues = value; }

            [WriteOnly, NativeDisableParallelForRestriction] private NativeArray<ushort> _edgeVerticies;

            public NativeArray<ushort> EdgeVertecies { set => _edgeVerticies = value; }


            //we compare current chosen vertex to all others and change its index accordingly in the new array for remapiung
            public void Execute(int index)
            {
                //if (_outTriangles[index] < index) return;
                //asssign non duplicate verticies
                if (index < _vertexCount)
                {
                    //find edge verts
                    if (math.abs(_inVerticies[_outTriangles[index]]._position.x - _maxValues.x) < 0.05f ||
                        math.abs(_inVerticies[_outTriangles[index]]._position.x - _maxValues.y) < 0.05f ||
                        math.abs(_inVerticies[_outTriangles[index]]._position.z - _maxValues.z) < 0.05f ||
                        math.abs(_inVerticies[_outTriangles[index]]._position.z - _maxValues.w) < 0.05f)
                        _edgeVerticies[_edgeCounter.Increment()] = (ushort)index;

                    _outVerticies[_outTriangles[index]] = new VertexData(_inVerticies[_outTriangles[index]]._position, math.normalize(_inVerticies[_outTriangles[index]]._normal));
                    //_outVerticies[index] = new VertexData(_inVerticies[index]._position, math.normalize(_inVerticies[index]._normal));
                }

                //_vertexCount.Increment();

            }
        }

        //the job holds a vertex and its index to compare and remap
        [BurstCompile]
        public struct RemapJob : IJobParallelFor
        {
            public ushort duplicateCount;

            public ushort _vertexCount;

            [NativeDisableParallelForRestriction] private NativeArray<VertexData> _inVerticies;

            public NativeArray<VertexData> InVerticies { get => _inVerticies; set => _inVerticies = value; }

            //remapped indecies
            [NativeDisableParallelForRestriction] private NativeArray<ushort> _outTriangles;

            public NativeArray<ushort> OutTriangles { get => _outTriangles; set => _outTriangles = value; }

            //we compare current chosen vertex to all others and change its index accordingly in the new array for remapiung
            public void Execute(int index)
            {
                _outTriangles[index] = (ushort)index;

                for (int i = 0; i < index; i++)
                {
                    if (index == i) continue;
                    //add normals of verticies as they belong to different triangles
                    if (math.lengthsq(_inVerticies[i]._position - _inVerticies[index]._position) < 0.001f)
                    {
                        _outTriangles[index] = (ushort)i;
                        _inVerticies[i] = new VertexData(_inVerticies[i]._position, math.normalize(_inVerticies[i]._normal + _inVerticies[index]._normal));

                        return;

                        // if (math.dot(_inVerticies[i]._normal, _inVerticies[index]._normal) > 0.5f)
                        // {
                        // _inVerticies[i] = new VertexData((_inVerticies[i]._position + _inVerticies[index]._position) / 2, math.normalize(_inVerticies[i]._normal + _inVerticies[index]._normal));
                        // _inVerticies[index] = new VertexData((_inVerticies[i]._position + _inVerticies[index]._position) / 2, math.normalize(_inVerticies[i]._normal + _inVerticies[index]._normal));
                        // }


                    }

                }

            }
        }

        [BurstCompile]
        public struct GetVertexFromTrianglesJob : IJobParallelFor
        {

            [ReadOnly] private NativeArray<Triangle> _inTriangleStructs;

            public NativeArray<Triangle> InTriangleStructs { set => _inTriangleStructs = value; }

            [WriteOnly, NativeDisableParallelForRestriction] private NativeArray<VertexData> _outVertexData;

            public NativeArray<VertexData> OutVertexData { set => _outVertexData = value; }

            //we go through triangle array and assign/calculate vertex/normals
            public void Execute(int index) //triangle index
            {
                //get positions
                float3 a = _inTriangleStructs[index].p1;
                float3 b = _inTriangleStructs[index].p2;
                float3 c = _inTriangleStructs[index].p3;

                //calculate normal
                float3 ab = b - a;
                float3 ac = c - a;

                float3 normal = math.normalize(math.cross(ab, ac));

                _outVertexData[index * 3] = new VertexData(a, normal);
                _outVertexData[index * 3 + 1] = new VertexData(b, normal);
                _outVertexData[index * 3 + 2] = new VertexData(c, normal);
            }
        }



    }
}
