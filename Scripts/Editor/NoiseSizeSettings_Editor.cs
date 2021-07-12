using UnityEngine;
using UnityEditor;

namespace caveFog
{
    [CustomEditor(typeof(NoiseMinMaxSettings))]
    public class NoiseSizeSettings_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            NoiseMinMaxSettings _settings = (NoiseMinMaxSettings)target;
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                _settings.settingsChange?.Invoke();
            }
        }
    }

}

