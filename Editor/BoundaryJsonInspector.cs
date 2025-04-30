
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(BoundaryJsonHolder))]
public class BoundaryJsonInspector : Editor
{
    AreaData areaData;

    public override void OnInspectorGUI()
    {

        if (Event.current.type == EventType.Repaint || Event.current.type == EventType.MouseMove)
        {
            Repaint(); // Force Unity à redessiner l'inspector
        }


        BoundaryJsonHolder holder = (BoundaryJsonHolder)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Recharger depuis le fichier"))
        {
            holder.LoadJsonFromFile();
        }

        if (holder.LoadedData != null && holder.LoadedData.Boundaries != null)
        {
            GUILayout.Label("Aperçu des boundaries", EditorStyles.boldLabel);
            Rect previewRect = GUILayoutUtility.GetRect(300, 500);
            DrawBoundaryPreview(previewRect, holder.LoadedData.Boundaries, holder.LoadedData.SyncPos, holder.LoadedData.SyncQuaterion);
        }
        else
        {
            EditorGUILayout.HelpBox("Aucune donnée chargée.", MessageType.Info);
        }





        if (holder.LoadedData?.Boundaries != null)
        {
            GUILayout.Space(10);
            GUILayout.Label("Surface des boundaries :", EditorStyles.boldLabel);

            foreach (var boundary in holder.LoadedData.Boundaries)
            {
                float area = CalculateBoundaryArea(boundary.Points);
                EditorGUILayout.LabelField(boundary.ID, area.ToString("F2") + " m²");
            }
        }



    }

    void DrawBoundaryPreview(Rect rect, List<BoundaryE> boundaries, Vector3 originPos, Quaternion originRot)
    {
        Handles.BeginGUI();
        Vector2 center = rect.center;
        float scale = 25f;

        DrawGrid(rect, center, scale, 1f); // ? Grille 50cm
        DrawAxes(rect, center, scale);       // ? Repère centré

        foreach (var boundary in boundaries)
        {
            if (boundary.Points == null || boundary.Points.Count < 2)
                continue;

            Handles.color = new Color(boundary.BoundaryColor.x, boundary.BoundaryColor.y, boundary.BoundaryColor.z);

            for (int i = 0; i < boundary.Points.Count; i++)
            {
                Vector3 localP1 = boundary.Points[i];
                Vector3 localP2 = boundary.Points[(i + 1) % boundary.Points.Count];

                Vector3 worldP1 = TransformPoint(localP1, originPos, originRot);
                Vector3 worldP2 = TransformPoint(localP2, originPos, originRot);

                Vector2 guiP1 = center + new Vector2(worldP1.x, -worldP1.z) * scale;
                Vector2 guiP2 = center + new Vector2(worldP2.x, -worldP2.z) * scale;

                Handles.DrawLine(guiP1, guiP2);
            }

            if (boundary.Obstacles != null)
            {
                foreach (var obstacle in boundary.Obstacles)
                {
                    Vector3 worldObs = TransformPoint(obstacle.Position, originPos, originRot);
                    Vector2 pos = center + new Vector2(worldObs.x, -worldObs.z) * scale;
                    float radius = obstacle.Scale * obstacle.Size * scale * 0.5f;

                    Handles.color = Color.yellow;
                    Handles.DrawSolidDisc(pos, Vector3.forward, radius);
                    Handles.DrawWireDisc(pos, Vector3.forward, radius);
                }
            }
        }



        Vector2 origin = new Vector2(rect.xMin, rect.yMax); // coin bas-gauche du viewport
       // DrawAxisLinesAlongGrid(rect, origin, scale);
        DrawMouseCoordinates(rect, center, scale);
        Handles.EndGUI();
    }


    Vector3 TransformPoint(Vector3 localPoint, Vector3 originPos, Quaternion originRot)
    {
        return originPos + originRot * localPoint;
    }

    [Serializable]
    private class AreaDataWrapper
    {
        public AreaData wrapper;
    }




    private void DrawGrid(Rect rect, Vector2 center, float scale, float spacingMeters)
    {
        Handles.color = new Color(0.7f, 0.7f, 0.7f, 0.3f); // Gris clair, transparent

        float spacing = spacingMeters * scale;

        // Horizontal lines
        for (float y = rect.yMin; y < rect.yMax; y += spacing)
        {
            Handles.DrawLine(new Vector3(rect.xMin, y), new Vector3(rect.xMax, y));
        }

        // Vertical lines
        for (float x = rect.xMin; x < rect.xMax; x += spacing)
        {
            Handles.DrawLine(new Vector3(x, rect.yMin), new Vector3(x, rect.yMax));
        }

        Handles.color = Color.gray;
    }



    private float CalculateBoundaryArea(List<Vector3> points)
    {
        if (points == null || points.Count < 3)
            return 0f;

        float area = 0f;

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 a = points[i];
            Vector3 b = points[(i + 1) % points.Count];

            area += (a.x * b.z) - (b.x * a.z);
        }

        return Mathf.Abs(area) * 0.5f;
    }




    private void DrawAxes(Rect rect, Vector2 center, float scale)
    {
        float axisLength = 20f; // longueur en pixels

        // X axis (rouge)
        Handles.color = Color.red;
        Handles.DrawLine(
            center + new Vector2(-axisLength, 0),
            center + new Vector2(axisLength, 0)
        );

        // Z axis (bleu)
        Handles.color = Color.blue;
        Handles.DrawLine(
            center + new Vector2(0, -axisLength),
            center + new Vector2(0, axisLength)
        );

        // Centre (blanc)
        Handles.color = Color.white;
        Handles.DrawSolidDisc(center, Vector3.forward, 4f);
    }


    void DrawAxisLinesAlongGrid(Rect rect, Vector2 origin, float scale)
    {
        Handles.color = Color.green;
        GUIStyle labelStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = 10,
            normal = { textColor = Color.green },
            alignment = TextAnchor.UpperCenter
        };

        float step = 1f * scale; // 1 mètre = step pixels

        // --- Axe X : vers la droite ---
        for (float x = origin.x; x < rect.xMax; x += step)
        {
            float value = ((x - origin.x) / scale);
            Handles.DrawLine(new Vector2(x, origin.y - 4), new Vector2(x, origin.y + 4));
            Handles.Label(new Vector2(x, origin.y + 6), value.ToString("0"), labelStyle);
        }

        // --- Axe X : vers la gauche ---
        for (float x = origin.x - step; x > rect.xMin; x -= step)
        {
            float value = (x - origin.x) / scale;
            Handles.DrawLine(new Vector2(x, origin.y - 4), new Vector2(x, origin.y + 4));
            Handles.Label(new Vector2(x, origin.y + 6), value.ToString("0"), labelStyle);
        }

        // --- Axe Z : vers le haut ---
        for (float y = origin.y; y > rect.yMin; y -= step)
        {
            float value = (origin.y - y) / scale;
            Handles.DrawLine(new Vector2(origin.x - 4, y), new Vector2(origin.x + 4, y));
            Handles.Label(new Vector2(origin.x - 12, y - 6), value.ToString("0"), labelStyle);
        }

        // --- Axe Z : vers le bas ---
        for (float y = origin.y + step; y < rect.yMax; y += step)
        {
            float value = (origin.y - y) / scale;
            Handles.DrawLine(new Vector2(origin.x - 4, y), new Vector2(origin.x + 4, y));
            Handles.Label(new Vector2(origin.x - 12, y - 6), value.ToString("0"), labelStyle);
        }

        // --- Lignes d’axes principales
        Handles.DrawLine(new Vector2(rect.xMin, origin.y), new Vector2(rect.xMax, origin.y)); // X
        Handles.DrawLine(new Vector2(origin.x, rect.yMin), new Vector2(origin.x, rect.yMax)); // Z

        // --- Origine 0,0
        Handles.Label(origin + new Vector2(-10, 4), "0", labelStyle);
    }


    void DrawMouseCoordinates(Rect rect, Vector2 guiCenter, float scale)
    {
        Vector2 mousePos = Event.current.mousePosition;

        if (!rect.Contains(mousePos)) return;

        Vector2 delta = mousePos - guiCenter;
        float worldX = delta.x / scale;
        float worldZ = -delta.y / scale; // Z monte vers le haut dans GUI

        GUIStyle style = new GUIStyle(EditorStyles.label)
        {
            fontSize = 11,
            normal = { textColor = Color.yellow }
        };

        string text = $"X: {worldX:F2}  |  Z: {worldZ:F2}";

        Vector2 labelSize = style.CalcSize(new GUIContent(text));
        Rect labelRect = new Rect(rect.xMax - labelSize.x - 8, rect.yMax - labelSize.y - 4, labelSize.x + 4, labelSize.y + 2);

        EditorGUI.DrawRect(labelRect, new Color(0f, 0f, 0f, 0.5f));
        GUI.Label(labelRect, text, style);
    }


}
#endif
