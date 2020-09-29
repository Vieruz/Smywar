using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct MapData
{
    public Vector3Int position;
    public int tileIndex;
}

[Serializable]
public struct Room
{
    public int id;
    public int tid;
    public int width;
    public int height;
    public Vector3Int offset;
    public List<MapData> data;
}