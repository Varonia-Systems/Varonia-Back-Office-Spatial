// MyPackage/Editor/LayerSettingsEditor.cs
using UnityEditor;
using UnityEngine;
using System.IO;

public class LayerSettingsEditor : EditorWindow
{
    private const string AssetPath = "Assets/Resources/VBO/LayerSettings/LayerSettings.asset";
    private LayerSettings settings;
    private string[] allLayers;
    private int selectedLayerIndex = -1;

    [MenuItem("Varonia/Boundary Layer Settings")]
    public static void ShowWindow()
    {
        var window = GetWindow<LayerSettingsEditor>("Boundary Layer Configuration");
        window.minSize = new Vector2(400, 140);
        window.Show();
    }

    private void OnEnable()
    {
        EnsureSettingsExists();

        settings = AssetDatabase.LoadAssetAtPath<LayerSettings>(AssetPath);
        RefreshLayerList();

        if (!string.IsNullOrEmpty(settings.requiredLayerName))
        {
            selectedLayerIndex = System.Array.IndexOf(allLayers, settings.requiredLayerName);
        }
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "To ensure this add-on works correctly, please select an **unused layer** that will be used internally by the system.\n\nYou can define it in the Layer Manager if needed.",
            MessageType.Info);

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Required Layer", EditorStyles.boldLabel);

        if (allLayers.Length == 0)
        {
            EditorGUILayout.HelpBox("No layers found. Please define a layer in Project Settings > Tags and Layers.", MessageType.Warning);
            return;
        }

        EditorGUI.BeginChangeCheck();
        selectedLayerIndex = EditorGUILayout.Popup("Select a Layer", selectedLayerIndex, allLayers);
        if (EditorGUI.EndChangeCheck())
        {
            settings.requiredLayerName = allLayers[selectedLayerIndex];
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }

        GUILayout.Space(10);
        if (!string.IsNullOrEmpty(settings.requiredLayerName) && LayerMask.NameToLayer(settings.requiredLayerName) == -1)
        {
            EditorGUILayout.HelpBox($"Warning: The selected layer '{settings.requiredLayerName}' does not exist in your project settings.", MessageType.Warning);
        }
        else if (!string.IsNullOrEmpty(settings.requiredLayerName))
        {
            EditorGUILayout.HelpBox($"Layer '{settings.requiredLayerName}' is valid and will be used by the system.", MessageType.Info);
        }
    }

    private void EnsureSettingsExists()
    {
        var dir = Path.GetDirectoryName(AssetPath);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        if (!File.Exists(AssetPath))
        {
            var asset = ScriptableObject.CreateInstance<LayerSettings>();
            AssetDatabase.CreateAsset(asset, AssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    private void RefreshLayerList()
    {
        var layers = new System.Collections.Generic.List<string>();

        for (int i = 0; i <= 31; i++)
        {
            var layerName = LayerMask.LayerToName(i);
            if (!string.IsNullOrEmpty(layerName))
                layers.Add(layerName);
        }

        allLayers = layers.ToArray();
    }
}
