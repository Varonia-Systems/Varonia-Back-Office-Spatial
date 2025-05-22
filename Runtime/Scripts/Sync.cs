
using System.Collections;
using UnityEngine;
using VaroniaBackOffice;

public class Sync : MonoBehaviour
{
    public IEnumerator Start()
    {
        yield return new WaitUntil(() => Boundary.Instance != null);
        yield return new WaitUntil(() => Boundary.Instance.BoundaryIsReady != null);
        Boundary.Instance.BoundaryIsReady.AddListener(Ready);
    }

    void Ready()
    {
        transform.position = Config.Spatial.SyncPos.asVec3();
        transform.rotation = Config.Spatial.SyncQuaterion.asQuat();
    }
}
