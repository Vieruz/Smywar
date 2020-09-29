using Firebase.Auth;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCreation : MonoBehaviour
{
    public string mainPlayerUid;

    public GameObject characterCreationPanel;
    public InputField playerName;
    public Dropdown playerClass;
    public MapManager map;

    private Coroutine _coroutine;
    private PlayerSaveManager playerSaveManager;
    private GameObject _gameTitle;
    private GameObject _uiGamePanel;

    private void Start()
    {
        _gameTitle = GameController.DefaultInstance.gameTitle;
        _uiGamePanel = GameController.DefaultInstance.gamePanel;
        mainPlayerUid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        playerSaveManager = GetComponent<PlayerSaveManager>();
        Trigger();
    }

    public void Trigger()
    {
        if(_coroutine == null)
        {
            _coroutine = StartCoroutine(CharacterCreationCoroutine());
        }
    }

    private IEnumerator CharacterCreationCoroutine()
    {
        var saveExistsTask = playerSaveManager.SaveExists(mainPlayerUid);
        yield return new WaitUntil(() => saveExistsTask.IsCompleted);

        if(!saveExistsTask.Result)
        {
            StartCoroutine("OpenCreationPanel");
        } else
        {
            StartCoroutine("LoadMainPlayer");
            _uiGamePanel.SetActive(true);
        }

        _coroutine = null;
    }

    IEnumerator OpenCreationPanel()
    {
        _gameTitle.SetActive(false);
        _uiGamePanel.SetActive(false);

        Task<PlayerEvent> pEvent = PlayerEventManager.DefaultInstance.GetPlayerEvent(mainPlayerUid);
        yield return new WaitUntil(() => pEvent.IsCompleted);

        if(pEvent.Result.uid == mainPlayerUid)
        {
            // Afficher le message.
            GameController.DefaultInstance.DisplayEvent(pEvent.Result);
        } else
        {
            characterCreationPanel.SetActive(true);
        }
    }

    public void CreatePlayer()
    {
        PlayerClass classe = (PlayerClass) playerClass.value;

        //Position du perso
        Vector3Int randomPosition = new Vector3Int(Random.Range(1, map.width), Random.Range(1, map.height), 0);
        while (map.maps[1].GetTile(randomPosition) != null)
        {
            randomPosition = new Vector3Int(Random.Range(1, map.width), Random.Range(1, map.height), 0);
        }

        //Création du perso
        CreateCharacter(mainPlayerUid, playerName.text, classe, randomPosition);
        StartCoroutine("LoadMainPlayer");
        _uiGamePanel.SetActive(true);
        _gameTitle.SetActive(false);
        characterCreationPanel.SetActive(false);
    }

    public void CreateCharacter(string uid, string name, PlayerClass classe, Vector3Int randomPosition)
    {
        PlayerData data = new PlayerData();
        data.Uid = uid;
        data.Name = name;
        data.PlayerClass = classe;
        data.Level = 1;
        data.Floor = 1;

        switch (classe)
        {
            case PlayerClass.Knight:
                data.Strength = Random.Range(3, 19) + 5;
                data.Dexterity = Random.Range(3, 19) + 3;
                data.Stamina = Random.Range(3, 19) + 8;
                data.Intelligence = Random.Range(3, 19);
                data.Wisdom = Random.Range(3, 19);
                break;
            case PlayerClass.Mage:
                data.Strength = Random.Range(3, 19);
                data.Dexterity = Random.Range(3, 19) + 3;
                data.Stamina = Random.Range(3, 19);
                data.Intelligence = Random.Range(3, 19) + 8;
                data.Wisdom = Random.Range(3, 19) + 5;
                break;
            case PlayerClass.Priest:
                data.Strength = Random.Range(3, 19);
                data.Dexterity = Random.Range(3, 19);
                data.Stamina = Random.Range(3, 19) + 3;
                data.Intelligence = Random.Range(3, 19) + 5;
                data.Wisdom = Random.Range(3, 19) + 8;
                break;
            default:
                Debug.Log("Not implemented classe: " + classe.ToString());
                data.Strength = Random.Range(3, 19);
                data.Dexterity = Random.Range(3, 19);
                data.Stamina = Random.Range(3, 19);
                data.Intelligence = Random.Range(3, 19);
                data.Wisdom = Random.Range(3, 19);
                break;
        }

        data.MaxLP = 100 + (data.Stamina * 5);
        data.LP = data.MaxLP;
        data.MaxMP = data.Wisdom * 5;
        data.MP = data.MaxMP;
        data.Gold = 100;

        data.playerPosition = randomPosition;
        data.targetPosition = randomPosition;
        data.Inventory = new ItemData[3];
        data.Inventory[0].quantity = 3;

        playerSaveManager.SavePlayer(data);
    }

    IEnumerator LoadMainPlayer()
    {
        yield return new WaitUntil(() => playerSaveManager.FindPlayer(mainPlayerUid));
        PlayerController mainPlayer = playerSaveManager.FindPlayer(FirebaseAuth.DefaultInstance.CurrentUser.UserId);
        mainPlayer.SetAsMainPlayer();
    }
}
