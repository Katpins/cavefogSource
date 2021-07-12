using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Cinemachine;

namespace caveFog
{
    public class SimpleController : MonoBehaviour
    {
        [SerializeField]
        private PlayerData _data = null;
        public float _speed = 5f;

        private bool _crouching = false;
        public Camera _cam;

        private Vector3 _camDir;
        private Vector3 _camDirX;
        //public Transform _cameraObject = null;
        public float _sensitivity = 5f;
        public float _accel = 0.1f;
        public Rigidbody _rigid = null;

        // private float _rotationVel = 0;
        //private float _currentRotationVel = 0;
        private Vector3 _force;

        public CinemachineVirtualCamera _camBrain;

        private CinemachinePOV _povCam;

        private void Awake()
        {
            _data.SetUpPlayer += InitPlayer;
            _data.MovePlayer += MovePlayer;
            _rigid.isKinematic = true;

            _povCam = _camBrain.GetCinemachineComponent<CinemachinePOV>();
        }



        private void OnDisable()
        {
            _data.SetUpPlayer -= InitPlayer;
            _data.MovePlayer -= MovePlayer;
        }
        private void Start()
        {
            _crouching = false;


        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl)) Crouch(true);
            if (Input.GetKeyUp(KeyCode.LeftControl)) Crouch(false);

            _camDir = _cam.transform.forward;
            _camDir.y = 0;

            _camDirX = _cam.transform.right;

            _force = Vector3.zero;
            if (Input.GetKey(KeyCode.W)) _force += _camDir.normalized;
            if (Input.GetKey(KeyCode.S)) _force -= _camDir.normalized;
            if (Input.GetKey(KeyCode.A)) _force -= _camDirX;
            if (Input.GetKey(KeyCode.D)) _force += _camDirX;

            _rigid.velocity = new Vector3(_force.normalized.x * _speed, _rigid.velocity.y, _force.normalized.z * _speed);
            //if(_rigid.velocity.magnitude > _speed)
            if (Input.GetKeyDown(KeyCode.Space)) _rigid.velocity += Vector3.up.normalized * _data.JumpPower;


        }
        private void FixedUpdate()
        {
            WalkCheck();
        }
        private void LateUpdate()
        {
            if (_povCam.m_VerticalAxis.m_MaxSpeed != _data.Sensitivity)
            {
                _povCam.m_VerticalAxis.m_MaxSpeed = _data.Sensitivity;
                _povCam.m_HorizontalAxis.m_MaxSpeed = _data.Sensitivity;
            }
        }

        private void WalkCheck()
        {
            //check if we move
            if ((_rigid.velocity.sqrMagnitude < 0.3f) || (Mathf.Abs(_rigid.velocity.normalized.y) > 0.95f))
            {
                _data.IsWalking = false;
                return;
            }


            //check if there is ground under us
            _data.IsWalking = Physics.Raycast(transform.position, Vector3.down, 0.4f, 1 << LayerMask.NameToLayer("CaveMesh") | 1 << LayerMask.NameToLayer("Prop"));

        }

        private void Crouch(bool is_crouching)
        {
            if (is_crouching == _crouching) return;

            if (is_crouching) transform.localScale = Vector3.one * 0.6f;

            if (!is_crouching)
            {
                transform.localScale = Vector3.one;
                //transform.position += Vector3.up * 0.1f;
            }

            _crouching = is_crouching;
        }

        private void InitPlayer(float3 _pos, float3 _forward)
        {
            MovePlayer(_pos, _forward);
            _rigid.isKinematic = false;

            _data.TransformReference = this.transform;

            _data.FinishSetUpPlayer?.Invoke();
        }
        public void MovePlayer(float3 _pos, float3 _forward)
        {
            transform.position = _pos + math.float3(0, 1, 0) * 0.1f;
            transform.rotation = Quaternion.AngleAxis(
                                 Quaternion.Angle(transform.rotation,
                                                Quaternion.LookRotation(_forward, transform.up)),
                                 transform.up);
        }
    }

}
