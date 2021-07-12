using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace caveFog
{
    //Settings for creating a Path
    [CreateAssetMenu(menuName = "Chunk/Main Path Settings")]
    public class MainPathSettings : PathSettings
    {
        public NoiseCurveSettings _secondaryCurve = null;
        public NoiseCurveSettings _basePathCurve = null;
        public NoiseCurveSettings _inclineCurve = null;

        [Range(0.1f, 3f)]
        public float _stepLength;

        public float _yStepRestriction = 0.1f;

        [Header("Base Curvines")]
        public bool _useThreshold = false;

        [Range(1, 1000)]
        public int _iterationThreshold = 200;

        public AnimationCurve _noiseCurveToDistance = new AnimationCurve();

        public AnimationCurve _secondaryNoiseToDistance = new AnimationCurve();

        [Header("Restrict worm")]

        public bool _disableCenterPull;
        public AnimationCurve _centerPullToDistance = new AnimationCurve();

        [Range(0f, 2f)]
        public float _centerPullAmount = 1f;

        [Range(0f, 10f)]
        public float _pushFromBoundsThreshold = 1;

        public float _pushFromBoundsStrength = 1;

        public AnimationCurve _pushDampCurve = new AnimationCurve();

        public AnimationCurve _pushApplicationCurve = new AnimationCurve();

        // private System.Random seedGen;

        [Range(-1f, 1f)]
        public float _noiseStrength = 0;

        [Range(-1f, 1f)]
        public float _inclineStrength = 0;

        // public void RandomizeSeeds()
        // {

        //     _secondaryCurve.ChangeSeed(Random.Range(-1000, 1000));
        //     _basePathCurve.ChangeSeed(Random.Range(-1000, 1000));
        //     _inclineCurve.ChangeSeed(Random.Range(-1000, 1000));
        // }

        // public void RandomizeSeeds(int _seed)
        // {

        //     seedGen = new System.Random(_seed);

        //     _secondaryCurve.ChangeSeed(seedGen.Next(-1000, 1000));
        //     _basePathCurve.ChangeSeed(seedGen.Next(-1000, 1000));
        //     _inclineCurve.ChangeSeed(seedGen.Next(-1000, 1000));
        // }

    }
}
