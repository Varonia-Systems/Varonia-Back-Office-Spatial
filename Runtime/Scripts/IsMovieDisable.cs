using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VaroniaBackOffice;

public class IsMovieDisable : MonoBehaviour
{
    [Header("Settings")]
    public bool DisableGameobject; // If true, disables/enables a child GameObject instead of the MeshRenderer

    private MeshRenderer meshRenderer;
    private Transform childTransform;

    private IEnumerator Start()
    {
        // Wait until VaroniaConfig is fully initialized
        yield return new WaitUntil(() => Config.VaroniaConfig != null);

        // Cache components for better performance
        if (!DisableGameobject)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }
        else
        {
            if (transform.childCount > 0)
                childTransform = transform.GetChild(0);
        }

        // Continuously update based on the Movie setting
        while (true)
        {
            if (Config.VaroniaConfig.Movie)
            {
                // If Movie mode is active, disable visuals
                if (!DisableGameobject && meshRenderer != null)
                    meshRenderer.enabled = false;
                else if (childTransform != null)
                    childTransform.gameObject.SetActive(false);
            }
            else
            {
                // If Movie mode is inactive, enable visuals
                if (!DisableGameobject && meshRenderer != null)
                    meshRenderer.enabled = true;
                else if (childTransform != null)
                    childTransform.gameObject.SetActive(true);
            }

            yield return null; // Wait for next frame
        }
    }
}
