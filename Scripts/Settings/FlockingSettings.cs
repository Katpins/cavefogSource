using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace caveFog
{
    [CreateAssetMenu(menuName = "Flocking Settings")]
    public class FlockingSettings : ScriptableObject
    {
        [SerializeField]
        private float radius = 1;
        public float SpreadRadius { get => radius; }

        [SerializeField]
        private float _placementRadius = 1;
        public float MinPlacementDistance { get => _placementRadius; }

        [SerializeField]
        private float _placementAmount = 10;
        public float MaxPointAmount { get => _placementAmount; }

        [SerializeField]
        private float _bias = 1;
        public float PlacementBias { get => _bias; }

        [SerializeField]
        private float _normalLength = 1;
        public float RaycastOriginDistance { get => _normalLength; }
    }

}
