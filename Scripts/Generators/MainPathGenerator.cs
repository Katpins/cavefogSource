using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace caveFog
{

    [CreateAssetMenu(menuName = "Path Generators/Main Path Generator")]
    //object which contains all calculations for path creation
    public class MainPathGenerator : PathGenerator
    {
        //ref to settings of main path gen
        //[HideInInspector]
        public MainPathSettings _pathSettings;
        //private Chunk _currentChunk;
        public WormSettings _wormSettings;

        public PathData GenerateMainPath(Chunk in_chunk, MainPathOffsets in_offsets)
        {
            //_currentChunk = in_chunk;

            for (int i = 0; i < in_chunk._connections.Count; i++)
            {
                if (in_chunk._connections[i] != in_chunk._entrance)
                    return GenerateSinglePath(in_chunk, in_offsets, in_chunk._connections[i]);
            }

            return new PathData();
        }

        private PathData GenerateSinglePath(Chunk _chunk, MainPathOffsets _offsets, Connection _exit)
        {

            List<Vector3> _pathPoints = new List<Vector3>();

            float[] _pathRadius;

            Vector3 _end = _exit._point;
            Vector3 _start = _chunk._entrance._point;

            _pathPoints.Add(_start);

            //calcullate points when entering "safe" part of chunk
            Vector3 _preEnd = _end + 1.5f * _chunk.GetConnectionNormal(_exit) * _pathSettings._pushFromBoundsThreshold / 2;
            bool _reachedPreEnd = false;

            Vector3 _preStart = _start + 1.5f * _chunk.GetConnectionNormal(_chunk._entrance) * _pathSettings._pushFromBoundsThreshold / 2;
            bool _reachedPreStart = false;

            Vector3 prevPoint = _start;
            Vector3 newPoint = _start;
            int i = 0;
            //float _distance = (_end - _start).magnitude;
            float _remainingDistance = (_end - prevPoint).magnitude;

            float curveStrength = 0;
            float centerPullStrength = 0;
            Vector3 force;

            Vector3 noiseForce = _end - _start;
            Vector3 baseCurve = _end - _start;

            //repeat until we reach end point
            while (_remainingDistance > _pathSettings._stepLength)
            {
                _pathPoints.Add(prevPoint);
                if (_pathPoints.Count > 5000)
                {
                    Debug.LogError("To much positions! Are you sure this line will reach the end?");
                    break;
                }

                //adding direct force towards the exit
                if ((_preEnd - prevPoint).sqrMagnitude < _pathSettings._stepLength / 5f) _reachedPreEnd = true;
                if ((_preStart - prevPoint).sqrMagnitude < _pathSettings._stepLength / 5f) _reachedPreStart = true;

                if (!_reachedPreStart) force = DirectForce(prevPoint, _preStart);
                else if (!_reachedPreEnd) force = DirectForce(prevPoint, _preEnd);
                else force = DirectForce(prevPoint, _end);
                //force = DirectForce(prevPoint, _reachedPreStart ? _reachedPreEnd ? _end : _preEnd : _preStart)
                //calculate the amount of curviness depending on distnce. So the path will find the end

                curveStrength = _pathSettings._noiseCurveToDistance.Evaluate((float)i / _pathSettings._iterationThreshold);
                if (!_pathSettings._disableCenterPull) centerPullStrength = _pathSettings._centerPullToDistance.Evaluate((float)i / (float)_pathSettings._iterationThreshold) * _pathSettings._centerPullAmount;


                baseCurve = BaseCurveForce(baseCurve, _offsets, _pathSettings._basePathCurve);

                //getting smaler noise layer
                noiseForce = (NoiseForce(noiseForce, _offsets, _pathSettings._secondaryCurve, _pathSettings._inclineCurve) * _pathSettings._secondaryNoiseToDistance.Evaluate((float)i / (float)_pathSettings._iterationThreshold)).normalized;//((float)i / (float)_pathSettings._iterationThreshold);

                // noiseForce.x *= _pathSettings._noiseStrength;
                // noiseForce.z *= _pathSettings._noiseStrength;
                // noiseForce.y *= _pathSettings._inclineStrength;

                noiseForce = new Vector3(noiseForce.x * _pathSettings._noiseStrength, noiseForce.y * _pathSettings._inclineStrength, noiseForce.z * _pathSettings._noiseStrength);

                force = force * (1f - curveStrength) + baseCurve * curveStrength;

                newPoint = prevPoint + (force.normalized + noiseForce + RestrainForce(prevPoint, _chunk) * centerPullStrength) * _pathSettings._stepLength;
                if (newPoint.y - prevPoint.y > _pathSettings._yStepRestriction)
                {
                    newPoint.y = prevPoint.y + _pathSettings._yStepRestriction;
                }
                prevPoint = newPoint;
                prevPoint = RestrictPosition(prevPoint, (float)i / (float)_pathSettings._iterationThreshold, _chunk._settings._maxValues, _chunk);

                _remainingDistance = (_end - prevPoint).magnitude;

                i++;
            }

            _pathPoints.Add(_end);
            _pathPoints.Add(_end);

            _pathRadius = new float[_pathPoints.Count];

            float3[] _path = new float3[_pathPoints.Count];

            float _wormSize = 0;
            float _noiseStrength = 0;

            for (i = 0; i < _pathPoints.Count; i++)
            {
                _wormSize = math.lerp(_wormSettings.MinSize, _wormSettings.MaxSize, _wormSettings.SizeOverPathBase.Evaluate((float)i / (float)_pathPoints.Count));

                _noiseStrength = _wormSettings.NoiseOverPath.Evaluate((float)i / (float)_pathPoints.Count);

                _wormSize = _wormSize * (1f - _noiseStrength) + BaseSizeNoise(_wormSettings.MinSize, _wormSettings.MaxSize, _offsets, _wormSettings._sizeNoise) * _noiseStrength;

                _pathRadius[i] = _wormSize;
                _path[i] = _pathPoints[i];
            }


            return new PathData(_path, _pathRadius, _wormSettings.DensityArray, _wormSettings.BaseDensity);
        }

        private Vector3 RestrainForce(Vector3 _position, Chunk _chunk)
        {
            Vector3 _force = _chunk._centerPos - _position;
            return _force.normalized;
        }
        //stop path from exiting the chunk
        private Vector3 RestrictPosition(Vector3 _position, float _damp, Vector3 _maxValues, Chunk _chunk)
        {
            Vector3 _pushAway = Vector3.zero;

            Vector3 _relativePosition = _position - _chunk._centerPos;

            //identify "pushing" vectors
            if (_maxValues.x - Mathf.Abs(_relativePosition.x) < _pathSettings._pushFromBoundsThreshold)
            {
                _pushAway.x = -Mathf.Sign(_relativePosition.x) * _pathSettings._pushFromBoundsStrength * _pathSettings._pushApplicationCurve.Evaluate((_maxValues.x - Mathf.Abs(_relativePosition.x)) / _pathSettings._pushFromBoundsThreshold); ;
            }
            if (_maxValues.y - Mathf.Abs(_relativePosition.y) < _pathSettings._pushFromBoundsThreshold)
            {
                _pushAway.y = -Mathf.Sign(_relativePosition.y) * _pathSettings._pushFromBoundsStrength * _pathSettings._pushApplicationCurve.Evaluate((_maxValues.y - Mathf.Abs(_relativePosition.y)) / _pathSettings._pushFromBoundsThreshold);
            }
            if (_maxValues.z - Mathf.Abs(_relativePosition.z) < _pathSettings._pushFromBoundsThreshold)
            {
                _pushAway.z = -Mathf.Sign(_relativePosition.z) * _pathSettings._pushFromBoundsStrength * _pathSettings._pushApplicationCurve.Evaluate((_maxValues.z - Mathf.Abs(_relativePosition.z)) / _pathSettings._pushFromBoundsThreshold); ;
            }

            return _pushAway * _pathSettings._pushDampCurve.Evaluate(_damp) + _position;
        }

        // //base force from start to end
        // private Vector3 DirectForce(Vector3 in_start, Vector3 in_end)
        // {
        //     //direction from stert to end
        //     Vector3 _directForce = (in_end - in_start);

        //     return _directForce.normalized;
        // }

        // //base force to add curviness to base path
        // private Vector3 BaseCurveForce(Vector3 baseForce, float distance, MainPathOffsets _offsets, NoiseCurveSettings _noise)
        // {
        //     float _angle = 0;

        //     _angle = NoiseFunction(1, _offsets._baseOffset, _noise, _offsets._baseLayerOffsets);

        //     //assign max turn
        //     _angle = Mathf.Lerp(-_noise._maxAngle, _noise._maxAngle, _angle);

        //     //rotate direction vector
        //     baseForce = Quaternion.Euler(0, _angle, 0) * baseForce;

        //     //change offset for the next step
        //     _offsets._baseOffset += 1;

        //     return baseForce.normalized;

        // }

        //curl noise force
        private Vector3 NoiseForce(Vector3 in_direction, MainPathOffsets _offsets, NoiseCurveSettings _secondary, NoiseCurveSettings _incline)
        {
            float _angleX = 0;
            float _angleY = 0;

            _angleY = NoiseFunction(1, _offsets._secondaryOffset, _secondary, _offsets._secondaryLayerOffsets);
            _angleX = NoiseFunction(_offsets._inclineOffset, 1, _incline, _offsets._inclineLayerOffsets);

            //assign max turn
            _angleY = Mathf.Lerp(-_secondary._maxAngle, _secondary._maxAngle, _angleY);
            _angleX = Mathf.Lerp(-_incline._maxAngle, _incline._maxAngle, _angleX);

            //Debug.Log("Angle X Axis = " + _angleX + ". Angle Y Axis = " + _angleY);

            //rotate direction vector
            in_direction = Quaternion.Euler(_angleX, _angleY, 0) * in_direction;

            //change offset for the next step
            _offsets._inclineOffset += 1;
            _offsets._secondaryOffset += 1;

            return in_direction.normalized;
        }
        // private float NoiseFunction(float in_x, float in_y, NoiseSettings in_settings, Vector2[] _offsets)
        // {
        //     float freq = in_settings.startFrequency;
        //     float amplitude = 1f;
        //     float noiseValue = 0;
        //     float octave = 0;
        //     for (int i = 0; i <= in_settings.octaves - 1; i++)
        //     {
        //         octave = Mathf.PerlinNoise(in_x * freq + _offsets[i].x, in_y * freq + _offsets[i].y);
        //         octave = (octave * 2) - 1;
        //         octave = octave * amplitude;
        //         //Debug.Log("Layer value " + i + ": " + layerValue);

        //         noiseValue += octave;

        //         amplitude *= in_settings.persistence;
        //         freq *= in_settings.freqChange;
        //     }
        //     noiseValue = Mathf.InverseLerp(-1, 1, noiseValue);
        //     //noiseValue = noiseValue);
        //     return noiseValue;
        // }
    }
}


