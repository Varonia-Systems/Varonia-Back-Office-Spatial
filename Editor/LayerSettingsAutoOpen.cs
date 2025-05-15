using UnityEditor;
using UnityEngine;
using System.IO;

[InitializeOnLoad]
public static class LayerSettingsAutoOpen
{
    private const string AssetPath = "Assets/Resources/VBO/LayerSettings/LayerSettings.asset";

    static LayerSettingsAutoOpen()
    {
        EditorApplication.update += CheckLayerSettings;
    }

    private static void CheckLayerSettings()
    {
        EditorApplication.update -= CheckLayerSettings;

        if (!File.Exists(AssetPath))
        {
            Debug.Log("[LayerSettings] Asset not found, opening config window.");
            LayerSettingsEditor.ShowWindow();
            return;
        }

        var settings = AssetDatabase.LoadAssetAtPath<LayerSettings>(AssetPath);

        if (settings == null || string.IsNullOrEmpty(settings.requiredLayerName) || LayerMask.NameToLayer(settings.requiredLayerName) == -1)
        {
            Debug.Log("[LayerSettings] Layer is missing or invalid, opening config window.");
            LayerSettingsEditor.ShowWindow();
        }
    }
}
