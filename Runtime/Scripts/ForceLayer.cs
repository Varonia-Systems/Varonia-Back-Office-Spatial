using UnityEngine;

public class ForceLayer : MonoBehaviour
{
    void Start()
    {
        var settings = LayerSettings.Load();
        if (settings == null)
        {
            Debug.LogError("LayerSettings not found in Resources.");
            return;
        }

        int layer = LayerMask.NameToLayer(settings.requiredLayerName);
        if (layer == -1)
        {
            Debug.LogError("Configured layer doesn't exist.");
            return;
        }

        gameObject.layer = layer;
       
    }
}
