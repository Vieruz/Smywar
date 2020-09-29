using UnityEngine;
using Firebase;
using Firebase.Analytics;
using UnityEngine.Events;

public class FirebaseInit : MonoBehaviour
{
    public static FirebaseInit _instance;
    public static FirebaseApp _app;
    public UnityEvent OnFirebaseInitialized = new UnityEvent();

    private async void Awake()
    {
        OnFirebaseInitialized.AddListener(EnableGame);

        if(_instance == null)
        {
            _instance = this;
            var dependencyResult = await FirebaseApp.CheckAndFixDependenciesAsync();
            if(dependencyResult == DependencyStatus.Available)
            {
                _app = FirebaseApp.DefaultInstance;
                OnFirebaseInitialized.Invoke();
            }
            else
            {
                Debug.LogError($"Failed to initialize Firebase with {dependencyResult}");
            }
        }
    }

    void EnableGame()
    {
        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
        GameObject gc = GameObject.FindGameObjectWithTag("GameController");
        gc.GetComponent<GameController>().enabled = true;
    }
}
