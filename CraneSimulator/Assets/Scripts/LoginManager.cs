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

    [Header("Mode Select Popup")]
    public GameObject modeSelectPopup;
    public GameObject popupBlocker;
    public Button trainingButton;          
    public Button finalExamButton;

    private bool popupActive = false;
    
    void Awake()
    {
        var managers = FindObjectsOfType<LoginManager>();
        if (managers.Length > 1)
        {
            Destroy(gameObject);
            return;
        }
    }
    void Start()
    {
        popupActive = false;

        Assign();

        if (popupBlocker != null)
            popupBlocker.SetActive(false);

        if (modeSelectPopup != null)
            modeSelectPopup.SetActive(false);

        if (trainingButton != null)
        {
            trainingButton.onClick.RemoveAllListeners(); 
            trainingButton.onClick.AddListener(OnTrainingModeSelected);
        }

        if (finalExamButton != null)
        {
            finalExamButton.onClick.RemoveAllListeners();
            finalExamButton.onClick.AddListener(OnFinalExamModeSelected);
        }

        if (loginButton != null)
        {
            loginButton.onClick.RemoveAllListeners();
            loginButton.onClick.AddListener(OnLoginButtonClicked);
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(OnExitButtonClicked);
        }
    }
    void Update()
    {
        if (popupActive) return;
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            OnLoginButtonClicked();
            return;
        }
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
        if (popupActive) return;
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
            popupActive = true;               
            popupBlocker.SetActive(true);
            modeSelectPopup.SetActive(true);
        }
        else
        {
            errorText.color = Color.red;
            errorText.text = "Đăng nhập thất bại. Vui lòng kiểm tra tên đăng nhập hoặc mật khẩu.";
        }
    }
    void OnTrainingModeSelected()
    {
        PlayerPrefs.SetInt("IsFinalExam", 0);
        PlayerPrefs.Save();
        popupActive = false;
        popupBlocker.SetActive(false);
        SceneManager.LoadScene("PracticeListScene");
    }

    void OnFinalExamModeSelected()
    {
        PlayerPrefs.SetInt("IsFinalExam", 1);
        PlayerPrefs.Save();
        popupActive = false;
        popupBlocker.SetActive(false);
        SceneManager.LoadScene("PracticeListScene");
    }
    public void OnExitButtonClicked()
    {
        if (popupActive) return;
        Debug.Log("Exiting application...");
        #if UNITY_EDITOR
                // Works in the Unity Editor
                UnityEditor.EditorApplication.isPlaying = false;
        #else
            // Works in the built app (closes it completely)
            Application.Quit();
        #endif
    }
    public void Assign()
    {
        usernameInput = GameObject.Find("Canvas/Login/username")?.GetComponent<TMP_InputField>();
        passwordInput  = GameObject.Find("Canvas/Login/password")?.GetComponent<TMP_InputField>();
        errorText = GameObject.Find("Canvas/Login/Error")?.GetComponent<TextMeshProUGUI>();
        loginButton = GameObject.Find("Canvas/Login/Button/BtnLogin")?.GetComponent<Button>();
        exitButton = GameObject.Find("Canvas/Login/Button/BtnExit")?.GetComponent<Button>();
        modeSelectPopup = GameObject.Find("Canvas/ModeSelectPopup")?.gameObject;
        popupBlocker = GameObject.Find("Canvas/PopupBlocker")?.gameObject;
        trainingButton = GameObject.Find("Canvas/ModeSelectPopup/Button/Button_ModeTraining")?.GetComponent<Button>();
        finalExamButton = GameObject.Find("Canvas/ModeSelectPopup/Button/Button_ModeExam")?.GetComponent<Button>();
}

}


