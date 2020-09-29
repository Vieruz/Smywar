using Firebase.Auth;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LoginButton : MonoBehaviour
{
    public InputField emailField;
    public InputField passwordField;
    public Button loginButton;

    public LoginSucceededEvent OnLoginSucceeded;
    public LoginFailedEvent OnLoginFailed;

    private Coroutine _loginCoroutine;

    private void Start()
    {
        emailField.onValueChanged.AddListener(HandleValueChanged);
        passwordField.onValueChanged.AddListener(HandleValueChanged);
        UpdateButtonInteractable();
    }

    private void HandleValueChanged(string _)
    {
        UpdateButtonInteractable();
    }

    public void HandleButtonClicked()
    {
        if(_loginCoroutine == null)
        {
            _loginCoroutine = StartCoroutine(LoginCoroutine(emailField.text, passwordField.text));
        }
    }

    private IEnumerator LoginCoroutine(string email, string password)
    {
        var auth = FirebaseAuth.DefaultInstance;
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => loginTask.IsCompleted);

        if(loginTask.Exception != null)
        {
            Debug.LogWarning($"Login failed with {loginTask.Exception}");
            OnLoginFailed.Invoke(loginTask.Exception);
        }
        else
        {
            Debug.Log($"Login succeeded with {loginTask.Result.Email}");
            OnLoginSucceeded.Invoke(loginTask.Result);
        }

        _loginCoroutine = null;
        UpdateButtonInteractable();

    }

    private void UpdateButtonInteractable()
    {
        loginButton.interactable =
            _loginCoroutine == null
            && !string.IsNullOrEmpty(emailField.text)
            && !string.IsNullOrEmpty(passwordField.text);
    }

    [Serializable]
    public class LoginSucceededEvent : UnityEvent<FirebaseUser>
    {

    }

    [Serializable]
    public class LoginFailedEvent : UnityEvent<AggregateException>
    {

    }
}
