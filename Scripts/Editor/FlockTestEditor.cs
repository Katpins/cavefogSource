
using UnityEngine;
using UnityEditor;
namespace caveFog
{
    [CustomEditor(typeof(FlockTest))]
    public class FlockTestEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            FlockTest _test = (FlockTest)target;
            
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            
            if (EditorGUI.EndChangeCheck())
            {
                _test.ScatterPoints();
            }


        }
    }


}
