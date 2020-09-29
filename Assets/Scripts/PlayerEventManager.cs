using Firebase.Auth;
using Firebase.Database;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerEventManager : MonoBehaviour
{
    public static PlayerEventManager DefaultInstance;
    public const string EVENT_KEY = "Events";

    private FirebaseDatabase _database;

    private void Start()
    {
        if(DefaultInstance == null)
        {
            DefaultInstance = this;
        }

        _database = FirebaseDatabase.DefaultInstance;
    }

    public void SaveEvent(PlayerEvent e)
    {
        // Enregistre l'event pour le joueur correspondant
        _database.GetReference(EVENT_KEY).Child(e.uid).SetRawJsonValueAsync(JsonUtility.ToJson(e));
    }

    public async Task<PlayerEvent> GetPlayerEvent(string playerUid)
    {
        var dataSnapshot = await _database.GetReference(EVENT_KEY).Child(playerUid).GetValueAsync();
        if (dataSnapshot.Exists)
        {
            var playerEvent = JObject.Parse(dataSnapshot.GetRawJsonValue());
            PlayerEvent data = playerEvent.ToObject<PlayerEvent>();
            return data;
        }
        else
        {
            return new PlayerEvent();
        }
    }

    public void EraseEvent()
    {
        string mainPlayerUid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        _database.GetReference(EVENT_KEY).Child(mainPlayerUid).RemoveValueAsync();
    }
}

public struct PlayerEvent
{
    public string uid;
    public PlayerEventType playerEventType;
    public string title;
    public string message;
}

public enum PlayerEventType
{
    Killed
}
