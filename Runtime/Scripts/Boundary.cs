using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using VaroniaBackOffice;

public class Boundary : MonoBehaviour
{
    //===== Public Fields =====//


    [Header("Prefabs & Materials")]
    public GameObject Center_; // Center prefab
    public GameObject Wall_prefab; // Wall prefab
    public GameObject SmallObstacle; // Small obstacle prefab
    public GameObject MediumObstacle; // Medium obstacle prefab
    public GameObject LargeObstacle; // Large obstacle prefab
    public Material Source_Material; // Default material
    public Material Source_Material_Alt; // Alternative material (more visible)

    [Header("Settings")]
    public bool Spectator; // Spectator mode (no tracking)
    public bool SpatialOK; // Indicates if spatial setup is ready
    [HideInInspector]
    public bool InitStart = true; // Initialization flag

    [Header("Runtime Data")]
    public List<GameObject> Boundaries = new List<GameObject>(); // List of created boundaries
    public List<List<MeshRenderer>> meshRenderers = new List<List<MeshRenderer>>(); // Wall mesh renderers
    public Transform ItemTrack; // Player or object to track
    public Transform MainCenter; // Main center reference
    public UnityEvent BoundaryIsReady = new UnityEvent(); // Event when boundaries are ready

    //===== Private Fields =====//

    private List<Vector4> maskCenters = new List<Vector4>(); // Center points for shaders
    private Transform tempParent; // Temporary parent during setup
    private bool firstLoop = true; // Used for first update pass

    // Shader property IDs to avoid string calls
    private static readonly int ColorID = Shader.PropertyToID("_Color");
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
    private static readonly int MaskCentersArrayID = Shader.PropertyToID("_MaskCentersArray");
    private static readonly int MaskRadiusID = Shader.PropertyToID("_MaskRadius");
    private static readonly int MaskCentersCountID = Shader.PropertyToID("_MaskCentersCount");
    private static readonly int MainTexID = Shader.PropertyToID("_MainTex");


    private void Awake()
    {

        tempParent = transform.parent; // Store parent reference
        transform.parent = null; // Detach during setup
    }

    //----------------------------------------------------------------------



    private IEnumerator Start()
    {
        // If the spatial config is missing, destroy this object
        if (!File.Exists(Config.VaroniaFolder_Path + "/NewSpatial.json"))
        {
            Destroy(gameObject);
            yield break;
        }

        // Wait for config to be fully loaded
        while (Config.Spatial == null)
            yield return null;

        ClearChildren();
        yield return BuildBoundaries();

        foreach (var boundary in Boundaries)
            boundary.SetActive(true);

        SpatialOK = true;
        yield return new WaitForSeconds(0.1f);

        transform.SetParent(tempParent, false); // Reattach to original parent


        BoundaryIsReady.Invoke(); // Notify that boundaries are ready

#if UNITY_2022_1_OR_NEWER
        var A = Object.FindObjectsByType<Sync>(FindObjectsSortMode.None);
#else
       var A = Object.FindObjectsOfType<Sync>();   
#endif

        foreach (var item in A)
        {
            item.Ready();
        }


    }

    //

    //----------------------------------------------------------------------

    private void Update()
    {
        if (meshRenderers.Count == 0 || !SpatialOK)
            return;

        if (firstLoop)
        {
            InitializeMaterials();
        }

        UpdateMaskCenters();
        UpdateMaterialsWithTracking();

        if (firstLoop)
            firstLoop = false;
    }

    //====================================================================//
    //                            Custom Methods                          //
    //====================================================================//

    private void ClearChildren()
    {
        // Destroy all child objects
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }

    //----------------------------------------------------------------------

    private IEnumerator BuildBoundaries()
    {
        int index = 0;

        foreach (Boundary_ boundary in Config.Spatial.Boundaries)
        {
            // Create parent GameObject for the boundary
            GameObject boundaryParent = new GameObject(boundary.MainBoundary ? "Main Boundary" : "Boundary");
            boundaryParent.transform.parent = transform;
            meshRenderers.Add(new List<MeshRenderer>());

            BuildWalls(boundary, boundaryParent, index);

            var center = Instantiate(Center_, FindCenterOfTransforms(boundary.Points), Quaternion.identity, transform).GetComponent<InBoundary>();
            yield return new WaitForSeconds(0.1f);

            if (boundary.MainBoundary)
            {
                center.name = "Main Center";
                MainCenter = center.transform;
            }

            yield return null;

            AdjustBoundaryOrientation(boundary, boundaryParent, center);
            RemoveCollidersIfNeeded(boundary, boundaryParent);
            AddObstacles(boundary, boundaryParent);

            boundaryParent.SetActive(false);
            Boundaries.Add(boundaryParent);
            index++;
        }
    }

    //----------------------------------------------------------------------

    private void BuildWalls(Boundary_ boundary, GameObject parent, int index)
    {
        for (int i = 0; i < boundary.Points.Count; i++)
        {
            Vector3 point = boundary.Points[i].asVec3();
            Vector3 nextPoint = boundary.Points[(i + 1) % boundary.Points.Count].asVec3();

            var wall = Instantiate(Wall_prefab, point, Quaternion.identity, parent.transform);
            wall.transform.LookAt(nextPoint);
            wall.transform.localScale = new Vector3(wall.transform.localScale.x, wall.transform.localScale.y, Vector3.Distance(point, nextPoint));

            Transform wallChild = wall.transform.GetChild(0);
            wallChild.localScale += new Vector3(0, 39, 0);
            wallChild.position += new Vector3(0, 1.95f, 0);

            meshRenderers[index].Add(wallChild.GetComponent<MeshRenderer>());

            var rimRenderer = wall.transform.GetChild(1).GetComponent<MeshRenderer>();
            var color = new Color(boundary.BoundaryColor.x, boundary.BoundaryColor.y, boundary.BoundaryColor.z);
            rimRenderer.material.color = color;
            rimRenderer.material.SetColor(EmissionColorID, color);
        }
    }

    //----------------------------------------------------------------------

    private void AdjustBoundaryOrientation(Boundary_ boundary, GameObject parent, InBoundary center)
    {

        if (!center) return;

        // If boundary is out of limits, flip it
        if (center.currentStatus == Where.OutLimit)
        {
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                var wall = parent.transform.GetChild(i);
                Vector3 scale = wall.localScale;
                wall.localScale = new Vector3(-scale.x, scale.y, scale.z);

                var outWall = GetChildByName(wall, "_OutWall");
                if (outWall) outWall.localScale = new Vector3(-outWall.localScale.x, outWall.localScale.y, outWall.localScale.z);

                var inWall = GetChildByName(wall, "_InWall");
                if (inWall) inWall.localScale = new Vector3(-inWall.localScale.x, inWall.localScale.y, inWall.localScale.z);
            }
        }

        // If boundary needs to be reversed, flip it
        if (boundary.Reverse)
        {
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                var wall = parent.transform.GetChild(i);
                Vector3 scale = wall.localScale;
                wall.localScale = new Vector3(-scale.x, scale.y, scale.z);

                var outWall = GetChildByName(wall, "_OutWall");
                if (outWall) outWall.localScale = new Vector3(-outWall.localScale.x, outWall.localScale.y, outWall.localScale.z);

                var inWall = GetChildByName(wall, "_InWall");
                if (inWall) inWall.localScale = new Vector3(-inWall.localScale.x, inWall.localScale.y, inWall.localScale.z);
            }
        }
    }

    //----------------------------------------------------------------------

    private void RemoveCollidersIfNeeded(Boundary_ boundary, GameObject parent)
    {
        // Remove colliders from non-main boundaries
        if (!boundary.MainBoundary)
        {
            foreach (var collider in parent.GetComponentsInChildren<BoxCollider>())
                Destroy(collider);
        }
    }

    //----------------------------------------------------------------------

    private void AddObstacles(Boundary_ boundary, GameObject parent)
    {
        if (boundary.Obstacles == null)
            return;

        foreach (var obstacle in boundary.Obstacles)
        {
            GameObject prefab;
            switch (obstacle.Size)
            {
                case ObstacleSize.Small:
                    prefab = SmallObstacle;
                    break;
                case ObstacleSize.Medium:
                    prefab = MediumObstacle;
                    break;
                case ObstacleSize.Large:
                    prefab = LargeObstacle;
                    break;
                default:
                    prefab = null;
                    break;
            }


            if (prefab != null)
            {
                var obj = Instantiate(prefab, obstacle.Position.asVec3(), Quaternion.Euler(obstacle.Rotation.asVec3()), parent.transform);
                obj.transform.localScale = Vector3.one * obstacle.Scale;
            }
        }
    }

    //----------------------------------------------------------------------

    private void InitializeMaterials()
    {
        // Initialize wall materials based on visibility setting
        for (int x = 0; x < meshRenderers.Count; x++)
        {
            for (int i = 0; i < meshRenderers[x].Count; i++)
            {
                var renderer = meshRenderers[x][i];
                bool moreVisible = Config.Spatial.Boundaries[x].BoundaryMoreVisible;

                renderer.material = moreVisible ? Source_Material_Alt : Source_Material;
                renderer.material.SetColor(ColorID, new Color(
                    Config.Spatial.Boundaries[x].BoundaryColor.x,
                    Config.Spatial.Boundaries[x].BoundaryColor.y,
                    Config.Spatial.Boundaries[x].BoundaryColor.z,
                    renderer.material.GetColor(ColorID).a));
                renderer.material.SetInt(MaskCentersCountID, 1);
                renderer.material.SetTextureScale(MainTexID, new Vector2(renderer.transform.lossyScale.x * 10f, 40f));
            }
        }
    }

    //----------------------------------------------------------------------

    private void UpdateMaskCenters()
    {
        maskCenters.Clear();

        // Update tracking position
        if (!Spectator)
        {
            if (ItemTrack == null)
            {
                var trackObject = GameObject.Find("-Bound Track-");
                if (trackObject != null)
                    ItemTrack = trackObject.transform;
            }

            if (ItemTrack != null)
                maskCenters.Add(ItemTrack.position);
        }
    }

    //----------------------------------------------------------------------

    private void UpdateMaterialsWithTracking()
    {
        for (int x = 0; x < meshRenderers.Count; x++)
        {
            for (int i = 0; i < meshRenderers[x].Count; i++)
            {
                var renderer = meshRenderers[x][i];

                if (!Spectator && maskCenters.Count > 0)
                {
                    renderer.material.SetVectorArray(MaskCentersArrayID, maskCenters);
                    renderer.material.SetFloat(MaskRadiusID, Config.Spatial.Boundaries[x].DisplayDistance);
                }
            }
        }
    }

    //====================================================================//
    //                           Utility Methods                          //
    //====================================================================//

    private static Transform GetChildByName(Transform parent, string childName)
    {
        // Find a child by name
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;
        }
        return null;
    }

    private static Vector3 FindCenterOfTransforms(List<Vector3_> points)
    {
        // Calculate center of multiple points
        var bounds = new Bounds(points[0].asVec3(), Vector3.zero);
        for (int i = 1; i < points.Count; i++)
            bounds.Encapsulate(points[i].asVec3());
        return bounds.center;
    }
}
