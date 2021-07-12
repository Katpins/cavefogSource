using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

namespace caveFog
{
    public static class CarveJobs
    {
        //carve job for main Path
        [BurstCompile]
        public struct CarveMainJob : IJobParallelFor
        {
            public ushort _length;
            public float3 _startPosition;
            public float3 _offsets;

            [ReadOnly] private PathData _pathData;
            public PathData PathData { set => _pathData = value; }

            [NativeDisableParallelForRestriction] private VoxelData _voxelDatas;
            public VoxelData VoxelDatas { set => _voxelDatas = value; get => _voxelDatas; }

            public void Execute(int index)
            {

                float _distance = 0;
                //float _modDistance = 0;

                float _density = 0;

                float3 _relativePos;
                float3 _modRelativePos;

                //relative position
                float3 _closestPoint = _pathData.GetPosition(index) - _startPosition;

                _closestPoint = new float3(

                Mathf.CeilToInt(_closestPoint.x / _offsets.x),
                Mathf.CeilToInt(_closestPoint.y / _offsets.y),
                Mathf.CeilToInt(_closestPoint.z / _offsets.z)

                );

                for (int x = (int)(_closestPoint.x - _length); x <= (int)(_closestPoint.x + _length); x++)
                {
                    if (x < 0) continue;
                    if (x > _voxelDatas.Width - 1) break;
                    for (int y = (int)(_closestPoint.y - _length); y <= (int)(_closestPoint.y + _length); y++)
                    {

                        if (y < 0) continue;
                        if (y > _voxelDatas.Height - 1) break;

                        for (int z = (int)(_closestPoint.z - _length); z <= (int)(_closestPoint.z + _length); z++)
                        {
                            if (z < 0) continue;
                            if (z > _voxelDatas.Length - 1) break;

                            // if (z == _voxelDatas.Length - 1 || z == 0)
                            // {
                            //     _voxelDatas.AddValue(x, y, z, 1);
                            //     continue;
                            // }
                            // if (x == _voxelDatas.Width - 1 || x == 0)
                            // {
                            //     _voxelDatas.AddValue(x, y, z, 1);
                            //     continue;
                            // }
                            // if (y == _voxelDatas.Height - 1 || y == 0)
                            // {
                            //     _voxelDatas.AddValue(x, y, z, 1);
                            //     continue;
                            // }

                            _relativePos = IndexToWorldPos(x, y, z) - _pathData.GetPosition(index);

                            _modRelativePos = _relativePos;
                            if (y >= _closestPoint.y) _modRelativePos.y *= 1.3f;
                            else _modRelativePos.y *= 1.5f;

                            _distance = math.length(_modRelativePos);

                            if (_distance < _pathData.GetRadius(index))
                            {
                                _density = _pathData.GetDensityFromDistance(index, _distance);
                                // _voxelDatas.AddValue(x, y, z, _relativePos.y <  (-_pathData.GetRadius(index) * 0.5f)? -_density : _density);

                                _voxelDatas.AddValue(x, y, z, _density);
                            }

                        }
                    }
                }
            }

            private float3 IndexToWorldPos(int x, int y, int z)
            {
                return new float3(x * _offsets.x, y * _offsets.y, z * _offsets.z) + _startPosition;
            }

        }
        //carve job for main Path
        [BurstCompile]
        public struct CarveNegativeMainJob : IJobParallelFor
        {
            public float _radius;
            public ushort _length;
            public float3 _startPosition;
            public float3 _offsets;

            [NativeDisableParallelForRestriction, ReadOnly] private PathData _pathData;
            public PathData PathData { set => _pathData = value; }

            [NativeDisableParallelForRestriction] private VoxelData _voxelDatas;
            public VoxelData VoxelDatas { set => _voxelDatas = value; get => _voxelDatas; }

            public void Execute(int index)
            {
                _radius = 0.25f;
                // float3 _modPosition = _pathData.GetPosition(index);
                // _modPosition.y -= 0.4f;
                if ((_pathData.GetPosition(index + 2) - _pathData.GetPosition(index - 2)).y < -0.35f || (_pathData.GetPosition(index + 3) - _pathData.GetPosition(index - 3)).y > 0.3f) return;

                int3 _centerVoxelCurrent;
                // if (_centerVoxelCurrent.y - 1 < 0) return;
                // if (_voxelDatas.GetValue(_centerVoxelCurrent.x, _centerVoxelCurrent.y - 1, _centerVoxelCurrent.z) > 0.75f) return;
                //int3 _centerVoxelCurrent = GetClosestPoint(_pathData.GetPosition(index) + math.float3(0, -_pathData.GetRadius(index) * 0.8f, 0));

                //if (!CheckIfOnPit(_centerVoxelCurrent.x, _centerVoxelCurrent.y, _centerVoxelCurrent.z)) return;


                // int3 _startPos = index > 0 ? GetClosestPoint(_pathData.GetPosition(index - 1) - math.float3(0, -0.4f, 0))
                // : GetClosestPoint(_pathData.GetPosition(index) * 2 -_pathData.GetPosition(index + 1) - math.float3(0, -0.4f, 0));
                // int3 _endPos =  index < _pathData.ArrayLength ? GetClosestPoint(_pathData.GetPosition(index + 1) - math.float3(0, -0.4f, 0))
                // : GetClosestPoint(_pathData.GetPosition(index) * 2 -_pathData.GetPosition(index - 1) - math.float3(0, -0.4f, 0));

                // float3 _startPos = _pathData.GetPosition(index) * 2 - _pathData.GetPosition(index + 5) + math.float3(0, -0.58f, 0);
                // float3 _endPos = _pathData.GetPosition(index) * 2 - _pathData.GetPosition(index - 5) + math.float3(0, -0.58f, 0);
                // float3 _startPos = _pathData.GetPosition(index) * 2 - _pathData.GetPosition(index + 3) + math.float3(0, -_pathData.GetRadius(index) * 0.8f, 0);
                // float3 _endPos = _pathData.GetPosition(index) * 2 - _pathData.GetPosition(index - 3) + math.float3(0, -_pathData.GetRadius(index) * 0.8f, 0);

                float3 _startPos = _pathData.GetPosition(index + 2) + math.float3(0, -_pathData.GetRadius(index) * 0.87f, 0);
                float3 _endPos = _pathData.GetPosition(index - 2) + math.float3(0, -_pathData.GetRadius(index) * 0.87f, 0);
                float3 _pos;


                for (int i = 0; i < 10; i++)
                {
                    _pos = _startPos + (_endPos - _startPos) * (float)i / 10f;
                    _centerVoxelCurrent = GetClosestPoint(_pos);
                    if (_centerVoxelCurrent.y - 2 < 0) continue;
                    if (_voxelDatas.GetValue(_centerVoxelCurrent.x, _centerVoxelCurrent.y - 3, _centerVoxelCurrent.z) < 0.7f) continue;
                    Carve(_pos);
                }
                //Carve(_pathData.GetPosition(index) + math.float3(0, -_pathData.GetRadius(index) * 0.8f, 0));

            }

            private void Carve(float3 _pos)
            {
                float3 _relativePos;
                float3 _modRelativePos;
                //float _distance = 0;
                float _modDistance = 0;
                float _density = 0;

                int3 _closestPoint = GetClosestPoint(_pos);

                for (int x = (_closestPoint.x - _length); x <= (_closestPoint.x + _length); x++)
                {
                    if (x < 0) continue;
                    if (x > _voxelDatas.Width - 1) break;
                    for (int y = (_closestPoint.y - _length); y <= (_closestPoint.y + _length); y++)
                    {

                        if (y < 0) continue;
                        if (y > _voxelDatas.Height - 1) break;

                        for (int z = (_closestPoint.z - _length); z <= (_closestPoint.z + _length); z++)
                        {
                            if (z < 0) continue;
                            if (z > _voxelDatas.Length - 1) break;

                            _relativePos = IndexToWorldPos(x, y, z) - _pos;

                            _modRelativePos = _relativePos;
                            if (y >= _closestPoint.y) _modRelativePos.y *= 3.5f;
                            else _modRelativePos.y *= 3.2f;

                            //_distance = math.length(_relativePos);
                            _modDistance = math.length(_modRelativePos);

                            if (_modDistance <= _radius)
                            {
                                _density = _pathData.GetDensityFromRadius(_radius, _modDistance);
                                // _voxelDatas.AddValue(x, y, z, _relativePos.y <  (-_pathData.GetRadius(index) * 0.5f)? -_density : _density);

                                _voxelDatas.AddValue(x, y, z, -_density * 3f);
                                //_voxelDatas.AddValue(x, y, z, -math.lerp(_radius, 0.2f, _modDistance) * 1.5f);
                            }

                        }
                    }
                }
            }

            private bool CheckIfOnPit(int x, int y, int z)
            {
                //int countFilled = 0;
                if (y < 6) return false;
                // if (_voxelDatas.GetValue(x, y - 15, z) > 0.8f) return true;
                // else return false;

                for (int i = 1; i < 6; i++)
                {
                    if (_voxelDatas.GetValue(x, y - i, z) < 0.85f) return false;
                }

                return true;
            }

            //private rotate
            private float3 IndexToWorldPos(int x, int y, int z)
            {
                return new float3(x * _offsets.x, y * _offsets.y, z * _offsets.z) + _startPosition;
            }

            private int3 GetClosestPoint(float3 _point)
            {
                //relative position
                float3 _closestPoint = _point - _startPosition;

                _closestPoint = new float3(

                Mathf.CeilToInt(_closestPoint.x / _offsets.x),
                Mathf.CeilToInt(_closestPoint.y / _offsets.y),
                Mathf.CeilToInt(_closestPoint.z / _offsets.z)

                );

                return (int3)_closestPoint;

            }
        }

        //carve job for ceiling worms
        [BurstCompile]
        public struct CarveSimpleJob : IJobParallelFor
        {

            public ushort _length;
            public float3 _startPosition;
            public float3 _offsets;

            [ReadOnly] private PathData _pathData;
            public PathData PathData { set => _pathData = value; }
            [NativeDisableParallelForRestriction] private VoxelData _voxelDatas;
            public VoxelData VoxelDatas { set => _voxelDatas = value; get => _voxelDatas; }

            public void Execute(int index)
            {
                float _distance = 0;
                float _density = 0;
                float3 _relativePos;

                //relative position
                float3 _closestPoint = _pathData.GetPosition(index) - _startPosition;

                _closestPoint = new float3(

                Mathf.CeilToInt(_closestPoint.x / _offsets.x),
                Mathf.CeilToInt(_closestPoint.y / _offsets.y),
                Mathf.CeilToInt(_closestPoint.z / _offsets.z)

                );

                for (int x = (int)(_closestPoint.x - _length); x <= (int)(_closestPoint.x + _length); x++)
                {
                    if (x < 0) continue;
                    if (x > _voxelDatas.Width - 1) break;
                    for (int y = (int)(_closestPoint.y - _length); y <= (int)(_closestPoint.y + _length); y++)
                    {

                        if (y < 0) continue;
                        if (y > _voxelDatas.Height - 1) break;

                        for (int z = (int)(_closestPoint.z - _length); z <= (int)(_closestPoint.z + _length); z++)
                        {
                            if (z < 0) continue;
                            if (z > _voxelDatas.Length - 1) break;

                            if (z == 0 || z == _voxelDatas.Length - 1 ||
                                y == 0 || y == _voxelDatas.Height - 1 ||
                                x == 0 || x == _voxelDatas.Width - 1)
                            {
                                _voxelDatas.SetValue(x, y, z, 0);
                                continue;
                            }

                            _relativePos = IndexToWorldPos(x, y, z) - _pathData.GetPosition(index);

                            if (_voxelDatas.GetValue(x, y, z) < 0) return;

                            _distance = math.length(_relativePos);

                            if (_distance < _pathData.GetRadius(index))
                            {
                                _density = _pathData.GetDensityFromDistance(index, _distance);
                                _voxelDatas.AddValue(x, y, z, _density);
                            }

                        }
                    }
                }
            }

            private float3 IndexToWorldPos(int x, int y, int z)
            {
                return new float3(x * _offsets.x, y * _offsets.y, z * _offsets.z) + _startPosition;
            }
        }
    }

}
