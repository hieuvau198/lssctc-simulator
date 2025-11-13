using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class ApiService : MonoBehaviour
{
    public static ApiService Instance { get; private set; }

    private static readonly HttpClient httpClient = new HttpClient();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void SetAuthorizationToken(string token)
    {
        httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    // helper to GET raw string
    private async Task<string> GetRawAsync(string url)
    {
        try
        {
            var res = await httpClient.GetAsync(url);
            var txt = await res.Content.ReadAsStringAsync();
            if (!res.IsSuccessStatusCode)
            {
                Debug.LogError($"GET {url} failed: {res.StatusCode}\n{txt}");
            }
            return txt;
        }
        catch (Exception ex)
        {
            Debug.LogError($"GET Exception {url}: {ex.Message}");
            return null;
        }
    }

    // helper to POST with body and get raw string
    private async Task<string> PostRawAsync(string url, object body = null)
    {
        try
        {
            StringContent content = null;
            if (body != null)
            {
                content = new StringContent(JsonUtility.ToJson(body), Encoding.UTF8, "application/json");
            }
            else
            {
                // some endpoints accept empty body (original code used PostWwwForm with empty string)
                content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            }

            var res = await httpClient.PostAsync(url, content);
            var txt = await res.Content.ReadAsStringAsync();
            if (!res.IsSuccessStatusCode)
            {
                Debug.LogError($"POST {url} failed: {res.StatusCode}\n{txt}");
            }
            return txt;
        }
        catch (Exception ex)
        {
            Debug.LogError($"POST Exception {url}: {ex.Message}");
            return null;
        }
    }

    // helper to PUT (used for complete attempt)
    private async Task<string> PutRawAsync(string url, object body = null)
    {
        try
        {
            HttpContent content = null;
            if (body != null)
                content = new StringContent(JsonUtility.ToJson(body), Encoding.UTF8, "application/json");
            else
                content = new StringContent(string.Empty, Encoding.UTF8, "application/json");

            var res = await httpClient.PutAsync(url, content);
            var txt = await res.Content.ReadAsStringAsync();
            if (!res.IsSuccessStatusCode)
            {
                Debug.LogError($"PUT {url} failed: {res.StatusCode}\n{txt}");
            }
            return txt;
        }
        catch (Exception ex)
        {
            Debug.LogError($"PUT Exception {url}: {ex.Message}");
            return null;
        }
    }

    #region API
    public async Task<LoginResponse> LoginAsync(string username, string password)
    {
        string url = ApiConfig.Login();

        var body = new LoginRequest
        {
            username = username,
            password = password
        };

        try
        {
            var json = JsonUtility.ToJson(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(url, content);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Debug.LogError($"Login failed: {response.StatusCode}\n{responseText}");
                return null;
            }

            // Parse JSON into LoginResponse
            var result = JsonUtility.FromJson<LoginResponse>(responseText);

            // Optional: store token for later requests
            if (result != null && !string.IsNullOrEmpty(result.accessToken))
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.accessToken);

                PlayerPrefs.SetString("AccessToken", result.accessToken);
                PlayerPrefs.SetString("Username", result.userName);
                PlayerPrefs.SetInt("ExpiresIn", result.expiresIn);
            }

            return result;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Login exception: {ex.Message}");
            return null;
        }
    }
    //Classes for User
    public async Task<List<ClassDto>> GetClassesForUserAsync(int userId)
    {
        string raw = await GetRawAsync(ApiConfig.ClassesForUser(userId));
        if (string.IsNullOrEmpty(raw)) return null;

        // Parse as List<ClassDto>
        ClassDto[] classes = JsonUtility.FromJson<ClassDtoArrayWrapper>("{\"items\":" + raw + "}").items;
        return new List<ClassDto>(classes);
    }
    [Serializable]
    private class ClassDtoArrayWrapper
    {
        public ClassDto[] items;
    }
    //Practices for Class
    public async Task<List<PracticeDto>> GetPracticesForClassAsync(int classId)
    {
        string raw = await GetRawAsync(ApiConfig.PracticesForClass(classId));
        if (string.IsNullOrEmpty(raw)) return null;
        PracticeDto[] practices = JsonUtility.FromJson<PracticeDtoArrayWrapper>("{\"items\":" + raw + "}").items;
        return new List<PracticeDto>(practices);
    }
    [Serializable]
    private class PracticeDtoArrayWrapper 
    { 
        public PracticeDto[] items; 
    }
    //Finish Practice attempt
    public async Task<string> CompletePracticeAttemptAsync(PracticeAttemptCompleteDto attempt)
    {
        try
        {
            var json = JsonUtility.ToJson(attempt);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var res = await httpClient.PostAsync(ApiConfig.PracticeAttemptComplete(), content);
            var txt = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
            {
                Debug.LogError($"POST PracticeAttemptComplete failed: {res.StatusCode}\n{txt}");
                return null;
            }

            return txt;
        }
        catch (Exception ex)
        {
            Debug.LogError($"POST PracticeAttemptComplete Exception: {ex.Message}");
            return null;
        }
    }


    public async Task<ComponentDto> GetComponentByIdAsync(int id)
    {
        string raw = await GetRawAsync(ApiConfig.ComponentById(id));
        if (string.IsNullOrEmpty(raw)) return null;
        return JsonUtility.FromJson<ComponentDto>(raw);
    }
    #endregion

}
