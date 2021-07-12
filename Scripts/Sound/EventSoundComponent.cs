using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace caveFog
{
    public class EventSoundComponent : MonoBehaviour
    {

        [SerializeField]
        private PlayerData _data;

        [SerializeField]
        private AudioSource _soundSource;

        private bool _lastBool = false;

        private void Update()
        {
            if (_lastBool != _data.IsWalking)
            {
                _lastBool = _data.IsWalking;
                if (_data.IsWalking)
                {
                    _soundSource.Play();
                    return;
                }

                _soundSource.Stop();
            }
        }
    }
}


