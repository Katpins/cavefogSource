using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace caveFog
{
    public class RandomSizeComponent : RandomComponent
    {
        public float _minSize = 1;
        public float _maxSize = 2;

        

        public override void Apply(System.Random _seed, PropPlacement in_placement)
        {
            transform.localScale = Vector3.one * _seed.Next((int)(_minSize * 100), (int)(_maxSize * 100)) / 100f;
        }


    }

}
