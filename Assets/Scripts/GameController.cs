using Firebase.Auth;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController DefaultInstance;

    public GameObject gameTitle;
    public GameObject loadingPanel;
    public GameObject startPanel;
    public GameObject registrationPanel;
    public GameObject loginPanel;
    public GameObject gamePanel;
    public GameObject creditsPanel;
    public Text errorText;
    public Text consoleText;
    public MapManager map;

    public GameObject eventPanel;
    public Text eventTitle;
    public Text eventText;

    public bool loadingInProgress = true;
    public UnityEvent loadingDone;

    private MonsterManager _monsterManager;
    private CharacterCreation _characterCreation;

    private void Start()
    {
        if (DefaultInstance == null)
        {
            DefaultInstance = this;
        }

        Log("GameStarted");
        map.gameObject.SetActive(true);
        GetComponent<PlayerEventManager>().enabled = true;

        _monsterManager = GetComponent<MonsterManager>();
        _characterCreation = GetComponent<CharacterCreation>();
        FirebaseAuth.DefaultInstance.StateChanged += HandleAuthStateChanged;
        //Credential credential = GoogleAuthProvider.GetCredential("232085783098-mi6nvuqi2ah67914vsigmkblpjaeld2k.apps.googleusercontent.com", "XvhasMevL5dbMx5pxu01NQJU");
        //FirebaseAuth.DefaultInstance.SignInWithCredentialAsync(credential).ContinueWith(task => { 
        //    if(task.IsCanceled)
        //    {
        //        Debug.LogError("SignInWithCredentialAsync was canceled.");
        //        return;
        //    }
        //    if(task.IsFaulted)
        //    {
        //        Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
        //        return;
        //    }

        //    FirebaseUser newUser = task.Result;
        //    Debug.LogFormat("User signed in successfully: {0} ({1})", newUser.DisplayName, newUser.UserId);
        //});
    }

    private void OnDestroy()
    {
        FirebaseAuth.DefaultInstance.StateChanged -= HandleAuthStateChanged;
    }

    private void HandleAuthStateChanged(object sender, EventArgs e)
    {
        CheckUser();
    }

    private void CheckUser()
    {
        if(FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            Log("Player connected!");
            loadingDone.AddListener(GoToMainScene);
            StartCoroutine("Loading");
        }
        else
        {
            Log("Player not connected!");
            loadingDone.AddListener(GoToStartingScene);
            StartCoroutine("Loading");
        }
    }

    public void GoToRegistrationScene()
    {
        gameTitle.SetActive(true);
        startPanel.SetActive(false);
        registrationPanel.SetActive(true);
    }

    public void GoToLoginScene()
    {
        gameTitle.SetActive(true);
        startPanel.SetActive(false);
        loginPanel.SetActive(true);
    }

    public void GoToStartingScene()
    {
        gameTitle.SetActive(true);
        loadingPanel.SetActive(false);
        creditsPanel.SetActive(false);
        registrationPanel.SetActive(false);
        loginPanel.SetActive(false);
        HideErrorPanel();
        startPanel.SetActive(true);
    }

    public void GoToMainScene()
    {
        gameTitle.SetActive(false);
        loadingPanel.SetActive(false);
        startPanel.SetActive(false);
        registrationPanel.SetActive(false);
        loginPanel.SetActive(false);
        _characterCreation.enabled = true;
        _monsterManager.enabled = true;
    }

    public void GoToCredits()
    {
        gameTitle.SetActive(true);
        startPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void StopLoading()
    {
        PlayerSaveManager psm = GetComponent<PlayerSaveManager>();
        psm.enabled = true;
        loadingInProgress = false;
    }

    public IEnumerator Loading()
    {
        yield return new WaitUntil(() => { return !loadingInProgress; });
        loadingDone.Invoke();
    }

    public void HideErrorPanel()
    {
        errorText.transform.parent.gameObject.SetActive(false);
    }

    public void Error(AggregateException error)
    {
        UIGameController.DefaultInstance.Logout();

        gameTitle.SetActive(false);
        loadingPanel.SetActive(false);
        startPanel.SetActive(false);
        registrationPanel.SetActive(false);
        loginPanel.SetActive(false);
        gamePanel.SetActive(false);

        Exception e = error.GetBaseException();
        errorText.text = e.Message;
        errorText.transform.parent.gameObject.SetActive(true);
    }

    public void Log(string text)
    {
        Debug.Log(text);
        consoleText.text = text;
    }

    public void DisplayEvent(PlayerEvent pEvent)
    {
        switch (pEvent.playerEventType)
        {
            case PlayerEventType.Killed:
                //Panneau killed
                eventPanel.GetComponentInChildren<Text>().text = pEvent.message;
                eventPanel.SetActive(true);
                break;
            default:
                Debug.Log("Not implmented event!");
                break;
        }
    }
}
