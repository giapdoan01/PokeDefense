using Firebase.Auth;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Extensions;
using UnityEngine.SceneManagement;

public class FirebaseAuthManager : MonoBehaviour
{
    [Header("Register")]
    public GameObject registerPanel;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;
    public Button registerButton;
    public Button goToLoginButton;

    [Header("Login")]
    public GameObject loginPanel;
    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPasswordInput;
    public Button loginButton;
    public Button goToRegisterButton;
    private FirebaseAuth auth;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        registerButton.onClick.AddListener(RegisterAccount);
        goToLoginButton.onClick.AddListener(GoToLoginPanel);
        loginButton.onClick.AddListener(LoginAccount);
        goToRegisterButton.onClick.AddListener(GoToRegisterPanel);
        registerPanel.SetActive(false);
        loginPanel.SetActive(true);
    }

    public void RegisterAccount()
    {
        string email = emailInput.text;
        string password = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            Debug.Log("Please fill in all fields.");
            return;
        }
        if (password != confirmPassword)
        {
            Debug.Log("Passwords do not match.");
            return;
        }
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }
            if (task.IsCompleted)
            {
                Debug.LogFormat("Firebase user created successfully");
            }
        });
    }

    public void GoToRegisterPanel()
    {
        registerPanel.SetActive(true);
        loginPanel.SetActive(false);
    }
    public void GoToLoginPanel()
    {
        registerPanel.SetActive(false);
        loginPanel.SetActive(true);
    }
    

    public void LoginAccount()
    {
        string email = loginEmailInput.text;
        string password = loginPasswordInput.text;
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.Log("Please fill in all fields.");
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }
            if (task.IsCompleted)
            {
                Debug.LogFormat("User signed in successfully");
                SceneManager.LoadScene("HomePage");
            }
        });
    }

}