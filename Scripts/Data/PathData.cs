using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;
using Unity.Mathematics;

namespace caveFog
{
    public struct PathData : IDisposable
    {
        private NativeArray<float3> _pathPoints;
        private NativeArray<float> _pathRadius;
        private float _baseDensity;
        public float BaseDensity { get => _baseDensity; }
        private NativeArray<float> _densityArray;
        private int _length;
        public int ArrayLength { get => _length; }

        public PathData(int i = 0)
        {
            _length = i;

            _pathPoints = new NativeArray<float3>(i, Allocator.Temp);
            _pathRadius = new NativeArray<float>(i, Allocator.Temp);
            _densityArray = new NativeArray<float>(i, Allocator.Temp);

            _baseDensity = 0;
        }

        public PathData(float3[] in_pathPoints, float[] in_pathRadius, float[] in_density, float in_baseDensity)
        {
            _length = in_pathPoints.Length;

            _pathPoints = new NativeArray<float3>(in_pathPoints.Length, Allocator.Persistent);
            _pathRadius = new NativeArray<float>(in_pathRadius.Length, Allocator.Persistent);
            _densityArray = new NativeArray<float>(in_density.Length, Allocator.Persistent);

            _baseDensity = in_baseDensity;

            _pathRadius.CopyFrom(in_pathRadius);
            _pathPoints.CopyFrom(in_pathPoints);
            _densityArray.CopyFrom(in_density);
        }

        public float3 GetPosition(int index)
        {
            if (index > _pathPoints.Length - 1) return _pathPoints[_pathPoints.Length - 1];
            if (index < 0) return _pathPoints[0];
            return _pathPoints[index];
        }

        public float GetRadius(int index)
        {
            return _pathRadius[index];
        }

        public void Dispose()
        {
            _pathPoints.Dispose();
            _pathRadius.Dispose();
            _densityArray.Dispose();
        }

        public float3[] PositionsToArray()
        {
            return _pathPoints.ToArray();
        }

        public float[] SizeToArray()
        {
            return _pathRadius.ToArray();
        }

        public float GetDensityFromDistance(int index, float in_distance)
        {
            int _densityIndex = (int)math.ceil((_pathRadius[index] - in_distance) / (_pathRadius[index]) * (_densityArray.Length - 1));

            return _densityArray[_densityIndex] * _baseDensity;
        }

        public float GetDensityFromRadius(float radius, float in_distance)
        {
            int _densityIndex = (int)math.ceil((radius - in_distance) / (radius) * (_densityArray.Length - 1));

            return _densityArray[_densityIndex] * _baseDensity;
        }

    }

}
