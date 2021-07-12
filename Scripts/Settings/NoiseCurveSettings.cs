using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace caveFog
{
    [CreateAssetMenu(menuName = "Noise/Noise Curve Settings")]
    public class NoiseCurveSettings : NoiseSettings
    {
        [Range(0f, 360f)]
        public float _maxAngle;
    }
}
