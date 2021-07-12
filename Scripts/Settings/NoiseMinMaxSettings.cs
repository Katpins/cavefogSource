using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace caveFog
{
    [CreateAssetMenu(menuName = "Noise/Noise Settings")]
    public class NoiseMinMaxSettings : NoiseSettings
    {
        public float minValue;
        public float maxValue;
    }

}
