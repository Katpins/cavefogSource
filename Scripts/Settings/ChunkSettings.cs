using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace caveFog
{
    [CreateAssetMenu(menuName = "Chunk/Chunk Settings")]
    public class ChunkSettings : ScriptableObject
    {

        [Header("Voxel Size")]

        [Range(2, 100)]
        public int _heightVoxels;
        [Range(2, 100)]
        public int _lengthVoxels;
        [Range(2, 100)]
        public int _widthVoxels;

        [Header("World Unit Size")]
        public int _heightWU;
        public int _lengthWU;
        public int _widthWU;

        [Header("Generation")]
        [Range(0f, 1f)]
        public float _surface = 1f;

        public float _timeToDie = 5f;

        public float _timeToDieIfNotVisited = 5f;

        [HideInInspector]
        public float _widthOffset;

        [HideInInspector]
        public float _lengthOffset;

        [HideInInspector]
        public float _heightOffset;

        [HideInInspector]
        public Vector3 _maxValues;

        private void OnEnable()
        {
            CalculateOffsets();
        }

        public void CalculateOffsets()
        {
            _widthOffset = (float)_widthWU / ((float)_widthVoxels - 1);
            _heightOffset = (float)_heightWU / ((float)_heightVoxels - 1);
            _lengthOffset = (float)_lengthWU / ((float)_lengthVoxels - 1);

            _maxValues = new Vector3(_widthWU / 2f - 1f, _heightWU / 2f - 1f, _lengthWU / 2f - 1f);
        }

    }
}
