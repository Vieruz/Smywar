using Firebase.Auth;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RegistrationManager : MonoBehaviour
{
    public GameController gameController;

    public InputField emailField;
    public InputField passwordField;
    public InputField verifyPasswordField;

    public State CurrentState { get; private set; }

    public string Email => emailField.text;
    public string Password => passwordField.text;

    public Button registrationButton;
    private Coroutine _registrationCoroutine;

    private void Start()
    {
        emailField.onValueChanged.AddListener(HandleValueChanged);
        passwordField.onValueChanged.AddListener(HandleValueChanged);
        verifyPasswordField.onValueChanged.AddListener(HandleValueChanged);
        ComputeState();
        UpdateInteractable();
    }

    public void GoToStartingScene()
    {
        GameController.DefaultInstance.GoToStartingScene();
    }

    public void GoToMainScene()
    {
        GameController.DefaultInstance.GoToMainScene();
    }

    private void HandleValueChanged(string _)
    {
        ComputeState();
        HandleRegistrationStateChanged(CurrentState);
    }

    private void ComputeState()
    {
        if (string.IsNullOrEmpty(emailField.text))
        {
            SetState(State.EnterEmail);
        }
        else if (string.IsNullOrEmpty(passwordField.text))
        {
            SetState(State.EnterPassword);
        }
        else if (passwordField.text != verifyPasswordField.text)
        {
            SetState(State.PasswordsDontMatch);
        }
        else
        {
            SetState(State.Ok);
        }
    }

    private void SetState(State state)
    {
        CurrentState = state;
    }

    public enum State
    {
        EnterEmail,
        EnterPassword,
        PasswordsDontMatch,
        Ok
    }

    private void UpdateInteractable()
    {
        registrationButton.interactable =
            CurrentState == State.Ok
            && _registrationCoroutine == null;
    }

    public void HandleRegistrationStateChanged(State registrationState)
    {
        UpdateInteractable();
    }

    public void HandleRegistrationButtonClicked()
    {
        _registrationCoroutine = StartCoroutine(RegisterUser(Email, Password));
        UpdateInteractable();
    }

    private IEnumerator RegisterUser(string email, string password)
    {
        var auth = FirebaseAuth.DefaultInstance;
        var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => registerTask.IsCompleted);

        if(registerTask.Exception != null)
        {
            Debug.LogWarning($"Failed to register task with {registerTask.Exception}");
            gameController.Error(registerTask.Exception);
        }
        else
        {
            Debug.Log($"Successfully registered user {registerTask.Result.Email}");
            GoToMainScene();
        }

        _registrationCoroutine = null;
        UpdateInteractable();        
    }
}
