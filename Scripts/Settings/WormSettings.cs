using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

namespace caveFog
{
    [CreateAssetMenu(menuName = "Chunk/ Worm Settings")]
    public class WormSettings : ScriptableObject
    {
        [SerializeField]
        private float _baseRadius = 1f;
        public float BaseRadius { get => _baseRadius; }


        [SerializeField]
        private float _baseDensity = 1f;
        public float BaseDensity { get => _baseDensity; }

        [SerializeField]
        private AnimationCurve _densityOverSlice = new AnimationCurve();
        public AnimationCurve DensityOverSlice { get => _densityOverSlice; }

        public int _densityPrecision = 200;

        //[SerializeField]
        private float[] _densityArray;
        public float[] DensityArray { get => _densityArray; }

        [SerializeField]
        public NoiseMinMaxSettings _sizeNoise;

        [SerializeField]
        private AnimationCurve _noiseOverPath = new AnimationCurve();
        public AnimationCurve NoiseOverPath { get => _noiseOverPath; }

        [SerializeField]
        private float _minSize;
        public float MinSize { get => _minSize; }

        [SerializeField]
        private float _maxSize;
        public float MaxSize { get => _maxSize; }

        [SerializeField]
        private AnimationCurve _sizeOverPathBase = new AnimationCurve();
        public AnimationCurve SizeOverPathBase { get => _sizeOverPathBase; }


        private void OnEnable()
        {
            _densityArray = new float[_densityPrecision + 1];

            float _step = 1f / (float)_densityPrecision;

            for (int i = 0; i < _densityPrecision + 1; i++)
            {
                _densityArray[i] = _densityOverSlice.Evaluate(_step * i);
            }
        }
        // public float[] GetDensity()
        // {
        //     //if (_densityArray != null) return _densityArray;
        //     return _densityArray;
        // }
    }

}

