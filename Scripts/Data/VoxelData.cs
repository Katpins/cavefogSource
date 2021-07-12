using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace caveFog
{
    public struct VoxelData : IDisposable
    {
        [NativeDisableParallelForRestriction] private NativeArray<float> _voxels;

        private int width, height, length;

        public int Width { get => width; }
        public int Height { get => height; }
        public int Length { get => length; }

        public VoxelData(int in_width, int in_height, int in_length)
        {
            width = in_width;
            height = in_height;
            length = in_length;

            _voxels = new NativeArray<float>(width * height * length, Allocator.Persistent);
        }

        public float GetValue(int x, int y, int z)
        {
            return _voxels[x +
                            y * width +
                            z * width * height];
        }

        public float GetValue(int index)
        {
            return _voxels[index];
        }

        public void SetValue(int x, int y, int z, float _value)
        {
            _voxels[x +
                    y * width +
                    z * width * height] = _value;

            // _voxels[x +
            //             y * width +
            //             z * width * height] = math.round(_voxels[x + y * width + z * width * height] * 10f) / 10f;
        }
        public void SetAllValues(float _value)
        {
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    for (int y = 0; y < length; y++)
                    {
                        SetValue(x, y, z, _value);
                    }
                }
            }
        }
        public void AddValue(int x, int y, int z, float _value)
        {
            if ((_voxels[x +
                    y * width +
                    z * width * height] + _value) < 0)
                _voxels[x +
                y * width +
                z * width * height] = 0;
            else
                _voxels[x +
                        y * width +
                        z * width * height] += _value;

            //round the value
            // _voxels[x +
            //             y * width +
            //             z * width * height] = math.round(_voxels[x + y * width + z * width * height] * 10f) / 10f;

        }

        public float[,,] GetArray()
        {
            float[,,] _array = new float[width, height, length];

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    for (int y = 0; y < length; y++)
                    {
                        _array[x, y, z] = GetValue(x, y, z);
                    }
                }
            }

            return _array;
        }

        public float[] GetPureArray()
        {
            return _voxels.ToArray();
        }

        public void Clear()
        {
            _voxels.Dispose();
            _voxels = new NativeArray<float>(width * height * length, Allocator.Persistent);
        }
        public int3 IndexToCoordinate(int index)
        {
            int3 coordinate = new int3(
                index % width,
                (index / width) % height,
                index / (width * height));

            return coordinate;
        }

        public void Dispose()
        {
            _voxels.Dispose();
        }

        public void NormalizeValues(float _maxValue = 1f)
        {
            for (int i = 0; i < _voxels.Length; i++)
            {
                if (_voxels[i] > 1) _voxels[i] = 1;
                if (_voxels[i] < 0) _voxels[i] = 0;
            }
        }
    }

}
