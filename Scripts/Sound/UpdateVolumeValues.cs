using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine;


namespace caveFog
{
    public class UpdateVolumeValues : MonoBehaviour
    {
        [SerializeField]
        private PostProcessVolume _volume;
        private ColorGrading _colorGrading;

        [SerializeField]
        private PlayerData _settings;

        private void Start()
        {
            if (!_volume.profile.TryGetSettings<ColorGrading>(out _colorGrading))
            {
                if (Application.isEditor) Debug.LogWarning("Oh no! We can't fiind colorgrading settings");
            }
        }
        private void Update()
        {
            if (_colorGrading.gamma.value != Vector4.one * _settings.Gamma)
            {
                _colorGrading.gamma.value = Vector4.one * _settings.Gamma;
            }
        }
    }

}
