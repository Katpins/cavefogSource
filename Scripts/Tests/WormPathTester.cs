using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;

namespace caveFog
{

    //this boi creates a path out of spheres to test the settings of cave
    public class WormPathTester : MonoBehaviour
    {
        [SerializeField]
        private MainPathGenerator _mainGenerator;

        [SerializeField]
        private MeshFilter _meshFilter;

        [SerializeField]
        private ComputeShader _triangleCreator;

        [SerializeField]
        private SimplePathGenerator _ceilingGenerator;

        [SerializeField]
        private SimplePathGenerator _pitGenerator;

        [SerializeField]
        private RoomSettings _roomSettings;

        [SerializeField]
        private ChunkSettings _settings;
        private Chunk _chunk;
        private MainPathOffsets _offsets;

        [SerializeField]
        private int _seed = 0;
        private float3[] _mainPath;
        private float[] _mainSizes;

        [SerializeField]
        private Transform[] _pointsForCeiling = new Transform[10];

        [SerializeField]
        private Transform[] _pointsForPits = new Transform[10];

        // [SerializeField]
        // [Range(-1, 1)]
        // private float _maxYValue = 1f;
        // [SerializeField]
        // [Range(-1, 1)]
        // private float _minYValue = -1f;

        // [SerializeField]
        // private bool _rotate = true;
        [SerializeField]
        private PropType _testSettings;

        [SerializeField]
        private Transform _point2;
        [SerializeField]
        private Transform _point1;

        private float3[] _ceilingPath;
        private float[] _ceilingSizes;

        // [SerializeField]
        // private float _pitDepth = 5f;
        private float3[] _pitsPath;
        private float[] _pitsSizes;

        [SerializeField]
        private int _directionsAmount = 5;
        private float3[] _directions;

        [SerializeField]
        private Prop _testProp;

        private CheckSphere[] _spheres;

        private int3[] _emptyVoxels;



        private void Awake()
        {
            _chunk = new Chunk(Vector3.zero, _settings, _seed);
            _chunk.AddConnection();
            _offsets = new MainPathOffsets(_chunk);

        }
        private void OnEnable()
        {
            _mainGenerator._pathSettings._basePathCurve.settingsChange += UpdateGizmos;
            _mainGenerator._pathSettings._inclineCurve.settingsChange += UpdateGizmos;
            _mainGenerator._pathSettings._secondaryCurve.settingsChange += UpdateGizmos;
            _mainGenerator._wormSettings._sizeNoise.settingsChange += UpdateGizmos;

            _ceilingGenerator._pathSettings._xRotationNoise.settingsChange += UpdateGizmos;
            _ceilingGenerator._pathSettings._yRotationNoise.settingsChange += UpdateGizmos;

            _pitGenerator._pathSettings._xRotationNoise.settingsChange += UpdateGizmos;
            _pitGenerator._pathSettings._yRotationNoise.settingsChange += UpdateGizmos;

            _ceilingGenerator._pathSettings.settingsChange += UpdateGizmos;
            _pitGenerator._pathSettings.settingsChange += UpdateGizmos;

            _ceilingGenerator._wormSettings._sizeNoise.settingsChange += UpdateGizmos;
            _pitGenerator._wormSettings._sizeNoise.settingsChange += UpdateGizmos;

            _mainGenerator._pathSettings.settingsChange += UpdateGizmos;

            //UpdateGizmos();

        }
        private void OnDisable()
        {
            _mainGenerator._pathSettings._basePathCurve.settingsChange -= UpdateGizmos;
            _mainGenerator._pathSettings._inclineCurve.settingsChange -= UpdateGizmos;
            _mainGenerator._pathSettings._secondaryCurve.settingsChange -= UpdateGizmos;
            _mainGenerator._wormSettings._sizeNoise.settingsChange -= UpdateGizmos;

            _pitGenerator._pathSettings._xRotationNoise.settingsChange -= UpdateGizmos;
            _pitGenerator._pathSettings._yRotationNoise.settingsChange -= UpdateGizmos;

            _ceilingGenerator._pathSettings.settingsChange -= UpdateGizmos;
            _pitGenerator._pathSettings.settingsChange -= UpdateGizmos;

            _ceilingGenerator._wormSettings._sizeNoise.settingsChange -= UpdateGizmos;
            _pitGenerator._wormSettings._sizeNoise.settingsChange -= UpdateGizmos;

            _mainGenerator._pathSettings.settingsChange -= UpdateGizmos;
        }

        public void UpdateGizmos()
        {
            _chunk = new Chunk(Vector3.zero, _settings, _seed);
            _chunk.AddConnection();
            _offsets = new MainPathOffsets(_chunk);

            //_chunk = new Chunk(Vector3.zero, _settings, _seed);
            _offsets.ResetOffsets();
            GenerateMainPathtest();
            //_offsets.ResetOffsets();
            GenerateCeilingWorms(_chunk);
            //_offsets.ResetOffsets();
            GeneratePitWorms(_chunk);
            //_offsets.ResetOffsets();
            GenerateRandomDirections();
        }

        private void GenerateMainPathtest()
        {
            PathData _data = _mainGenerator.GenerateMainPath(_chunk, _offsets);
            _mainPath = _data.PositionsToArray();
            _mainSizes = _data.SizeToArray();
            _data.Dispose();
        }

        public void GeneratePathMesh()
        {
            _offsets.ResetOffsets();
            PathData _data = _mainGenerator.GenerateMainPath(_chunk, _offsets);
            CarvePath(_data);

            QuickMesh();
            _data.Dispose();
        }

        public void GenerateWalkablePathMesh()
        {
            _offsets.ResetOffsets();
            PathData _data = _mainGenerator.GenerateMainPath(_chunk, _offsets);
            CarveWalkablePath(_data);
            QuickMesh();
            _data.Dispose();
        }

        public void GenerateWalkablePathWithMesh()
        {
            _offsets.ResetOffsets();
            PathData _data = _mainGenerator.GenerateMainPath(_chunk, _offsets);
            CarveMainAndWalkablePath(_data);
            QuickMesh();
            _data.Dispose();
        }


        private void GenerateRandomDirections()
        {

            _directions = new float3[_directionsAmount];
            for (int i = 0; i < _directionsAmount; i++)
            {
                _directions[i] = RandomOnUnitSphere(_chunk.RandomGenerator);
            }
        }
        private void GenerateCeilingWorms(Chunk _chunk)
        {
            float3[] _startPos = new float3[_pointsForCeiling.Length];
            float3[] _endPos = new float3[_pointsForCeiling.Length];

            for (int i = 0; i < _pointsForCeiling.Length; i++)
            {
                _startPos[i] = (float3)_pointsForCeiling[i].position;
                _endPos[i] = _startPos[i];
                _endPos[i].y += (float)_chunk.RandomGenerator.Next(_roomSettings._ceilingsHeightMin * 10, _roomSettings._ceilingsHeightMax * 10) / 10f;
            }

            PathData _data = _ceilingGenerator.GenerateWormPaths(_startPos, _endPos, _offsets);

            _ceilingPath = _data.PositionsToArray();
            _ceilingSizes = _data.SizeToArray();
            _data.Dispose();
        }

        private void GeneratePitWorms(Chunk _chunk)
        {
            float3[] _startPos = new float3[_pointsForPits.Length];
            float3[] _endPos = new float3[_pointsForPits.Length];

            for (int i = 0; i < _pointsForPits.Length; i++)
            {
                _startPos[i] = (float3)_pointsForPits[i].position;
                _endPos[i] = _startPos[i];
                _endPos[i].y -= (float)_chunk.RandomGenerator.Next(_roomSettings._pitsDepthMin * 10, _roomSettings._pitsDepthMax * 10) / 10f;
            }

            PathData _data = _pitGenerator.GenerateWormPaths(_startPos, _endPos, _offsets);

            _pitsPath = _data.PositionsToArray();
            _pitsSizes = _data.SizeToArray();
            _data.Dispose();
        }

        private void CarvePath(PathData _path)
        {
            VoxelData _voxels = new VoxelData(_chunk._settings._widthVoxels, _chunk._settings._heightVoxels, _chunk._settings._lengthVoxels);

            CarveJobs.CarveMainJob _carveJob = new CarveJobs.CarveMainJob
            {
                _length = 5,
                _offsets = new float3(_chunk._settings._widthOffset,
                                       _chunk._settings._heightOffset,
                                       _chunk._settings._lengthOffset),
                _startPosition = _chunk._startPos,
                VoxelDatas = _voxels,
                PathData = _path
            };

            JobHandle _handle = _carveJob.Schedule(_path.ArrayLength, 400);

            _handle.Complete();
            FillEmptyVoxels(_voxels);
            _chunk._VoxelMap1D = _voxels.GetPureArray();
            _voxels.Dispose();

        }
        private void CarveWalkablePath(PathData _path)
        {
            VoxelData _voxels = new VoxelData(_chunk._settings._widthVoxels, _chunk._settings._heightVoxels, _chunk._settings._lengthVoxels);

            _voxels.SetAllValues(1f);

            CarveJobs.CarveNegativeMainJob _carveJob = new CarveJobs.CarveNegativeMainJob
            {
                _radius = 0.4f,
                _length = 5,
                _offsets = new float3(_chunk._settings._widthOffset,
                                       _chunk._settings._heightOffset,
                                       _chunk._settings._lengthOffset),
                _startPosition = _chunk._startPos,
                VoxelDatas = _voxels,
                PathData = _path
            };

            JobHandle _handle = _carveJob.Schedule(_path.ArrayLength, 400);
            _handle.Complete();
            FillEmptyVoxels(_voxels);
            _chunk._VoxelMap1D = _voxels.GetPureArray();
            _voxels.Dispose();

        }

        public void FillEmptyVoxels(VoxelData in_voxels)
        {
            int index = 0;

            //find first empty voxel
            while (in_voxels.GetValue(index) >= _chunk._settings._surface)
            {
                index++;
            }

            //getting starting coordinate
            int3 coordinate = in_voxels.IndexToCoordinate(index);

            //list of coordinates to check
            List<int3> _coordToCheck = new List<int3>();
            _coordToCheck.Add(coordinate);

            //list of checked voxels
            VoxelData _checkedVoxels = new VoxelData(_chunk._settings._widthVoxels, _chunk._settings._heightVoxels, _chunk._settings._lengthVoxels);
            _checkedVoxels.SetAllValues(0);



            //empty voxels
            List<int3> _emptyVoxelsList = new List<int3>();

            int iter = 0;
            //check coordinates
            while (_coordToCheck.Count > 0)
            {
                // if (iter > 10000)
                // {
                //     Debug.Log("Oh no an endless loop!");
                //     break;
                // }
                coordinate = _coordToCheck[0];

                _checkedVoxels.SetValue(coordinate.x, coordinate.y, coordinate.z, 1);

                //add to empty voxels
                _emptyVoxelsList.Add(coordinate);

                //remove added checked voxel
                _coordToCheck.Remove(coordinate);

                //add its unchecked neighbours
                VoxelUtility.AddNeighbours(coordinate, _coordToCheck, in_voxels, _checkedVoxels, 1, _chunk._settings._surface, true);

                iter++;
            }
            _checkedVoxels.Dispose();

            _emptyVoxels = _emptyVoxelsList.ToArray();

            //Debug.Log(_emptyVoxels.Length);
            // Debug.Log(index);
        }


        private void CarveMainAndWalkablePath(PathData _path)
        {
            VoxelData _voxels = new VoxelData(_chunk._settings._widthVoxels, _chunk._settings._heightVoxels, _chunk._settings._lengthVoxels);

            //_voxels.SetAllValues(1f);
            CarveJobs.CarveMainJob _carveJob = new CarveJobs.CarveMainJob
            {
                //_radius = 0.4f,
                _length = 5,
                _offsets = new float3(_chunk._settings._widthOffset,
                                      _chunk._settings._heightOffset,
                                      _chunk._settings._lengthOffset),
                _startPosition = _chunk._startPos,
                VoxelDatas = _voxels,
                PathData = _path
            };

            CarveJobs.CarveNegativeMainJob _carveWalkJob = new CarveJobs.CarveNegativeMainJob
            {
                _radius = 0.55f,
                _length = 5,
                _offsets = new float3(_chunk._settings._widthOffset,
                                       _chunk._settings._heightOffset,
                                       _chunk._settings._lengthOffset),
                _startPosition = _chunk._startPos,
                VoxelDatas = _voxels,
                PathData = _path
            };

            JobHandle _handle = _carveJob.Schedule(_path.ArrayLength, 200);
            JobHandle _handleTwo = _carveWalkJob.Schedule(_path.ArrayLength, 200, _handle);
            _handleTwo.Complete();
            _chunk._VoxelMap1D = _voxels.GetPureArray();
            _voxels.Dispose();

        }
        private void QuickMesh()
        {
            int _mainMarchKernel = _triangleCreator.FindKernel("CSMain");

            Vector3 _startPos = _chunk._startPos - transform.position;

            int width = _chunk._settings._widthVoxels;
            int length = _chunk._settings._lengthVoxels;
            int height = _chunk._settings._heightVoxels;

            float[] _map1D = _chunk._VoxelMap1D;

            ComputeBuffer _valuesBuffer = new ComputeBuffer(_map1D.Length, sizeof(float), ComputeBufferType.Default);
            _valuesBuffer.SetData(_map1D);
            _triangleCreator.SetBuffer(_mainMarchKernel, "_values", _valuesBuffer);

            ComputeBuffer _trianglesBuffer = new ComputeBuffer(_map1D.Length * 5, sizeof(float) * 9, ComputeBufferType.Append);
            _trianglesBuffer.SetCounterValue(0);
            _triangleCreator.SetBuffer(_mainMarchKernel, "_triangles", _trianglesBuffer);

            // ComputeBuffer _vertexBuffer = new ComputeBuffer(_map1D.Length * 5 * 3, sizeof(float) * 3, ComputeBufferType.Default);
            // _triangleCreator.SetBuffer(_mainMarchKernel, "outVertex", _vertexBuffer);

            ComputeBuffer _triangleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

            _triangleCreator.SetFloats("START_POS", new float[3] { _startPos.x, _startPos.y, _startPos.z });
            _triangleCreator.SetFloats("OFFSETS", new float[3] { _chunk._settings._widthOffset, _chunk._settings._heightOffset, _chunk._settings._lengthOffset });
            _triangleCreator.SetFloats("DIMENSIONS", new float[3] { width, height, length });
            _triangleCreator.SetFloat("SURFACE", _chunk._settings._surface);

            _triangleCreator.Dispatch(_mainMarchKernel, width / 4, height / 4, length / 4);

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

            Triangle[] _rawTriangles = new Triangle[_triangleCount];

            _trianglesBuffer.GetData(_rawTriangles, 0, 0, _triangleCount);
            _trianglesBuffer.Release();
            _trianglesBuffer.Dispose();

            List<Vector3> _verticies = new List<Vector3>();
            List<int> _triangles = new List<int>();

            for (int i = 0; i < _triangleCount; i++)
            {
                _verticies.Add(_rawTriangles[i].p1);
                _verticies.Add(_rawTriangles[i].p2);
                _verticies.Add(_rawTriangles[i].p3);
                _triangles.Add(i * 3);
                _triangles.Add(i * 3 + 1);
                _triangles.Add(i * 3 + 2);

            }


            // for (int index = 0; index < _triangles.Count; index++)
            // {
            //     //_triangles[index] = (ushort)index;

            //     for (int i = 0; i < _triangles.Count; i++)
            //     {
            //         if (i == index) continue;
            //         //add normals of verticies as they belong to different triangles
            //         if (math.lengthsq(_verticies[i] - _verticies[index]) < 0.001f)
            //         // equals = _verticiesRawNative[i]._position == _verticiesRawNative[index]._position;
            //         //if (equals.x && equals.y && equals.z)
            //         {

            //             _verticies[i] = _verticies[index];
            //             //_verticiesRawNative[index] = new VertexData(_verticiesRawNative[index]._position, math.normalize(_verticiesRawNative[i]._normal + _verticiesRawNative[index]._normal));

            //         }

            //     }
            // }

            _meshFilter.sharedMesh = new Mesh();
            _meshFilter.sharedMesh.vertices = _verticies.ToArray();
            _meshFilter.sharedMesh.triangles = _triangles.ToArray();

            _meshFilter.sharedMesh.RecalculateNormals();
            _meshFilter.sharedMesh.RecalculateTangents();

        }

        private float3 rotateXZCoordinates(float3 _coord, float3 _center, float _angle)
        {
            float3 newCoord = _coord;

            newCoord.x = _center.x + (_coord.x - _center.x) * math.cos(_angle) + (_coord.z - _center.z) * math.sin(_angle);
            newCoord.z = _center.z - (_coord.x - _center.x) * math.sin(_angle) + (_coord.z - _center.z) * math.cos(_angle);

            // x2 = x0 + (x - x0) * cos(theta) + (y - y0) * sin(theta)
            // y2 = y0 - (x - x0) * sin(theta) + (y - y0) * cos(theta)

            return newCoord;
        }
        private float3 RandomOnUnitSphere(System.Random _seedGen)
        {
            float3 _dir = math.float3(_seedGen.Next(-1000, 1000), _seedGen.Next((int)(1000f * _testSettings.MinY), (int)(1000f * _testSettings.MaxY)), _seedGen.Next(-1000, 1000)) / 1000f;


            if (_testSettings.DeadZone)
            {
                //check normal coordinates
                if (math.abs(_dir.z) < _testSettings.DeadZoneSizeZ)
                {
                    _dir.z = math.sign(_dir.z) * _testSettings.DeadZoneSizeZ;
                }

                if (math.abs(_dir.x) > _testSettings.DeadZoneSizeX)
                {
                    _dir.x = math.sign(_dir.x) * _testSettings.DeadZoneSizeX;
                }

                float _angle = math.asin(math.normalize(_point2.position - _point1.position).z);

                _dir = rotateXZCoordinates(_dir, math.float3(0, 0, 0), _angle);

            }

            //_dir = (math.pow(_unit, 1f / 3f) / math.length(_dir)) * _dir;
            _dir = (1f / math.length(_dir)) * _dir;

            return _dir;
        }
        private void OnDrawGizmos()
        {
            if (_chunk == null) return;
            if (_chunk._VoxelMap1D != null)
            {
                for (int x = 0; x < _chunk._settings._widthVoxels; x++)
                {
                    for (int y = 0; y < _chunk._settings._heightVoxels; y++)
                    {
                        for (int z = 0; z < _chunk._settings._lengthVoxels; z++)
                        {

                            if (_chunk._VoxelMap1D[x +
                                                y * _chunk._settings._widthVoxels +
                                                z * _chunk._settings._widthVoxels * _chunk._settings._heightVoxels] == 0) continue;

                            Gizmos.color = Color.Lerp(Color.blue, Color.green, _chunk._VoxelMap1D[x +
                                                y * _chunk._settings._widthVoxels +
                                                z * _chunk._settings._widthVoxels * _chunk._settings._heightVoxels] / _chunk._settings._surface);
                            Gizmos.DrawCube(new Vector3(x * _chunk._settings._widthOffset, y * _chunk._settings._heightOffset, z * _chunk._settings._lengthOffset)
                            + _chunk._startPos
                            , Vector3.one * 0.05f);




                        }
                    }
                }

            }

            // for (int i = 0; i < _emptyVoxels.Length; i++)
            // {
            //     Gizmos.DrawCube(new Vector3(_emptyVoxels[i].x * _chunk._settings._widthOffset, _emptyVoxels[i].y * _chunk._settings._heightOffset, _emptyVoxels[i].z * _chunk._settings._lengthOffset)
            //            + _chunk._startPos
            //            , Vector3.one * 0.05f);
            // }

            if (_mainPath == null) return;
            for (int i = 0; i < _mainPath.Length; i++)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(_mainPath[i], _mainSizes[i] / 2);
                Gizmos.color = Color.Lerp(Color.blue, Color.red, (float)i / (float)_mainPath.Length);
                Gizmos.DrawSphere(_mainPath[i], 0.03f);
            }

            if (_chunk == null) return;
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(_chunk._centerPos, new Vector3(_chunk._settings._widthWU, _chunk._settings._heightWU, _chunk._settings._lengthWU));

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(_chunk._centerPos, new Vector3(

                _chunk._settings._widthVoxels * _chunk._settings._widthOffset,
                _chunk._settings._heightVoxels * _chunk._settings._heightOffset,
                _chunk._settings._lengthVoxels * _chunk._settings._lengthOffset

            ) - new Vector3(1, 1, 1) * _mainGenerator._pathSettings._pushFromBoundsThreshold);

            foreach (Connection c in _chunk._connections)
            {
                Gizmos.DrawSphere(c._point + _chunk.GetConnectionNormal(c) * _mainGenerator._pathSettings._pushFromBoundsThreshold / 2, 0.3f);
            }


            if (_ceilingPath == null) return;
            for (int i = 0; i < _ceilingPath.Length; i++)
            {
                Gizmos.DrawWireSphere(_ceilingPath[i], _ceilingSizes[i]);
            }

            if (_pitsPath == null) return;
            for (int i = 0; i < _pitsPath.Length; i++)
            {
                Gizmos.DrawWireSphere(_pitsPath[i], _pitsSizes[i]);
            }


            if (_directions == null || _point1 == null || _point2 == null) return;
            Gizmos.color = Color.red;
            for (int i = 0; i < _directions.Length; i++)
            {
                Gizmos.DrawLine((_point1.position + _point2.position) / 2,
                                (_point1.position + _point2.position) / 2 + (Vector3)_directions[i]);
            }


            // if (_testProp == null) return;

            //if (_spheres == null) _spheres = _testProp.GetCheckSpheres();



        }
    }

}
