using UnityEngine;
using UnityEditor;

namespace caveFog
{
    [CustomEditor(typeof(WormPathTester))]
    public class WormTester_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            WormPathTester _tester = (WormPathTester)target;
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                _tester.UpdateGizmos();
            }

            if (GUILayout.Button("Generate Path Mesh"))
            {
                _tester.GeneratePathMesh();
            }

            if (GUILayout.Button("Generate Walkable Path Mesh"))
            {
                _tester.GenerateWalkablePathMesh();
            }

            if (GUILayout.Button("Generate Full Path Mesh"))
            {
                _tester.GenerateWalkablePathWithMesh();
            }
        }
    }

}
