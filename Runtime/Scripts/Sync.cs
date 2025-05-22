
using System.Collections;
using UnityEngine;
using VaroniaBackOffice;

public class Sync : MonoBehaviour
{

    bool IsSync;

    public IEnumerator Start()
    {
        while (!IsSync)
        {
            yield return new WaitUntil(() => Boundary.Instance != null);

            if (Boundary.Instance != null)
                Boundary.Instance.BoundaryIsReady.AddListener(Ready);

            yield return new WaitForFixedUpdate();

        }
    }

    void Ready()
    {
        IsSync = true;

        transform.position = Config.Spatial.SyncPos.asVec3();
        transform.rotation = Config.Spatial.SyncQuaterion.asQuat();
    }
}
