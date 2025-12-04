using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using Unity.VisualScripting.Antlr3.Runtime;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TextMeshProUGUI errorText;
    public Button loginButton;
    public Button exitButton;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            GameObject current = EventSystem.current.currentSelectedGameObject;

            if (current == usernameInput.gameObject)
            {
                passwordInput.Select();
                passwordInput.ActivateInputField();
            }
            else if (current == passwordInput.gameObject)
            {
                loginButton.Select();
            }
        }
    }
    public async void OnLoginButtonClicked()
    {
        errorText.text = "";
        string username = usernameInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            errorText.text = "Vui lòng nhập tên đăng nhập và mật khẩu.";
            return;
        }

        // Show loading message (optional)
        errorText.color = Color.white;
        errorText.text = "Đang đăng nhập...";

        var loginResult = await ApiService.Instance.LoginAsync(username, password);

        if (loginResult != null && !string.IsNullOrEmpty(loginResult.accessToken))
        {
            Debug.Log($"Login success for {loginResult.userName}");
            ApiService.Instance.SetAuthorizationToken(loginResult.accessToken);
            PlayerPrefs.SetString("Username", loginResult.userName);
            PlayerPrefs.Save();
            errorText.text = "";
            this.enabled = false;
            SceneManager.LoadScene("PracticeListScene");
        }
        else
        {
            errorText.color = Color.red;
            errorText.text = "Đăng nhập thất bại. Vui lòng kiểm tra tên đăng nhập hoặc mật khẩu.";
        }
    }
    public void OnExitButtonClicked()
    {
        Debug.Log("Exiting application...");
        #if UNITY_EDITOR
                // Works in the Unity Editor
                UnityEditor.EditorApplication.isPlaying = false;
        #else
            // Works in the built app (closes it completely)
            Application.Quit();
        #endif
    }

}


