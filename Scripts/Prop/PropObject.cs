using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace caveFog
{
    [System.Serializable]
    public class CheckSphere
    {
        [SerializeField]
        private float3 position;
        public float3 Position { get => position; }

        [SerializeField]
        private float radius;
        public float Radius { get => radius; }

        [SerializeField]
        private OverlapCheck _checkType;
        public OverlapCheck CheckType { get => _checkType; }

        public CheckSphere(float3 in_position, float in_radius, OverlapCheck in_checkType)
        {
            position = in_position;
            radius = in_radius;
            _checkType = in_checkType;
        }
    }


    public class PropObject : MonoBehaviour
    {
        protected Prop _prop;
        public Prop PropData { get => _prop; }

        private bool _avaliable = false;
        public bool Avaliable { get => _avaliable; }


        [SerializeField]
        private CheckSphere[] _checkColliders;

        public CheckSphere[] CheckSpheres { get => _checkColliders; }


        [SerializeField]
        private RandomComponent[] _randomComponents;

        private System.Random _seedGen;

        public System.Random Seed { get => _seedGen; }

        public virtual void SetUp(Prop in_prop, PropPlacement in_placement, System.Random in_seed)
        {
            _prop = in_prop;
            _seedGen = in_seed;

            transform.position = in_placement.Position;
            if (in_prop.UseNormalOrientation) transform.rotation = Quaternion.FromToRotation(transform.up, in_placement.Normal);

            for (int i = 0; i < _randomComponents.Length; i++)
            {
                _randomComponents[i].Apply(_seedGen, in_placement);
            }

            _avaliable = false;
        }

        public void Disable()
        {
            _avaliable = true;
            _prop.AddToPool(this);
            //gameObject.SetActive(false);
        }

        private void OnDrawGizmos()
        {
            if (_checkColliders == null) return;

            for (int i = 0; i < _checkColliders.Length; i++)
            {
                Gizmos.DrawWireSphere(transform.position + (Vector3)_checkColliders[i].Position, _checkColliders[i].Radius);
            }

        }
    }

}
