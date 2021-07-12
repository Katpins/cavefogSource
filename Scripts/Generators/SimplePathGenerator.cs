using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

namespace caveFog
{
    [CreateAssetMenu(menuName = "Path Generators/Simple Path Generator")]
    public class SimplePathGenerator : PathGenerator
    {
        public SimplePathSettings _pathSettings;

        public WormSettings _wormSettings;

        public PathData GenerateWormPaths(float3[] _startPos, float3[] _endPos, MainPathOffsets _offsets)
        {

            if (_startPos.Length != _endPos.Length)
            {
                Debug.LogError("Start/End positions arent set up properly");
                return new PathData();
            }

            List<float3> _positions = new List<float3>();
            List<float> _radius = new List<float>();

            for (int i = 0; i < _startPos.Length; i++)
            {
                GenerateSinglePath(_startPos[i], _endPos[i], _positions, _radius, _offsets);
            }

            return new PathData(_positions.ToArray(), _radius.ToArray(), _wormSettings.DensityArray, _wormSettings.BaseDensity);

        }

        private void GenerateSinglePath(float3 _start, float3 _end, IList<float3> _outputPositions, IList<float> _outputRadius, MainPathOffsets _offsets)
        {
            //List<float3> _positions = new List<float3>();
            float3 prevPoint = _start;
            int i = 0;

            float3 force = math.normalize(_end - _start);
            float3 curveForce01 = force;
            float3 curveForce02 = force;
            //float3 curveForceV = force;


            float _distance = math.length(_end - _start);
            float curveStrength = 0;


            float _remainingDistance = math.length(_end - prevPoint);

            while (_remainingDistance > _pathSettings._stepLength)
            {
                _outputPositions.Add(prevPoint);

                if (i > 5000)
                {
                    Debug.LogError("To much positions! Are you sure this line will reach the end?");
                    break;
                }

                force = DirectForce(prevPoint, _end);

                curveStrength = _pathSettings._curveStrength.Evaluate((float)i * (float)_pathSettings._stepLength / _distance);

                curveForce01 = BaseCurveForce(curveForce01, _offsets, _pathSettings._yRotationNoise);
                curveForce02 = BaseCurveForce(curveForce02, _offsets, _pathSettings._xRotationNoise, true);

                force = force * (1f - curveStrength) + math.normalize(curveForce01 * _pathSettings._yRotationStrength + curveForce02 * _pathSettings._xRotationStrength) * curveStrength;

                prevPoint += math.normalize(force) * _pathSettings._stepLength;

                _remainingDistance = math.length(_end - prevPoint);

                i++;
            }

            float _noiseStrength;
            float _wormSize;

            for (int j = 0; j < i; j++)
            {
                _wormSize = math.lerp(_wormSettings.MinSize, _wormSettings.MaxSize, _wormSettings.SizeOverPathBase.Evaluate((float)j / (float)i));

                _noiseStrength = _wormSettings.NoiseOverPath.Evaluate((float)j / (float)i);

                _wormSize = _wormSize * (1f - _noiseStrength) + BaseSizeNoise(_wormSettings.MinSize, _wormSettings.MaxSize, _offsets, _wormSettings._sizeNoise) * _noiseStrength;

                _outputRadius.Add(_wormSize);
            }

        }
    }
}

