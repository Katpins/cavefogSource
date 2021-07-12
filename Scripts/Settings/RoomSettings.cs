using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace caveFog
{
    [CreateAssetMenu(menuName = "Room Settings")]
    public class RoomSettings : ScriptableObject
    {
        [Header("Room Detection")]
        public int _minNeighboursForRoomTile = 27;

        [Range(1, 3)]
        public int _neighboursLevel = 1;

        [Range(0f, 1f)]
        public float _valueForRoom;

        public ValueOpperation _conditionForRoom;

        [Header("Ceilings")]
        //public float _ceilingsHeight;


        public float _maxRandomOffsetCeil = 0.4f;
        public int _ceilingsHeightMin;
        public int _ceilingsHeightMax;
        public int _ceilingWormAmountMin;
        public int _ceilingWormAmountMax;

        [Header("Pits")]

        public float _maxRandomOffsetPits = 0.4f;

        public int _pitsDepthMin;
        public int _pitsDepthMax;

        public int _pitsWormAmountMin;
        public int _pitsWormAmountMax;

        public SimplePathGenerator _ceilingPathGen;

    }

}

