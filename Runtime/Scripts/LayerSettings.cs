using UnityEngine;

[System.Serializable]
public class LayerSettings : ScriptableObject
{
    public string requiredLayerName;

    public static LayerSettings Load()
    {
        return Resources.Load<LayerSettings>("VBO/LayerSettings/LayerSettings");
    }
}
