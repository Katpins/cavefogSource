using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace caveFog
{

    //contains settings of a multi octave perlin

    public abstract class NoiseSettings : ScriptableObject
    {
        // [Range(0.001f, 360f)]
        // public float _maxAngle = 90f;

        // [HideInInspector]
        // public int _baseOffset = 1;

        [Range(0, 4)]
        public int octaves = 3;

        [Range(0f, 2f)]
        public float startFrequency = 0.5f;

        public float freqChange = 0.5f;
        public float persistence = 0.5f;

        public Action settingsChange;

        // [HideInInspector]
        // public int seed = 0;
        // public Vector2[] layerOffsets = new Vector2[4];

        //DEBUG update
        //public Action valuesChanged;

        // private System.Random randomGen;

        // private void OnEnable()
        // {
        //     ChangeSeed();
        // }
        // public void ChangeSeed()
        // {
        //     layerOffsets = new Vector2[4];
        //     randomGen = new System.Random(seed);
        //     //noiseOffset = new Vector2(randomGen.Next(-1000, 1000), randomGen.Next(-1000, 1000));
        //     for (int i = 0; i < 4; i++)
        //     {
        //         layerOffsets[i] = new Vector2(randomGen.Next(-1000, 1000), randomGen.Next(-1000, 1000));
        //     }
        // }
        // public void ChangeSeed(int in_seed)
        // {
        //     seed = in_seed;
        //     layerOffsets = new Vector2[4];
        //     randomGen = new System.Random(seed);
        //     //noiseOffset = new Vector2(randomGen.Next(-1000, 1000), randomGen.Next(-1000, 1000));
        //     for (int i = 0; i < 4; i++)
        //     {
        //         layerOffsets[i] = new Vector2(randomGen.Next(-1000, 1000), randomGen.Next(-1000, 1000));
        //     }
        // }
        //     public void ResetOffset()
        //     {
        //         _baseOffset = 1;
        //     }
    }

}

