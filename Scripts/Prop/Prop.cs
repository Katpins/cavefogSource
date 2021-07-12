using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace caveFog
{

    //how and if we wanna check the overlap of the prop
    public enum OverlapCheck { Props, Cave, CaveAndProps };

    [System.Serializable]
    //contains all iinformation about the spawn of specific Prop
    [CreateAssetMenu(menuName = "Props/ new Prop")]
    public class Prop : ScriptableObject
    {
        [SerializeField]
        private PropObject _prefab = null;

        [SerializeField]
        [Range(0, 100)]
        private float _spawnChance = 100;

        public float SpawnChance { get => _spawnChance; }

        [SerializeField]
        private bool _useNormalOrientation;
        public bool UseNormalOrientation { get => _useNormalOrientation; }

        [SerializeField]
        private bool _useOverlapCheck = false;

        private CheckSphere[] _checkSpheres;

        [SerializeField]
        private Queue<PropObject> _avaliableObjectPool = new Queue<PropObject>();

        private Transform _poolParent;


        // private System.Random _seedGen;

        // public System.Random Seed { get => _seedGen; }


        private void OnEnable()
        {
            if (_useOverlapCheck) _checkSpheres = GetCheckSpheres();
        }
        private CheckSphere[] GetCheckSpheres()
        {
            return _prefab.CheckSpheres;
        }

        //this is where we go through all check spheres and
        public bool CheckOverlap(float3 in_pos)
        {
            if (!_useOverlapCheck) return false;

            Collider[] _buffer = new Collider[1];
            OverlapCheck _checkType = OverlapCheck.Cave;
            int _layerMask = GetLayerMask(_checkType);
            for (int i = 0; i < _checkSpheres.Length; i++)
            {
                _layerMask = _checkSpheres[i].CheckType == _checkType ? _layerMask : GetLayerMask(_checkSpheres[i].CheckType);

                if (Physics.OverlapSphereNonAlloc(_checkSpheres[i].Position + in_pos, _checkSpheres[i].Radius, _buffer, _layerMask, QueryTriggerInteraction.Ignore) > 0)
                {
                    return true;
                }
            }

            return false;

        }

        private int GetLayerMask(OverlapCheck _check)
        {

            switch (_check)
            {
                case OverlapCheck.Cave:
                    return 1 << LayerMask.NameToLayer("CaveMesh");

                case OverlapCheck.Props:
                    return 1 << LayerMask.NameToLayer("Prop");

                case OverlapCheck.CaveAndProps:
                    return 1 << LayerMask.NameToLayer("CaveMesh") | 1 << LayerMask.NameToLayer("Prop");

                //case OverlapCheck.None:
                default:
                    return 0;
            }
        }

        //return object to the pool
        public void AddToPool(PropObject in_object)
        {
            if (_poolParent == null)
                _poolParent = new GameObject(this.name + "Pool").transform;

            in_object.transform.SetParent(_poolParent);
            _avaliableObjectPool.Enqueue(in_object);
            in_object.gameObject.SetActive(false);

        }

        //get prop object which can be reused
        private PropObject CheckIfAvaliablePropObjectExists()
        {
            if (_avaliableObjectPool.Count > 0)
                return _avaliableObjectPool.Dequeue();

            return null;
        }

        public PropObject PlaceProp(PropPlacement _placement, Transform _parentChunk, System.Random in_seed)
        {
            //if (CheckOverlap(_placement.Position)) return null;

            //check if we can get existing object
            PropObject _newPropObject = CheckIfAvaliablePropObjectExists();

            //if not - instantiate object and add to the pool
            if (_newPropObject == null)
            {
                _newPropObject = Instantiate(_prefab);
            }
            else
            {
                //if object is reused - activate it
                _newPropObject.gameObject.SetActive(true);
            }

            //set up placement and parent
            _newPropObject.SetUp(this, _placement, in_seed);
            _newPropObject.transform.SetParent(_parentChunk);



            //_seedGen = in_seed;

            return _newPropObject;
        }
    }
}

