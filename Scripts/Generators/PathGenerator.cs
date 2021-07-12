using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace caveFog
{
    public class PathGenerator : ScriptableObject
    {


        //base force from start to end
        protected Vector3 DirectForce(Vector3 in_start, Vector3 in_end)
        {
            //direction from stert to end
            Vector3 _directForce = (in_end - in_start);

            return _directForce.normalized;
        }

        //base force to add curviness to base path
        protected Vector3 BaseCurveForce(Vector3 baseForce, MainPathOffsets _offsets, NoiseCurveSettings _noise, bool _verticalCurve = false)
        {
            float _angle = 0;

            _angle = NoiseFunction(1, _offsets._baseOffset, _noise, _offsets._baseLayerOffsets);

            //assign max turn
            _angle = Mathf.LerpUnclamped(-_noise._maxAngle, _noise._maxAngle, _angle);

            //rotate direction vector
            if (!_verticalCurve) baseForce = Quaternion.Euler(0, _angle, 0) * baseForce;
            else baseForce = Quaternion.Euler(_angle, 0, 0) * baseForce;

            //change offset for the next step
            _offsets._baseOffset += 1;

            return baseForce.normalized;

        }

        protected float BaseSizeNoise(float _min, float _max, MainPathOffsets _offsets, NoiseMinMaxSettings _noise)
        {
            float _size = 0;

            _size = NoiseFunction(1, _offsets._baseOffset, _noise, _offsets._baseLayerOffsets);

            _size = Mathf.LerpUnclamped(_noise.minValue, _noise.maxValue, _size);

            //change offset for the next step
            _offsets._baseOffset += 1;

            return _size;
        }

        protected float NoiseFunction(float in_x, float in_y, NoiseSettings in_settings, Vector2[] _offsets)
        {
            float freq = in_settings.startFrequency;
            float amplitude = 1f;
            float noiseValue = 0;
            float octave = 0;
            for (int i = 0; i <= in_settings.octaves - 1; i++)
            {
                octave = Mathf.PerlinNoise(in_x * freq + _offsets[i].x, in_y * freq + _offsets[i].y);
                octave = (octave * 2) - 1;
                octave = octave * amplitude;
                //Debug.Log("Layer value " + i + ": " + layerValue);

                noiseValue += octave;

                amplitude *= in_settings.persistence;
                freq *= in_settings.freqChange;
            }
            noiseValue = Mathf.InverseLerp(-1, 1, noiseValue);
            //noiseValue = noiseValue);
            return noiseValue;
        }
    }

}

