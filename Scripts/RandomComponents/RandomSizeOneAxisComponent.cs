using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace caveFog
{
    public class RandomSizeOneAxisComponent : RandomComponent
    {
        public Vector3 _maxSize;
        public Vector3 _minSize;
        public override void Apply(System.Random _seed, PropPlacement in_placement)
        {
            transform.localScale = new Vector3( (_seed.Next((int)(_minSize.x * 100), (int)(_maxSize.x * 100)) / 100f),
                                                (_seed.Next((int)(_minSize.y * 100), (int)(_maxSize.y * 100)) / 100f),
                                                (_seed.Next((int)(_minSize.x * 100), (int)(_maxSize.x * 100)) / 100f));
        }
    }

}
