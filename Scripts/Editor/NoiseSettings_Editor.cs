using UnityEngine;
using UnityEditor;

namespace caveFog
{
    [CustomEditor(typeof(NoiseCurveSettings))]
    public class NoiseSettings_Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            NoiseCurveSettings _settings = (NoiseCurveSettings)target;
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                _settings.settingsChange?.Invoke();
            }
        }
    }

}
