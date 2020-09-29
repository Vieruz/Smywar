using Firebase.Database;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerSaveManager : MonoBehaviour
{
    public static PlayerSaveManager DefaultInstance;
    public const string PLAYER_KEY = "Players";

    public List<PlayerController> players;
    public GameObject[] playerPrefabs;
    public Sprite[] smileySprites;
    public MapManager map;

    private FirebaseDatabase _database;

    private void Start()
    {
        if(DefaultInstance == null)
        {
            DefaultInstance = this;
        }

        _database = FirebaseDatabase.DefaultInstance;

        players = new List<PlayerController>();
        _database.GetReference(PLAYER_KEY).ChildRemoved += HandleChildRemoved;
        _database.GetReference(PLAYER_KEY).ChildAdded += HandleChildAdd;
        _database.GetReference(PLAYER_KEY).ChildChanged += HandleChildChange;
    }

    private void OnApplicationQuit()
    {
        _database.GetReference(PLAYER_KEY).ChildRemoved -= HandleChildRemoved;
        _database.GetReference(PLAYER_KEY).ChildAdded -= HandleChildAdd;
        _database.GetReference(PLAYER_KEY).ChildChanged -= HandleChildChange;
    }

    public void SavePlayer(PlayerData player)
    {
        _database.GetReference(PLAYER_KEY).Child(player.Uid).SetRawJsonValueAsync(JsonUtility.ToJson(player));
    }

    public void SaveTargetPosition(PlayerData player)
    {
        if(player.LP > 0)
            _database.GetReference(PLAYER_KEY).Child(player.Uid).Child("targetPosition").SetRawJsonValueAsync(JsonUtility.ToJson(player.targetPosition));
    }

    public void SavePosition(PlayerData player)
    {
        if (player.LP > 0)
            _database.GetReference(PLAYER_KEY).Child(player.Uid).Child("playerPosition").SetRawJsonValueAsync(JsonUtility.ToJson(player.playerPosition));
    }

    public void SaveInventory(PlayerData player, ItemData item)
    {
        if (player.LP > 0)
            _database.GetReference(PLAYER_KEY).Child(player.Uid).Child("Inventory").Child(item.type.ToString()).SetRawJsonValueAsync(JsonUtility.ToJson(item));
    }

    void HandleChildAdd(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot
        var playerToAdd = JObject.Parse(args.Snapshot.GetRawJsonValue());
        PlayerData data = playerToAdd.ToObject<PlayerData>();
        Debug.Log("Player added: " + data.Name);
        AddPlayer(data);
    }

    void HandleChildChange(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot
        var playerToChange = JObject.Parse(args.Snapshot.GetRawJsonValue());
        PlayerData data = playerToChange.ToObject<PlayerData>();
        UpdatePlayer(data);
    }

    void HandleChildRemoved(object sender, ChildChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        // Do something with the data in args.Snapshot
        var playerRemoved = JObject.Parse(args.Snapshot.GetRawJsonValue());
        PlayerData data = playerRemoved.ToObject<PlayerData>();
        PlayerController playerToKill = players.Find((p) => p.PlayerData.Uid == data.Uid);
        players.Remove(playerToKill);
        playerToKill.Kill();
    }

    public void UpdatePlayer(PlayerData data)
    {
        PlayerController playerToUpdate = players.Find((player) => player.PlayerData.Uid == data.Uid);
        playerToUpdate.UpdateFromDb(data);
    }

    public void AddPlayer(PlayerData data)
    {
        if (!players.Find((p) => p.PlayerData.Uid == data.Uid))
        {
            int classIndex = 0;
            switch (data.PlayerClass)
            {
                case PlayerClass.Knight:
                    classIndex = 0;
                    break;
                case PlayerClass.Mage:
                    classIndex = 1;
                    break;
                case PlayerClass.Priest:
                    classIndex = 2;
                    break;
            }

            // Correction de position
            Vector3Int checkPosition = Vector3Int.RoundToInt(data.playerPosition);
            if(map.maps[1].GetTile(checkPosition) != null)
            {
                Vector3Int newPosition = checkPosition;
                for(int x = -1; x <= 1; x++)
                {
                    newPosition.x = checkPosition.x + x;
                    for(int y = -1; y <= 1; y++)
                    {
                        newPosition.y = checkPosition.y + y;
                        if (map.maps[1].GetTile(newPosition) == null)
                        {
                            data.playerPosition = newPosition;
                            break;
                        }
                    }
                }
            }

            GameObject newPlayer = Instantiate<GameObject>(playerPrefabs[classIndex], data.playerPosition, Quaternion.identity);
            PlayerController playerScript = newPlayer.GetComponent<PlayerController>();
            players.Add(playerScript);
            playerScript.UpdateFromDb(data);
        }
    }


    public async Task<bool> SaveExists(string playerUid)
    {
        var dataSnapshot = await _database.GetReference(PLAYER_KEY).Child(playerUid).GetValueAsync();
        return dataSnapshot.Exists;
    }

    public PlayerController FindPlayer(string playerUid)
    {
        return players.Find((p) => p.PlayerData.Uid == playerUid);
    }

    public void EraseSave(PlayerController player, string killerName)
    {
        _database.GetReference(PLAYER_KEY).Child(player.PlayerData.Uid).RemoveValueAsync();
        PlayerEvent pEvent = new PlayerEvent();
        pEvent.uid = player.PlayerData.Uid;
        pEvent.playerEventType = PlayerEventType.Killed;
        pEvent.title = "!!! MORT !!!";
        pEvent.message = "Tu as été tué par " + killerName + " !";
        PlayerEventManager.DefaultInstance.SaveEvent(pEvent);
    }

    public Sprite GetSmileySprite(int level)
    {
        if(players.Count > 0)
        {
            players.Sort((p1, p2) => (p2.PlayerData.Level.CompareTo(p1.PlayerData.Level)));
            float maxLevel = (float) players[0].PlayerData.Level;
            float lvl = (float)level;
            float indexFloat = (level / maxLevel) * (smileySprites.Length - 1);
            int index = (int) indexFloat;
            if(maxLevel < 50)
            {
                index = level;
            }
            return smileySprites[index];
        } else
        {
            return smileySprites[0];
        }
    }
}
