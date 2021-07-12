using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace caveFog
{
    public class DefaultInputValue : MonoBehaviour
    {
        public enum ValueType { Sensitivity, Jump, Gamma }

        [SerializeField]
        private PlayerData _playerData;

        [SerializeField]
        private InputField _field;

        [SerializeField]
        private bool _isSlider = false;

        [SerializeField]
        private Slider _slider;

        [SerializeField]
        private ValueType _valueType;
        private void OnEnable()
        {
            switch (_valueType)
            {
                case ValueType.Jump:
                    if (_isSlider)
                    {
                        _slider.value = _playerData.JumpPower;
                        break;
                    }
                    _field.text = "" + _playerData.JumpPower;
                    break;
                case ValueType.Sensitivity:
                    if (_isSlider)
                    {
                        _slider.value = _playerData.Sensitivity;
                        break;
                    }
                    _field.text = "" + _playerData.Sensitivity;
                    break;
                case ValueType.Gamma:
                    if (_isSlider)
                    {
                        _slider.value = _playerData.Gamma;
                        break;
                    }
                    _field.text = "" + _playerData.Gamma;
                    break;

                default:

                    _field.text = "0";
                    break;

            }
        }

    }

}
