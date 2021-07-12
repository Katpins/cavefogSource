using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace caveFog
{
    public static class VoxelUtility
    {
        public static void AddNeighbours(int3 _coordinate, IList<int3> _coordinateList, VoxelData _voxels, VoxelData _checkedVoxels, int _level, float _surfaceValue, bool _isEmpty = false)
        {
            bool _isValid = false;
            for (int x = _coordinate.x - _level; x <= _coordinate.x + _level; x++)
            {
                if (x < 0) continue;
                if (x > _voxels.Width - 1) break;

                for (int y = _coordinate.y - _level; y <= _coordinate.y + _level; y++)
                {
                    if (y < 0) continue;
                    if (y > _voxels.Height - 1) break;
                    for (int z = _coordinate.z - _level; z <= _coordinate.z + _level; z++)
                    {
                        if (z < 0) continue;
                        if (z > _voxels.Length - 1) break;

                        if (z == _coordinate.z && y == _coordinate.y && x == _coordinate.x) continue;

                        if (_checkedVoxels.GetValue(x, y, z) != 0) continue;

                        _isValid = _isEmpty ? _voxels.GetValue(x, y, z) <= _surfaceValue : _voxels.GetValue(x, y, z) >= _surfaceValue;

                        if (_isValid)
                        {
                            _coordinateList.Add(new int3(x, y, z));

                            _checkedVoxels.SetValue(x, y, z, 1);
                        }

                    }
                }
            }

        }

        public static int GetNeighboursCount(int3 _coordinate, int _level, float _value, ValueOpperation _condition, int _amountThreshold, VoxelData _voxels, VoxelData _checkedVoxels)
        {
            int _count = 0;
            for (int x = _coordinate.x - _level; x <= _coordinate.x + _level; x++)
            {
                if (x < 0) continue;
                if (x > _voxels.Width - 1) break;

                for (int y = _coordinate.y - _level; y <= _coordinate.y + _level; y++)
                {
                    if (y < 0) continue;
                    if (y > _voxels.Height - 1) break;
                    for (int z = _coordinate.z - _level; z <= _coordinate.z + _level; z++)
                    {
                        if (z < 0) continue;
                        if (z > _voxels.Length - 1) break;

                        if (z == _coordinate.z && y == _coordinate.y && x == _coordinate.x) continue;

                        if (_checkedVoxels.GetValue(x, y, z) == 1) _count++;

                        switch (_condition)
                        {
                            case ValueOpperation.Equal:
                                _count = _voxels.GetValue(x, y, z) == _value ? _count + 1 : _count;
                                break;

                            case ValueOpperation.Less:
                                _count = _voxels.GetValue(x, y, z) < _value ? _count + 1 : _count;
                                break;

                            case ValueOpperation.More:
                                _count = _voxels.GetValue(x, y, z) > _value ? _count + 1 : _count;
                                break;

                            case ValueOpperation.LessE:
                                _count = _voxels.GetValue(x, y, z) <= _value ? _count + 1 : _count;
                                break;

                            case ValueOpperation.MoreE:
                                _count = _voxels.GetValue(x, y, z) >= _value ? _count + 1 : _count;
                                break;
                            case ValueOpperation.Not:
                                _count = _voxels.GetValue(x, y, z) != _value ? _count + 1 : _count;
                                break;

                        }

                        if (_count >= _amountThreshold) return _count;
                    }

                }
            }
            return _count;
        }

    }

}
