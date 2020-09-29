using System.Collections.Generic;
using UnityEngine;

public class RankingUI : MonoBehaviour
{
    public ChairUI[] chairs;
    public Sprite hideSprite;

    public PlayerController[] _council;
    private PlayerSaveManager _psm;

    public void UpdateRankingUI()
    {
        _council = new PlayerController[5];
        _psm = PlayerSaveManager.DefaultInstance;
        List<PlayerController> allPlayers = _psm.players;
        allPlayers.Sort((p1, p2) => p2.PlayerData.Level.CompareTo(p1.PlayerData.Level));
        _council[0] = allPlayers[0];
        List<PlayerController> allKnights = allPlayers.FindAll((p) => p.PlayerData.PlayerClass == PlayerClass.Knight);
        allKnights.Remove(_council[0]);
        List<PlayerController> allMages = allPlayers.FindAll((p) => p.PlayerData.PlayerClass == PlayerClass.Mage
            || p.PlayerData.PlayerClass == PlayerClass.Priest);
        allMages.Remove(_council[0]);

        if (allKnights.Count > 0)
            _council[1] = allKnights[0];
        if(allMages.Count > 0)
            _council[2] = allMages[0];
        if(allKnights.Count > 1)
            _council[3] = allKnights[1];
        if(allMages.Count > 1)
            _council[4] = allMages[1];
        for(int i = 0; i < chairs.Length; i++)
        {
            if(_council[i] != null)
            {
                UpdateChair(chairs[i], _council[i]);
            } else
            {
                HideChair(chairs[i]);
            }
        }
    }

    public void UpdateChair(ChairUI chair, PlayerController player)
    {
        // Mettre à jour la place
        chair.smiley.sprite = player.smileySprite.sprite;
        chair.playerName.text = player.Name;
    }

    public void HideChair(ChairUI chair)
    {
        chair.smiley.sprite = hideSprite;
        chair.playerName.text = "";
    }
}
