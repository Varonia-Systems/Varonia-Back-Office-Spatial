using UnityEngine;
using System;
using System.IO;

public class BoundaryJsonHolder : MonoBehaviour
{
    [HideInInspector]
    public string fileName = "NewSpatial.json"; // nom du fichier
    [HideInInspector]
    public AreaData LoadedData;

    private void OnValidate()
    {
        LoadJsonFromFile();
    }

    public void LoadJsonFromFile()
    {
        string customPath = Application.persistentDataPath.Replace(Application.companyName + "/" + Application.productName, "Varonia");
        string fullPath = Path.Combine(customPath, fileName);

        if (File.Exists(fullPath))
        {
            try
            {
                string jsonText = File.ReadAllText(fullPath);
                LoadedData = JsonUtility.FromJson<AreaDataWrapper>("{\"wrapper\":" + jsonText + "}").wrapper;
                Debug.Log("Fichier JSON chargé avec succès depuis : " + fullPath);
            }
            catch (Exception e)
            {
                Debug.LogError("Erreur parsing JSON : " + e.Message);
                LoadedData = null;
            }
        }
        else
        {
            Debug.LogWarning("Fichier JSON introuvable : " + fullPath);
            LoadedData = null;
        }
    }

    [Serializable]
    private class AreaDataWrapper
    {
        public AreaData wrapper;
    }




    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (LoadedData?.Boundaries == null)
            return;

        Vector3 center = LoadedData.SyncPos;
        Quaternion rotation = LoadedData.SyncQuaterion;


        // ?? Grille 50cm x 50cm (affichée sur 20m x 20m autour du centre)
        Gizmos.color = new Color(0f, 1f, 1f, 0.2f);
        float gridSize = 1f;
        int gridCount = 40; // 20m x 20m

        for (int x = -gridCount / 2; x <= gridCount / 2; x++)
        {
            Vector3 start = center  + new Vector3(x * gridSize, 0.15f, -gridCount / 2f * gridSize);
            Vector3 end = center +  new Vector3(x * gridSize, 0.15f, gridCount / 2f * gridSize);
            Gizmos.DrawLine(start, end);
        }
        for (int z = -gridCount / 2; z <= gridCount / 2; z++)
        {
            Vector3 start = center  + new Vector3(-gridCount / 2f * gridSize, 0.15f, z * gridSize);
            Vector3 end = center +  new Vector3(gridCount / 2f * gridSize, 0.15f, z * gridSize);
            Gizmos.DrawLine(start, end);
        }



        // ?? Boundaries
        foreach (var boundary in LoadedData.Boundaries)
        {
            if (boundary.Points == null || boundary.Points.Count < 2)
                continue;

            Gizmos.color = new Color(boundary.BoundaryColor.x, boundary.BoundaryColor.y, boundary.BoundaryColor.z);
            for (int i = 0; i < boundary.Points.Count; i++)
            {
                Vector3 localA = boundary.Points[i];
                Vector3 localB = boundary.Points[(i + 1) % boundary.Points.Count];

                Vector3 worldA = center + rotation * localA;
                Vector3 worldB = center + rotation * localB;

                Gizmos.DrawLine(worldA, worldB);
            }

            // ?? Obstacles
            if (boundary.Obstacles != null)
            {
                foreach (var obs in boundary.Obstacles)
                {
                    Vector3 worldPos = center + rotation * obs.Position;
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawCube(worldPos+new Vector3(0,1,0), new Vector3(1,2,1));
                }
            }
        }
#endif
    }


}
