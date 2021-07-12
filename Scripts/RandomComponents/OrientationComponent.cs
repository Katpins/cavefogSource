using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace caveFog
{
    public class OrientationComponent : RandomComponent
    {
        private enum OrientationAxis { Forward, Right, Up, Normal }

        [SerializeField]
        private OrientationAxis _forward = OrientationAxis.Forward;

        [SerializeField]
        private bool _flattenForward = true;

        [SerializeField]
        private OrientationAxis _upward = OrientationAxis.Normal;

        public override void Apply(System.Random _seed, PropPlacement in_placement)
        {
            Vector3 _forwardVec;
            Vector3 _upwardVec;

            switch (_forward)
            {
                case OrientationAxis.Normal:
                    _forwardVec = in_placement.Normal;
                    break;
                case OrientationAxis.Forward:
                    _forwardVec = Vector3.forward;
                    break;
                case OrientationAxis.Right:
                    _forwardVec = Vector3.right;
                    break;
                case OrientationAxis.Up:
                    _forwardVec = Vector3.up;
                    break;
                default:
                    _forwardVec = Vector3.forward;
                    break;
            }

            if (_flattenForward) _forwardVec.y *= 0;

            switch (_upward)
            {
                case OrientationAxis.Normal:
                    _upwardVec = in_placement.Normal;
                    break;
                case OrientationAxis.Forward:
                    _upwardVec = Vector3.forward;
                    break;
                case OrientationAxis.Right:
                    _upwardVec = Vector3.right;
                    break;
                case OrientationAxis.Up:
                    _upwardVec = Vector3.up;
                    break;
                default:
                    _upwardVec = in_placement.Normal;
                    break;
            }

            transform.rotation = Quaternion.LookRotation(_forwardVec, _upwardVec);
        }
    }

}
