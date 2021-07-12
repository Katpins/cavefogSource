using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Cinemachine;

namespace caveFog
{
    public class PlayerCamera : MonoBehaviour
    {
        [SerializeField]
        private PlayerData _player;



        private void Awake()
        {
            // _cam.enabled = false;
            _player.FinishSetUpPlayer += OnPlayerInit;
        }

        private void OnPlayerInit()
        {
            // _cam.enabled = true;
            _player.CameraReference = GetComponent<Camera>();
            _player.FinishSetUpPlayer -= OnPlayerInit;


        }
    }

}
