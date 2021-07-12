using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace caveFog
{
    public class ToggleDisableParticles : RandomComponent
    {
        private enum DisableMethod { Random, DistanceAndView }

        [SerializeField]
        private PlayerData _playerRef = null;

        [SerializeField]
        private float _distanceToLook = 0.7f;

        [SerializeField]
        private DisableMethod _method = DisableMethod.Random;

        [SerializeField]
        private ParticleSystem[] _particlesToToggle;

        private ParticleSystem.EmissionModule _moduleRef;
        private Vector3 _screenPos;

        private bool _active = true;
        public override void Apply(System.Random _seed, PropPlacement in_placement)
        {
            if (_playerRef == null)
            {
                Debug.LogError("Missing Player Reference!");
                return;
            }
            if (_particlesToToggle.Length <= 0) return;
            if (_method == DisableMethod.Random) _active = _seed.Next(2) == 1;
            UpdateParticles(_active);

        }

        private void Update()
        {
            if (_method == DisableMethod.Random) return;

            _screenPos = _playerRef.CameraReference.WorldToViewportPoint(transform.position);

            _active = (transform.position - _playerRef.TransformReference.position).sqrMagnitude > math.pow(_distanceToLook, 2f);


            if (_screenPos.z < 0 || (_screenPos.x < 0 && _screenPos.x > 1) || (_screenPos.y < 0 && _screenPos.y > 1) || !_active)
            {
                UpdateParticles(_active);
            }


        }

        private void UpdateParticles(bool in_active)
        {

            for (int i = 0; i < _particlesToToggle.Length; i++)
            {
                _moduleRef = _particlesToToggle[i].emission;
                //if (in_active == _particlesToToggle[i].isPlaying) continue;
                _moduleRef.enabled = _active;

            }

        }
    }

}
