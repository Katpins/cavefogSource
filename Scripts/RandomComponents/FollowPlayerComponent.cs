using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace caveFog
{
    //make object turn towards player always or only when we dont look
    public class FollowPlayerComponent : RandomComponent
    {
        private enum LookingMethod { Always, OutOfCam, Distance }

        [SerializeField]
        private PlayerData _playerRef = null;


        [SerializeField]
        private float _moveSpeed = 1;

        [SerializeField]
        private float _intervalCheck = 2;

        [SerializeField]
        private bool _instant = true;

        [SerializeField]
        private float _distanceToLook = 0.7f;

        [SerializeField]
        private LookingMethod _method = LookingMethod.OutOfCam;

        // [SerializeField]
        // private bool _flattenForward = true;

        private bool _look = false;

        private bool _activated = false;
        private bool _moving = false;

        private Vector3 _screenPos;

        private Vector3 _lookDir;
        private Vector3 _lookDirTemp;

        private Coroutine _moveCoroutine;
        private Coroutine _checkCoroutine;

        public override void Apply(System.Random _seed, PropPlacement in_placement)
        {
            _lookDirTemp = transform.forward;
            _activated = true;
            _moving = false;
            _checkCoroutine = StartCoroutine(CO_Check());
        }

        private void OnDisable()
        {
            _activated = false;
            StopAllCoroutines();
        }
        private void LateUpdate()
        {

            // if (!_activated) return;

            // switch (_method)
            // {
            //     case LookingMethod.OutOfCam:
            //         _screenPos = _playerRef.CameraReference.WorldToViewportPoint(transform.position);
            //         _look = (_screenPos.z < 0 || (_screenPos.x < 0 && _screenPos.x > 1) || (_screenPos.y < 0 && _screenPos.y > 1));
            //         break;
            //     case LookingMethod.Distance:
            //         _look = (transform.position - _playerRef.TransformReference.position).sqrMagnitude > math.pow(_distanceToLook, 2f);
            //         break;
            //     case LookingMethod.Always:
            //         _look = true;
            //         break;

            //     default:
            //         break;
            // }

            // if (_look)
            // {
            //     _lookDirTemp = transform.forward;
            //     _lookDir = (_playerRef.TransformReference.position - transform.position).normalized;
            //     _lookDir = transform.InverseTransformDirection(_lookDir);
            //     _lookDir.y = 0;
            //     _lookDir = transform.TransformDirection(_lookDir);

            //     transform.rotation = Quaternion.LookRotation(_lookDir, Vector3.up);
            // }


        }
        private IEnumerator CO_Check()
        {
            while (_activated)
            {
                yield return new WaitForSeconds(_intervalCheck);

                switch (_method)
                {
                    case LookingMethod.OutOfCam:
                        _screenPos = _playerRef.CameraReference.WorldToViewportPoint(transform.position);
                        _look = (_screenPos.z < 0 || (_screenPos.x < 0 && _screenPos.x > 1) || (_screenPos.y < 0 && _screenPos.y > 1));
                        break;
                    case LookingMethod.Distance:
                        _look = (transform.position - _playerRef.TransformReference.position).sqrMagnitude > math.pow(_distanceToLook, 2f);
                        break;
                    case LookingMethod.Always:
                        _look = true;
                        break;

                    default:
                        break;
                }

                if (_look)
                {

                    _lookDir = (_playerRef.TransformReference.position - transform.position).normalized;
                    _lookDir = transform.InverseTransformDirection(_lookDir);
                    _lookDir.y = 0;
                    _lookDir = transform.TransformDirection(_lookDir);

                    if (_instant)
                    {
                        transform.rotation = Quaternion.LookRotation(_lookDir, Vector3.up);
                        continue;
                    }

                    if (!_moving) _moveCoroutine = StartCoroutine(CO_Move());

                }
            }



        }

        private IEnumerator CO_Move()
        {
            _moving = true;
            while (_lookDirTemp != _lookDir)
            {
                _lookDirTemp = Vector3.MoveTowards(_lookDirTemp, _lookDir, _moveSpeed * Time.deltaTime).normalized;
                transform.rotation = Quaternion.LookRotation(_lookDirTemp, Vector3.up);
                yield return null;
            }
            _moving = false;
        }


    }

}
