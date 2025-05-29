using UnityEngine;

public class BoxColliderAutoSize : MonoBehaviour
{
    void Awake()
    {
        var box = GetComponent<BoxCollider>();
        box.center = new Vector3(0, 10, 0);
        box.size = new Vector3(0.15f, 60f, 1f);
    }
}
