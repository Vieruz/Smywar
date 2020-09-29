using Firebase.Database;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    public const string MAP_KEY = "Map";
    public const string FLOOR_KEY = "Floor_";
    public const string ROOM_KEY = "Room_";
    public int floorId = 0;
    public int width = 100;
    public int height = 100;

    public Tilemap[] maps;
    public TileBase[] tiles;
    public List<Room> roomTemplates;
    private FirebaseDatabase _database;
    private List<MapData> _data;

    private void Awake()
    {
        _database = FirebaseDatabase.DefaultInstance;
    }

    private void Start()
    {
        StartCoroutine(GenerateMap());
    }

    public Task SaveRoom(Room room)
    {
        Task saveTask = _database.GetReference(MAP_KEY).Child(FLOOR_KEY + floorId).Child(ROOM_KEY + room.id).SetRawJsonValueAsync(JsonUtility.ToJson(room));
        return saveTask;
    }

    public async Task<List<MapData>> LoadMap()
    {
        var dataSnapshot = await _database.GetReference(MAP_KEY).Child(FLOOR_KEY + floorId).GetValueAsync();
        if (!dataSnapshot.Exists)
        {
            return null;
        }
        var rooms = JObject.Parse(dataSnapshot.GetRawJsonValue());
        List<MapData> floor = new List<MapData>();
        foreach(var r in rooms)
        {
            Room rm = r.Value.ToObject<Room>();
            if(rm.data != null)
            {
                foreach (MapData d in rm.data)
                {
                    MapData f = new MapData();
                    f.position = new Vector3Int(d.position.x + rm.offset.x, d.position.y + rm.offset.y, 0);
                    f.tileIndex = d.tileIndex;
                    floor.Add(f);
                }
            }
        }
        return floor;
    }

    public async Task<List<Room>> LoadRoomTemplates()
    {
        roomTemplates = new List<Room>();
        var dataSnapshot = await _database.GetReference(RoomBuilder.MAP_KEY).GetValueAsync();
        if (!dataSnapshot.Exists)
        {
            return null;
        }
        var rooms = JObject.Parse(dataSnapshot.GetRawJsonValue()); 
        foreach (var r in rooms)
        {
            Room rm = r.Value.ToObject<Room>();
            roomTemplates.Add(rm);
        }

        return roomTemplates;
    }

    public IEnumerator GenerateMap()
    {
        var mapDataTask = LoadMap();
        yield return new WaitUntil(() => mapDataTask.IsCompleted);
        var mapData = mapDataTask.Result;
        if (mapData != null)
        {
            _data = mapData;
            DisplayMap();
        }
        else
        {
            StartCoroutine(GenerateFloor());
        }
    }

    private void DisplayMap()
    {
        DisplayMapLimits();

        foreach (MapData m in _data)
        {
            maps[m.tileIndex].SetTile(m.position, tiles[m.tileIndex]);
        }

        GameController.DefaultInstance.StopLoading();
    }

    public void DisplayMapLimits()
    {
        int xMax = width - 1;
        int yMax = height - 1;

        for (int y = -1; y <= yMax + 1; y++)
        {
            for (int x = -1; x <= xMax + 1; x++)
            {
                if (x <= 0 || y <= 0 || x >= xMax || y >= yMax)
                {
                    MapData m = new MapData();
                    m.position = new Vector3Int(x, y, 0);
                    m.tileIndex = 1;
                    _data.Add(m);
                }
                MapData g = new MapData();
                g.position = new Vector3Int(x, y, 0);
                g.tileIndex = 0;
                _data.Add(g);
            }
        }
    }

    public IEnumerator GenerateFloor()
    {
        //Load room templates
        var roomTemplatesTask = LoadRoomTemplates();
        yield return new WaitUntil(() => roomTemplatesTask.IsCompleted);
        var roomTemplatesData = roomTemplatesTask.Result;

        //Generate floor
        _data = new List<MapData>();

        int id = 0;
        int xMax = width - 1;
        int yMax = height - 1;
        Room room = new Room();

        for (int y = 0; y <= yMax; y++)
        {
            for (int x = 0; x <= xMax; x++)
            {
                room = GenerateRoom(id, new Vector3Int(x, y, 0));
                var saveTask =  SaveRoom(room);
                yield return new WaitUntil(() => saveTask.IsCompleted);
                id++;
                x += room.width -1;
            }
            y += room.height -1;
        }

        var mapDataTask = LoadMap();
        yield return new WaitUntil(() => mapDataTask.IsCompleted);
        var mapData = mapDataTask.Result;
        _data = mapData;
        DisplayMap();
    }

    public Room GenerateRoom(int id, Vector3Int offset)
    {
        int randomRoom = Random.Range(0, roomTemplates.Count);
        Room room = roomTemplates[randomRoom];

        room.id = id;
        room.offset = offset;

        return room;
    }

    public Vector3Int GetRandomPosition()
    {
        // Trouver une position libre.
        Vector3Int randomPosition = new Vector3Int(Random.Range(1, width), Random.Range(1, height), 0);
        while (maps[1].GetTile(randomPosition) != null)
        {
            randomPosition = new Vector3Int(Random.Range(1, width), Random.Range(1, height), 0);
        }
        return randomPosition;
    }
}
