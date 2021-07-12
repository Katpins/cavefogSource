using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;


namespace caveFog
{
    public enum ValueOpperation { Equal, Not, Less, More, LessE, MoreE }
    //class containing all offsetes for current main path gen to separate state of noises from scriptables
    public class MainPathOffsets
    {
        public Vector2[] _baseLayerOffsets;
        public Vector2[] _secondaryLayerOffsets;
        public Vector2[] _inclineLayerOffsets;

        public int _baseOffset;
        public int _secondaryOffset;
        public int _inclineOffset;

        //private System.Random seedGen;
        public MainPathOffsets(Chunk in_chunk)
        {
            //seedGen = in_chunk.RandomGenerator;

            _baseLayerOffsets = new Vector2[4];
            _secondaryLayerOffsets = new Vector2[4];
            _inclineLayerOffsets = new Vector2[4];

            //noiseOffset = new Vector2(randomGen.Next(-1000, 1000), randomGen.Next(-1000, 1000));
            for (int i = 0; i < 4; i++)
            {
                _baseLayerOffsets[i] = new Vector2(in_chunk.RandomGenerator.Next(-1000, 1000), in_chunk.RandomGenerator.Next(-1000, 1000));
                _secondaryLayerOffsets[i] = new Vector2(in_chunk.RandomGenerator.Next(-1000, 1000), in_chunk.RandomGenerator.Next(-1000, 1000));
                _inclineLayerOffsets[i] = new Vector2(in_chunk.RandomGenerator.Next(-1000, 1000), in_chunk.RandomGenerator.Next(-1000, 1000));
            }
        }

        public void ResetOffsets()
        {
            _baseOffset = 0;
            _secondaryOffset = 0;
            _inclineOffset = 0;
        }

    }

    //boi which takes care of carving into the voxel cloud
    public class VoxelGenerator : MonoBehaviour
    {
        [SerializeField]
        private MainPathGenerator _pathGenerator = null;

        [SerializeField]
        private SimplePathGenerator _ceilingWormGenerator = null;

        [SerializeField]
        private SimplePathGenerator _pitWormGenerator = null;

       
        private Chunk _chunk = null;
        private VoxelData _voxels;
        private VoxelData _checkedVoxels;
        private MainPathOffsets _offsets;
        private PathData _mainPathData;
        //private PathData _ceilingWorms;

        private bool _sealEntrance = false;

        public bool SealEntrance { set => _sealEntrance = value; }

        [SerializeField]
        private RoomSettings _roomSettings = null;

        private List<int3> _roomTiles;

        public Action _VoxelsGenerated;

        private Coroutine _voxelGenerationCoroutine;

        //////////////////TEST///////////////

        private float3[] _posTest;

        private float[] _sizeTest;

        private float3[] _mainWormPathArray;

        private float[] _mainWormSizeArray;


        private void Update()
        {
            if (Input.GetKey(KeyCode.R))
            {
                //if (_chunk != null) _mainPathData = _pathGenerator.GenerateMainPath(_chunk, _offsets);
                //if (_chunk != null) GenerateCeilingPaths();
            }
        }
        public void SetUp(Chunk in_chunk)
        {
            _chunk = in_chunk;
            _offsets = new MainPathOffsets(_chunk);

            _voxels = new VoxelData(_chunk._settings._widthVoxels, _chunk._settings._heightVoxels, _chunk._settings._lengthVoxels);
            _checkedVoxels = new VoxelData(_chunk._settings._widthVoxels, _chunk._settings._heightVoxels, _chunk._settings._lengthVoxels);

            _voxelGenerationCoroutine = StartCoroutine(CO_GenerateVoxels());
        }

        private IEnumerator CO_GenerateVoxels()
        {
            //yield return new WaitForEndOfFrame();
            yield return null;

            //generate Path Data
            _mainPathData = _pathGenerator.GenerateMainPath(_chunk, _offsets);

            //copy 
            _mainWormPathArray = _mainPathData.PositionsToArray();
            _mainWormSizeArray = _mainPathData.SizeToArray();

            //reset offsets for next calculation
            _offsets.ResetOffsets();

            //carve main path
            RunWormOnMainPath(_mainPathData);
            //_voxels.SetAllValues(1);

            //assign main path points to chunk for Player Placement
            _chunk._wormPath = _mainWormPathArray;

            //yield return new WaitForEndOfFrame();

            //find "room" voxels
            FindRooms();

            //FindRoomsJobs();
            _chunk._roomPositions = GetRoomPositions();
            //FindRoomsJob();
            //yield return new WaitForEndOfFrame();

            //generate path data for ceiling worms
            GenerateCeilingWorms();
            GeneratePitWorms();

            RunWalkablePath();

            if (_sealEntrance) SealConnection(_chunk._entrance);

            yield return null;

            //make sure there are no floating pieces of mesh
            RemoveIslands();

            yield return null;

            //save voxels array to chunk... for reasons ?
            //_chunk._voxelMap = _voxels.GetArray();
            _chunk._VoxelMap1D = _voxels.GetPureArray();


            //get rid of native arrays
            _voxels.Dispose();
            _mainPathData.Dispose();
            _checkedVoxels.Dispose();

            //yield return new WaitForEndOfFrame();


            //finish voxel procedures
            if (_VoxelsGenerated != null) _VoxelsGenerated.Invoke();
        }

        public void SealConnection(Connection in_entrance)
        {
            CoordinateType in_type = CoordinateType.Vertical;

            switch (in_entrance._connectedSide)
            {
                case Side.Left:
                case Side.Right:
                    in_type = CoordinateType.Horizontal;
                    break;

                default:
                    break;
            }

            //relative position
            float3 _closestPoint = in_entrance._point - _chunk._startPos;

            _closestPoint = new float3(

            Mathf.CeilToInt(_closestPoint.x / _chunk._settings._widthOffset),
            Mathf.CeilToInt(_closestPoint.y / _chunk._settings._heightOffset),
            Mathf.CeilToInt(_closestPoint.z / _chunk._settings._lengthOffset)

            );


            if (in_type == CoordinateType.Horizontal)
            {
                int x = _closestPoint.x < 2 ? 0 : _voxels.Width - 1;
                for (int y = (int)(_closestPoint.y - 20); y <= (int)(_closestPoint.y + 20); y++)
                {

                    if (y < 0) continue;
                    if (y > _voxels.Height - 1) break;

                    for (int z = (int)(_closestPoint.z - 20); z <= (int)(_closestPoint.z + 20); z++)
                    {
                        if (z < 0) continue;
                        if (z > _voxels.Length - 1) break;

                        _voxels.SetValue(x, y, z, 0);
                    }
                }
            }
            else
            {
                int z = _closestPoint.z < 2 ? 0 : _voxels.Length - 1;
                for (int x = (int)(_closestPoint.x - 20); x <= (int)(_closestPoint.x + 20); x++)
                {

                    if (x < 0) continue;
                    if (x > _voxels.Width - 1) break;

                    for (int y = (int)(_closestPoint.y - 20); y <= (int)(_closestPoint.y + 20); y++)
                    {
                        if (y < 0) continue;
                        if (y > _voxels.Height - 1) break;

                        _voxels.SetValue(x, y, z, 0);
                    }
                }
            }

        }
        public void RunWormOnMainPath(PathData _path)
        {
            if (_path.ArrayLength == 0)
            {
                Debug.LogError("No Path to generate");
                return;
            }

            CarveJobs.CarveMainJob _carveJob = new CarveJobs.CarveMainJob
            {
                _length = 5,
                _offsets = new float3(_chunk._settings._widthOffset,
                                        _chunk._settings._heightOffset,
                                        _chunk._settings._lengthOffset),
                _startPosition = _chunk._startPos,
                VoxelDatas = _voxels,
                PathData = _mainPathData
            };

            JobHandle _handle = _carveJob.Schedule(_mainPathData.ArrayLength, 8);

            _handle.Complete();

        }

        private float3[] GetRoomPositions()
        {
            float3[] _roomPos = new float3[_roomTiles.Count];
            for (int i = 0; i < _roomTiles.Count; i++)
            {
                _roomPos[i] = IndexToWorldPos(_roomTiles[i].x, _roomTiles[i].y, _roomTiles[i].z);
            }

            return _roomPos;
        }

        private void RunWalkablePath()
        {
            CarveJobs.CarveNegativeMainJob _carveWalkJob = new CarveJobs.CarveNegativeMainJob
            {
                //_radius = 0.3f,
                _length = 8,
                _offsets = new float3(_chunk._settings._widthOffset,
                                    _chunk._settings._heightOffset,
                                    _chunk._settings._lengthOffset),
                _startPosition = _chunk._startPos,
                VoxelDatas = _voxels,
                PathData = _mainPathData
            };

            JobHandle _handle = _carveWalkJob.Schedule(_mainPathData.ArrayLength, 16);
            _handle.Complete();

        }

        //generate path data for ceilling worms
        private void GenerateCeilingWorms()
        {
            if (_roomTiles == null) return;

            int _ceilingsAmount = _chunk.RandomGenerator.Next(_roomSettings._ceilingWormAmountMin, _roomSettings._ceilingWormAmountMax);

            //get how many room tiiles should we skip to generate a worm
            int _voxelsForWorm = _roomTiles.Count / _ceilingsAmount;

            float3[] _posStart = new float3[_ceilingsAmount];
            float3[] _posEnd = new float3[_ceilingsAmount];

            //generate start points and end ppoints for worms according to room settings
            for (int i = 0; i < _ceilingsAmount; i++)
            {
                if (i * _voxelsForWorm > _roomTiles.Count - 1) break;
                _posStart[i] = IndexToWorldPos(_roomTiles[i * _voxelsForWorm].x,
                                                _roomTiles[i * _voxelsForWorm].y,
                                                _roomTiles[i * _voxelsForWorm].z);

                //random offset of the starting position
                _posStart[i] += math.normalize(
                                                math.float3(_chunk.RandomGenerator.Next(-100, 100) / 100f,
                                                            0,
                                                            _chunk.RandomGenerator.Next(-100, 100) / 100f)
                                               )
                                * _chunk.RandomGenerator.Next(0, (int)(_roomSettings._maxRandomOffsetCeil * 100f)) / 100f;

                _posEnd[i] = _posStart[i];
                _posEnd[i].y += (float)_chunk.RandomGenerator.Next(_roomSettings._ceilingsHeightMin * 10, _roomSettings._ceilingsHeightMax * 10) / 10f;
            }

            PathData _ceilingWorms = _ceilingWormGenerator.GenerateWormPaths(_posStart, _posEnd, _offsets);

            //save arrays for gizmos
            // _posTest = _ceilingWorms.PositionsToArray();
            // _sizeTest = _ceilingWorms.SizeToArray();

            CarveJobs.CarveSimpleJob _ceilingCarveJob = new CarveJobs.CarveSimpleJob
            {
                _length = 5,
                _offsets = new float3(_chunk._settings._widthOffset,
                                    _chunk._settings._heightOffset,
                                    _chunk._settings._lengthOffset),
                _startPosition = _chunk._startPos,
                VoxelDatas = _voxels,
                PathData = _ceilingWorms
            };

            JobHandle _handle = _ceilingCarveJob.Schedule(_ceilingWorms.ArrayLength, 8);
            _handle.Complete();
            _chunk._ceilingPath = _ceilingWorms.PositionsToArray();
            _ceilingWorms.Dispose();

        }

        //generate path data for ceilling worms
        private void GeneratePitWorms()
        {
            if (_roomTiles == null) return;

            int _pitsAmount = _chunk.RandomGenerator.Next(_roomSettings._pitsWormAmountMin, _roomSettings._pitsWormAmountMax);

            if (_pitsAmount == 0) return;

            //get how many room tiiles should we skip to generate a worm
            int _voxelsForWorm = _roomTiles.Count / _pitsAmount;

            float3[] _posStart = new float3[_pitsAmount];
            float3[] _posEnd = new float3[_pitsAmount];

            //generate start points and end ppoints for worms according to room settings
            for (int i = 0; i < _pitsAmount; i++)
            {
                if (i * _voxelsForWorm > _roomTiles.Count - 1) break;
                _posStart[i] = IndexToWorldPos(_roomTiles[i * _voxelsForWorm].x,
                                                _roomTiles[i * _voxelsForWorm].y,
                                                _roomTiles[i * _voxelsForWorm].z);

                //random offset of the starting position
                _posStart[i] += math.normalize(
                                                math.float3(_chunk.RandomGenerator.Next(-100, 100) / 100f,
                                                            0,
                                                            _chunk.RandomGenerator.Next(-100, 100) / 100f)
                                               )
                                * _chunk.RandomGenerator.Next(0, (int)(_roomSettings._maxRandomOffsetPits * 100f)) / 100f;

                _posEnd[i] = _posStart[i];
                _posEnd[i].y -= (float)_chunk.RandomGenerator.Next(_roomSettings._pitsDepthMin * 10, _roomSettings._pitsDepthMax * 10) / 10f;
            }

            PathData _pitWorms = _pitWormGenerator.GenerateWormPaths(_posStart, _posEnd, _offsets);

            //save arrays for gizmos
            // _posTest = _ceilingWorms.PositionsToArray();
            // _sizeTest = _ceilingWorms.SizeToArray();

            CarveJobs.CarveSimpleJob _pitCarveJob = new CarveJobs.CarveSimpleJob
            {
                _length = 5,
                _offsets = new float3(_chunk._settings._widthOffset,
                             _chunk._settings._heightOffset,
                             _chunk._settings._lengthOffset),
                _startPosition = _chunk._startPos,
                VoxelDatas = _voxels,
                PathData = _pitWorms
            };

            JobHandle _handle = _pitCarveJob.Schedule(_pitWorms.ArrayLength, 8);
            _handle.Complete();
            _chunk._pitPath = _pitWorms.PositionsToArray();
            _pitWorms.Dispose();

        }

        //find voxels which are part of the room according to the amount of filled neiighbours
        private void FindRooms()
        {
            _roomTiles = new List<int3>();
            int3 _coordinate;

            _checkedVoxels.Clear();

            //we go through all voxels trying to find one with enough neighbours
            for (int x = 15; x < _chunk._settings._widthVoxels - 15; x++)
            {
                for (int y = 15; y < _chunk._settings._heightVoxels - 15; y++)
                {
                    for (int z = 15; z < _chunk._settings._lengthVoxels - 15; z++)
                    {
                        //we only check filled voxels
                        if (_voxels.GetValue(x, y, z) < _chunk._settings._surface) continue;

                        if (_checkedVoxels.GetValue(x, y, z) == 1) continue;

                        _coordinate = new int3(x, y, z);

                        //check if has neighbours
                        if (CheckIfRoomTile(_coordinate))
                        {
                            _roomTiles.Add(_coordinate);

                            _checkedVoxels.SetValue(x, y, z, 1);

                        }

                    }
                }
            }

        }

        // private void FindRoomsJobs()
        // {
        //     _roomTiles = new List<int3>();
        //     //int3 _coordinate;

        //     _checkedVoxels.Clear();

        //     RoomJob _roomJob = new RoomJob
        //     {
        //         in_voxelDatas = _voxels,
        //         in_checkedVoxels = _checkedVoxels
        //     };

        //     JobHandle _roomHandle = _roomJob.Schedule(_voxels.Length, 8);
        //     _roomHandle.Complete();

        //     //get room coordinates from checked list
        //     for (int x = 0; x < _chunk._settings._widthVoxels; x++)
        //     {
        //         for (int y = 0; y < _chunk._settings._heightVoxels; y++)
        //         {
        //             for (int z = 0; z < _chunk._settings._lengthVoxels; z++)
        //             {
        //                 if (_checkedVoxels.GetValue(x, y, z) == 1)
        //                 {
        //                     _roomTiles.Add(new int3(x, y, z));
        //                 }
        //             }
        //         }
        //     }
        //     Debug.Log(_voxels.Length);
        //     Debug.Log(_roomTiles.Count);

        // }

        private void GetCeilingValues()
        {
            List<int3> _ceiling = new List<int3>();
            int3 _coordinate;
            for (int i = 0; i < _roomTiles.Count; i++)
            {
                _coordinate = _roomTiles[i];
                if (_coordinate.y > _chunk._settings._heightVoxels - 1) continue;
                if (_checkedVoxels.GetValue(_coordinate.x, _coordinate.y + 1, _coordinate.z) != 1) _ceiling.Add(_coordinate);
            }
            _roomTiles = _ceiling;
        }

        private void ExpandRooms()
        {
            int _oldCount = _roomTiles.Count;
            for (int i = 0; i < _oldCount; i++)
            {
                if (CheckEdgeTile(_roomTiles[i]))
                {
                    VoxelUtility.AddNeighbours(_roomTiles[i], _roomTiles, _voxels, _checkedVoxels, 1, _chunk._settings._surface);
                }
            }
        }

        private bool CheckIfRoomTile(int3 _coordinate)
        {
            int _neighbourAmount = VoxelUtility.GetNeighboursCount(_coordinate, _roomSettings._neighboursLevel, _roomSettings._valueForRoom, _roomSettings._conditionForRoom, _roomSettings._minNeighboursForRoomTile
            , _voxels, _checkedVoxels);
            return _neighbourAmount >= _roomSettings._minNeighboursForRoomTile ? true : false;
        }

        private bool CheckEdgeTile(int3 _coordinate)
        {
            int _neighbourAmount = VoxelUtility.GetNeighboursCount(_coordinate, 1, _chunk._settings._surface, ValueOpperation.Less, 2, _voxels, _checkedVoxels);
            return _neighbourAmount >= 1 ? true : false;
        }

        private Vector3 IndexToWorldPos(int x, int y, int z)
        {
            if (x > _chunk._settings._widthVoxels - 1) x = _chunk._settings._widthVoxels - 1;
            if (y > _chunk._settings._heightVoxels - 1) y = _chunk._settings._heightVoxels - 1;
            if (z > _chunk._settings._lengthVoxels - 1) z = _chunk._settings._lengthVoxels - 1;

            return new Vector3(x * _chunk._settings._widthOffset, y * _chunk._settings._heightOffset, z * _chunk._settings._lengthOffset) + _chunk._startPos;
        }

        private void RemoveIslands()
        {
            int index = 0;

            //find first empty voxel
            while (_voxels.GetValue(index) != 0)
            {
                index++;
            }

            //getting starting coordinate
            int3 coordinate = _voxels.IndexToCoordinate(index);

            //list of coordinates to check
            List<int3> _coordToCheck = new List<int3>();
            _coordToCheck.Add(coordinate);

            //list of checked voxels
            //VoxelData _checkedVoxels = new VoxelData(_chunk._settings._widthVoxels, _chunk._settings._heightVoxels, _chunk._settings._lengthVoxels);
            _checkedVoxels.Clear();
            _checkedVoxels.SetAllValues(0);

            //empty voxels
            VoxelData _emptyVoxels = new VoxelData(_chunk._settings._widthVoxels, _chunk._settings._heightVoxels, _chunk._settings._lengthVoxels);
            _checkedVoxels.SetAllValues(0);
            //int iter = 0;

            //find connected empty voxels until there is no valid coordinates to check
            while (_coordToCheck.Count > 0)
            {
                // if (iter > 10000)
                // {
                //     Debug.Log("Oh no an endless loop!");
                //     break;
                // }
                coordinate = _coordToCheck[0];

                _checkedVoxels.SetValue(coordinate.x, coordinate.y, coordinate.z, 1);

                //remove added checked voxel
                _coordToCheck.Remove(coordinate);

                // if (_voxels.GetValue(coordinate.x, coordinate.y, coordinate.z) > _chunk._settings._surface) continue;

                //add its unchecked neighbours
                VoxelUtility.AddNeighbours(coordinate, _coordToCheck, _voxels, _checkedVoxels, 1, _chunk._settings._surface - 0.3f, true);

                //make voxels empty
                _emptyVoxels.SetValue(coordinate.x, coordinate.y, coordinate.z, 1);

                //iter++;
            }

            //_checkedVoxels.Dispose();

            //check that all not filled voxels are over surface level
            // for (int x = 0; x < _chunk._settings._widthVoxels; x++)
            // {
            //     for (int y = 0; y < _chunk._settings._heightVoxels; y++)
            //     {
            //         for (int z = 0; z < _chunk._settings._lengthVoxels; z++)
            //         {

            //             if (_emptyVoxels.GetValue(x, y, z) != 1 && _voxels.GetValue(x, y, z) <= _chunk._settings._surface)
            //             {
            //                 _voxels.SetValue(x, y, z, 1);
            //             }
            //         }
            //     }
            // }

            CheckVoxels _checkEmptyJob = new CheckVoxels
            {
                VoxelData = _voxels,
                EmptyVoxels = _emptyVoxels
            };

            JobHandle _handle = _checkEmptyJob.Schedule(_voxels.Length, 32);
            _handle.Complete();

            _emptyVoxels.Dispose();

            //Debug.Log(_emptyVoxels.Length);
        }

        private void OnDrawGizmos()
        {
            // if (_mainWormPathArray == null) return;
            // for (int i = 0; i < _mainWormPathArray.Length; i++)
            // {
            //     Gizmos.DrawWireSphere(_mainWormPathArray[i], _mainWormSizeArray[i]);
            // }

            if (_chunk == null) return;

            //clamp bounda
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(_chunk._centerPos, new Vector3(

                _chunk._settings._widthVoxels * _chunk._settings._widthOffset,
                _chunk._settings._heightVoxels * _chunk._settings._heightOffset,
                _chunk._settings._lengthVoxels * _chunk._settings._lengthOffset

            ) - new Vector3(1, 1, 1) * _pathGenerator._pathSettings._pushFromBoundsThreshold);

            Gizmos.color = Color.magenta;

            foreach (Connection c in _chunk._connections)
            {
                Gizmos.DrawSphere(c._point + _chunk.GetConnectionNormal(c) * _pathGenerator._pathSettings._pushFromBoundsThreshold / 2, 0.3f);
            }

            if (_roomTiles == null) return;

            Gizmos.color = Color.red;

            for (int i = 0; i < _roomTiles.Count; i++)
            {
                Gizmos.DrawCube(new Vector3(
                    +_roomTiles[i].x * _chunk._settings._widthOffset,
                    +_roomTiles[i].y * _chunk._settings._heightOffset,
                    +_roomTiles[i].z * _chunk._settings._lengthOffset
                ) + _chunk._startPos,

                Vector3.one * 0.05f);
            }

            // if (_posTest == null) return;
            // Gizmos.color = Color.red;
            // for (int i = 0; i < _posTest.Length; i++)
            // {
            //     // Gizmos.color = Color.red;
            //     Gizmos.DrawSphere(_posTest[i], _sizeTest[i]);
            // }
        }

        //[BurstCompile]
        private struct RoomJob : IJobParallelFor
        {
            public VoxelData in_voxelDatas;
            [NativeDisableParallelForRestriction] public VoxelData in_checkedVoxels;


            //public VoxelData CheckedVoxels { set => _checkedVoxels = value; get => _checkedVoxels; }

            public void Execute(int index)
            {
                //get coordinates
                int3 _coordinate = in_voxelDatas.IndexToCoordinate(index);
                in_checkedVoxels.SetValue(_coordinate.x, _coordinate.y, _coordinate.z, 1);

            }

        }

        private struct CheckVoxels : IJobParallelFor
        {
            //public VoxelData in_voxelDatas;

            [NativeDisableParallelForRestriction] public VoxelData _voxelData;
            public VoxelData VoxelData { set => _voxelData = value; get => _voxelData; }
            [NativeDisableParallelForRestriction] public VoxelData _emptyVoxels;
            public VoxelData EmptyVoxels { set => _emptyVoxels = value; get => _emptyVoxels; }

            public float _surfaceValue;

            private int3 _coordinate;
            public void Execute(int index)
            {
                _coordinate = _voxelData.IndexToCoordinate(index);

                if (_emptyVoxels.GetValue(_coordinate.x, _coordinate.y, _coordinate.z) != 1 && _voxelData.GetValue(_coordinate.x, _coordinate.y, _coordinate.z) <= _surfaceValue)
                {
                    _voxelData.SetValue(_coordinate.x, _coordinate.y, _coordinate.z, 1);
                }

            }

        }


    }
}

