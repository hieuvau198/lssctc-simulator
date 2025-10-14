using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public TextMeshProUGUI errorText;

    private string apiUrl = "https://yourapi.com/api/auth/login";

    public void OnLoginButtonClicked()
    {
        PlayerPrefs.SetInt("UserID",1);
        SceneManager.LoadScene("PracticeListScene");
        //StartCoroutine(LoginCoroutine());
    }

    //IEnumerator LoginCoroutine()
    //{
    //    var loginData = new LoginRequest
    //    {
    //        Username = usernameInput.text,
    //        Password = passwordInput.text
    //    };

    //    string jsonData = JsonUtility.ToJson(loginData);

    //    UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
    //    byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(jsonData);
    //    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
    //    request.downloadHandler = new DownloadHandlerBuffer();
    //    request.SetRequestHeader("Content-Type", "application/json");

    //    yield return request.SendWebRequest();

    //    if (request.result == UnityWebRequest.Result.Success)
    //    {
    //        var response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
    //        PlayerPrefs.SetString("jwtToken", response.token); // Save token
    //        UnityEngine.SceneManagement.SceneManager.LoadScene("PracticeListScene");
    //    }
    //    else
    //    {
    //        errorText.text = "Login failed: " + request.error;
    //    }
    //}
}

[System.Serializable]
public class LoginRequest
{
    public string Username;
    public string Password;
}

[System.Serializable]
public class LoginResponse
{
    public string token;
}
