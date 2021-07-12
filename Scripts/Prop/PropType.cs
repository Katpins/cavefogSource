using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace caveFog
{

    [System.Serializable]
    //prop type decides upon distance of props from each other and alignment
    [CreateAssetMenu(menuName = "Props/new Prop Type")]
    public class PropType : ScriptableObject
    {

        [Header("Placement Settings")]
        [SerializeField]
        private bool _roomProp = false;
        public bool RoomProp { get => _roomProp; }

        [SerializeField]
        private bool _mainPathProp = false;
        public bool MainPathProp { get => _mainPathProp; }

        [SerializeField]
        private bool _pitProp = false;
        public bool PitsProp { get => _pitProp; }

        [SerializeField]
        private bool _ceilingProp = false;
        public bool CeilingProp { get => _ceilingProp; }


        [SerializeField]
        [Range(-1, 1)]
        private float _minNormalY = 0;
        public float MinNormalY { get => _minNormalY; }

        [SerializeField]
        [Range(-1, 1)]
        private float _maxNormalY = 1;
        public float MaxNormalY { get => _maxNormalY; }

        [SerializeField]
        private float _placementRadius;

        public float PlacementRadius { get => _placementRadius; }

        [SerializeField]
        [Range(0, 100)]
        private int _spawnChance = 100;
        public int SpawnChance { get => _spawnChance; }

        [SerializeField]
        [Range(1, 10)]
        private int _step = 2;
        public int SpawnStep { get => _step; }

        [SerializeField]
        [Range(5, 20)]
        private int _rays = 10;
        public int RaysAmount { get => _rays; }

        [Header("Y Placement")]
        [SerializeField]
        [Range(-1, 1)]
        private float _maxYValue = 1f;
        public float MaxY { get => _maxYValue; }

        [SerializeField]
        [Range(-1, 1)]
        private float _minYValue = -1f;
        public float MinY { get => _minYValue; }


        [Header("Dead Zone")]
        [SerializeField]
        private bool _enableDeadZone = true;
        public bool DeadZone { get => _enableDeadZone; }


        [SerializeField]
        [Range(0, 2f)]
        private float _deadSizeZ = 0.5f;
        public float DeadZoneSizeZ { get => _deadSizeZ; }

        [SerializeField]
        [Range(0, 2f)]
        private float _deadSizeX = 0.5f;
        public float DeadZoneSizeX { get => _deadSizeX; }

        [Header("Flocking")]
        [SerializeField]
        private bool _enableFlocking = false;
        public bool Flocking { get => _enableFlocking; }

        [SerializeField]
        private FlockingSettings _flockingSettings;
        public FlockingSettings FlockingSettings { get => _flockingSettings; }


        [Header("Prop Objects")]
        [SerializeField]
        private Prop[] _propsOfThisType;

        [SerializeField]
        private float[] _spawnChancesModified;

        public float[] SpawnChanceMod { get => _spawnChancesModified; }

        public int PropAmount { get => _propsOfThisType.Length; }

        private PropPlacement[] _possiblePositions;

        public PropPlacement[] PossiblePositions { get => _possiblePositions; set => _possiblePositions = value; }

        private void OnEnable()
        {
            GetSpawnChances();
        }

        private void GetSpawnChances()
        {
            //if no props then dont count spawn chances
            if (_propsOfThisType == null) return;
            _spawnChancesModified = new float[_propsOfThisType.Length];

            float _sum = 0;
            float _mod;

            //modify chances so that they dont exceed 100
            for (int i = 0; i < _propsOfThisType.Length; i++)
            {
                _sum += _propsOfThisType[i].SpawnChance;
            }

            _mod = 100 / _sum;

            for (int i = 0; i < _propsOfThisType.Length; i++)
            {
                _spawnChancesModified[i] = _propsOfThisType[i].SpawnChance * _mod;
            }
        }

        ///@ aldonaletto
        public int GetRandomPropID(System.Random in_seed)
        {
            int _rand = in_seed.Next(0, 101);

            float _lowLim;
            float _highLim = 0;

            for (int i = 0; i < _spawnChancesModified.Length; i++)
            {
                _lowLim = _highLim;
                _highLim += _spawnChancesModified[i];

                //if random int is in chance limit return according id
                if (_rand >= _lowLim && _rand <= _highLim)
                {
                    return i;
                }

            }

            return _spawnChancesModified.Length - 1;
        }

        public Prop this[int index]
        {
            // if (_propsOfThisType == null) return null;
            // if (index > _propsOfThisType.Length - 1 || index< 0) return null;
            get => _propsOfThisType[index];
        }


        public void PlaceProps(Transform _parent, System.Random in_seed, IList<PropObject> _objectList)
        {
            for (int i = 0; i < _possiblePositions.Length; i++)
            {
                //here we go through spawning
                _objectList.Add(_propsOfThisType[_possiblePositions[i].PropID].PlaceProp(_possiblePositions[i], _parent, in_seed));
            }
        }
    }

}
