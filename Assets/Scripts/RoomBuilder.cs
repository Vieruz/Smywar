using Firebase.Database;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomBuilder : MonoBehaviour
{
    public const string MAP_KEY = "RoomTemplates";
    public const string ROOM_KEY = "TRoom_";
    public int id;
    public int width = 10;
    public int height = 10;

    public Tilemap[] maps;

    private FirebaseDatabase _database;

    private void Awake()
    {
        _database = FirebaseDatabase.DefaultInstance;
    }

    public void Save()
    {
        Room room = new Room();
        room.id = id;
        room.tid = id;
        room.width = width;
        room.height = height;
        room.offset = Vector3Int.zero;
        room.data = new List<MapData>();

        for(int t = 0; t < maps.Length; t++)
        {
            for(int y = 0; y < height; y++)
            {
                for(int x = 0; x < width; x++)
                {
                    Vector3Int position = new Vector3Int(x, y, 0);
                    if (maps[t].GetTile(position))
                    {
                        MapData m = new MapData();
                        m.position = position;
                        m.tileIndex = t + 1;

                        room.data.Add(m);
                    }
                }
            }
        }

        SaveRoom(room);
    }

    public void DeleteTemplates()
    {
        Debug.Log("Room templates deleted! ");
        _database.GetReference(MAP_KEY).RemoveValueAsync();
    }

    public void DeleteMap()
    {
        Debug.Log("Map deleted! ");
        _database.GetReference(MapManager.MAP_KEY).RemoveValueAsync();
    }

    public void SaveRoom(Room room)
    {
        Debug.Log("Template Room_" + room.tid + " saved: " + JsonUtility.ToJson(room, true));
        _database.GetReference(MAP_KEY).Child(ROOM_KEY + room.id).SetRawJsonValueAsync(JsonUtility.ToJson(room));
    }
}
