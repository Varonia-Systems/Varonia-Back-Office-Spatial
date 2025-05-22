
using System.Collections;
using UnityEngine;
using VaroniaBackOffice;

public class Sync : MonoBehaviour
{


    public void Ready()
    {
        transform.position = Config.Spatial.SyncPos.asVec3();
        transform.rotation = Config.Spatial.SyncQuaterion.asQuat();
    }
}
