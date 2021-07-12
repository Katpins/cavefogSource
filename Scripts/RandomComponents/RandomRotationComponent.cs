using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace caveFog
{
    public class RandomRotationComponent : RandomComponent
    {

        public bool _rotationAxisY = true;
        public bool _rotationAxisX = false;
        public bool _rotationAxisZ = false;

          public override void Apply(System.Random _seed,PropPlacement in_placement)
        {
            if (_rotationAxisY) transform.Rotate(transform.up, _seed.Next(0, 360), Space.World) ;
            if (_rotationAxisX) transform.Rotate(transform.right, _seed.Next(0, 360), Space.World);
            if (_rotationAxisZ) transform.Rotate(transform.forward, _seed.Next(0, 360), Space.World);
        }

        
    }
}
