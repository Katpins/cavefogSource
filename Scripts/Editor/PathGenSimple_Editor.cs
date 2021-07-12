using UnityEngine;
using UnityEditor;

namespace caveFog
{
    [CustomEditor(typeof(SimplePathSettings))]
    public class PathGenSimple_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            SimplePathSettings _settings = (SimplePathSettings)target;
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                _settings.settingsChange?.Invoke();
            }
        }
    }

}

