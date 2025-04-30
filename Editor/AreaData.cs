using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AreaData
{
    public int IdArea;
    public Vector3 SyncPos;
    public Quaternion SyncQuaterion;
    public List<BoundaryE> Boundaries;

}


[Serializable]
public class Obstacle
{
    public Vector3 Position;
    public Vector3 Rotation;
    public float Size;
    public float Scale;
    public int SpecialId;
}

[Serializable]
public class BoundaryE
{
    public List<Vector3> Points;
    public Vector3 BoundaryColor;
    public string ID;
    public List<Obstacle> Obstacles;
}
