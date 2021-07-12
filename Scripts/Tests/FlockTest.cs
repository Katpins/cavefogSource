using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace caveFog
{
    [ExecuteInEditMode]
    public class FlockTest : MonoBehaviour
    {
        private List<float3> _points = new List<float3>();

        [SerializeField]
        private Vector3 _normal = Vector3.zero;

        [SerializeField]
        private float radius = 1;

        [SerializeField]
        private float _placementRadius = 1;

        [SerializeField]
        private float _bias = 1;

        private void OnEnable()
        {
            ScatterPoints();
        }

        public void ScatterPoints()
        {
            _points.Clear();

            float angle;
            float3 point;

            //float3 angles;

            //find rotation angles

            //scatter points on a disc
            for (int i = 0; i < 500; i++)
            {
                angle = UnityEngine.Random.Range(0f, math.PI * 2f);
                point = math.float3(math.cos(angle), 0f, math.sin(angle)) * math.pow(UnityEngine.Random.Range(0f, 1), _bias) * radius;

                point = Quaternion.FromToRotation(Vector3.up, _normal) * point;
                point += (float3)transform.position;

                if (_points.Contains(_points.FindLast(x => math.length(point - x) < _placementRadius))) continue;



                _points.Add(point);
            }


        }



        private void OnDrawGizmos()
        {
            Gizmos.DrawLine(transform.position, transform.position + _normal.normalized);

            Gizmos.color = Color.green;
            for (int i = 0; i < _points.Count; i++)
            {
                Gizmos.DrawSphere(_points[i], 0.01f);
                Gizmos.color = Color.white;
                Gizmos.DrawLine(transform.position + _normal.normalized, _points[i]);
            }
        }
    }
}

