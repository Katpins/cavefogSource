using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace caveFog
{
    [System.Serializable]
    [CreateAssetMenu(menuName = "Props/new PropCollection")]
    public class PropCollection : ScriptableObject
    {
        [SerializeField]
        private PropType[] _typesOfProps;

        public int Length { get => _typesOfProps.Length; }

        public PropType this[int index]
        {
            get => _typesOfProps[index];
        }
    }

}
