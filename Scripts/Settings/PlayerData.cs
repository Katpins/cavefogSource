using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Mathematics;

namespace caveFog
{
    [CreateAssetMenu(menuName = "Player Data")]
    public class PlayerData : ScriptableObject
    {

        private Transform _playerTransformReference;

        public Transform TransformReference { set => _playerTransformReference = value; get => _playerTransformReference; }

        private Camera _playerCamera;
        public Camera CameraReference { set => _playerCamera = value; get => _playerCamera; }

        [SerializeField]
        private float _sensitivity = 300f;
        public float Sensitivity { get => _sensitivity; }


        public Action<float3, float3> SetUpPlayer;
        public Action<float3, float3> MovePlayer;
        public Action FinishSetUpPlayer;

        private int _caveSeed = 0;

        public int CaveSeed { get => _caveSeed; }

        [SerializeField]
        private float jumpPower = 0;

        public float JumpPower { get => jumpPower; }

        private bool _isWalking;
        public bool IsWalking { get => _isWalking; set => _isWalking = value; }

        private float lookSensitivity = 0;

        public float LookSensitivity { get => lookSensitivity; }

        [SerializeField]
        private float gamma = 0;

        public float Gamma { get => gamma; }


        public void MakeRandomSeed()
        {
            //start out with random seed
            _caveSeed = UnityEngine.Random.Range(-1000, 1000);
        }

        public void UpdateCaveSeed(string _seed)
        {
            if (!int.TryParse(_seed, out _caveSeed))
            {
                _caveSeed = 0;
            }
        }

        public void UpdatejumpPower(string _power)
        {
            if (!float.TryParse(_power, out jumpPower))
            {
                jumpPower = 0;
            }


            if (jumpPower > 5) jumpPower = 5;
        }

        public void UpdateMouseSensitivity(string in_sensitivity)
        {
            if (!float.TryParse(in_sensitivity, out _sensitivity))
            {
                _sensitivity = 300f;
            }
            if (_sensitivity < 100f) _sensitivity = 100f;
        }

        public void UpdateGamma(float in_sensitivity)
        {
            gamma = in_sensitivity;
        }

    }

}
