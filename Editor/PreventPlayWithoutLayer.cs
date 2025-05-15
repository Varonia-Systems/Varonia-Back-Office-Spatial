using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class PreventPlayWithoutLayer
{
    static PreventPlayWithoutLayer()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            var settings = LayerSettings.Load();
            if (settings == null || string.IsNullOrEmpty(settings.requiredLayerName) || LayerMask.NameToLayer(settings.requiredLayerName) == -1)
            {
                EditorApplication.isPlaying = false;
                Debug.LogError("[LayerSettings] Cannot enter Play Mode: Required layer is not properly configured.");
                LayerSettingsEditor.ShowWindow();
            }
        }
    }
}
