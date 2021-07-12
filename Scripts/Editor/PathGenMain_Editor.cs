using UnityEngine;
using UnityEditor;

namespace caveFog
{
    [CustomEditor(typeof(MainPathSettings))]
    public class PathGenMain_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            MainPathSettings _settings = (MainPathSettings)target;
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                _settings.settingsChange?.Invoke();
            }
        }
    }

}
