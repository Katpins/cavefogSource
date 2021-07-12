using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace caveFog
{
    //Settings for creating a Path
    [CreateAssetMenu(menuName = "Chunk/Simple Path Settings")]
    public class SimplePathSettings : PathSettings
    {
        [Range(0.01f, 2f)]
        public float _stepLength = 0.3f;
        public NoiseCurveSettings _yRotationNoise;

        [Range(0f, 1f)]
        public float _yRotationStrength;
        public NoiseCurveSettings _xRotationNoise;

        [Range(0f, 1f)]
        public float _xRotationStrength;
        public AnimationCurve _curveStrength = new AnimationCurve();

    }

}
