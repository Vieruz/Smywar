using System.Collections;
using UnityEngine;

public class SyncPlayerToSave : MonoBehaviour
{
    public PlayerSaveManager playerSaveManager;
    public PlayerController player;

    private void Awake()
    {
        player.OnPlayerUpdated.AddListener(HandlePlayerUpdated);
    }

    private void Start()
    {
        playerSaveManager = PlayerSaveManager.DefaultInstance;
    }


    private void HandlePlayerUpdated()
    {
        playerSaveManager.SavePlayer(player.PlayerData);
    }
}
