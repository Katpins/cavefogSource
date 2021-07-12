using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace caveFog
{
    public class CheckOverlapTest : MonoBehaviour
    {

        public PropObject _prop;

         public Prop _propData;

        private int _layerMask;
        private CheckSphere[] _spheres;

        private void Start()
        {

            _spheres = _prop.CheckSpheres;
            _layerMask = GetLayerMask(_spheres[0].CheckType);

            StartCoroutine("Check");
        }
        // Start is called before the first frame update
        IEnumerator Check()
        {
            Collider[] _buffer = new Collider[1];
            string _overlap = "NULL";
            while (this.isActiveAndEnabled)
            {

                if (_propData.CheckOverlap(transform.position))
                {
                    switch (_spheres[0].CheckType)
                    {
                        case OverlapCheck.Cave:
                            _overlap = "CaveMesh";
                            break;

                        case OverlapCheck.Props:
                            _overlap = "Prop";
                            break;

                        case OverlapCheck.CaveAndProps:
                            _overlap = "CaveMesh or Prop";
                            break;

                        //case OverlapCheck.None:
                        default:
                            _overlap = " ????";
                            break;
                    }
                    Debug.Log("Overlaping with " + _overlap);
                }

                yield return new WaitForSeconds(1f);
            }
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
                    return (1 << LayerMask.NameToLayer("CaveMesh") | 1 << LayerMask.NameToLayer("Prop"));

                //case OverlapCheck.None:
                default:
                    return 0;
            }
        }
    }

}
