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
    public async Task<List<ClassDto>> GetClassesForUserAsync()
    {
        string raw = await GetRawAsync(ApiConfig.ClassesForUser());
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
        PracticeDtoArrayWrapper wrapper = JsonUtility.FromJson<PracticeDtoArrayWrapper>(raw);
        PracticeDto[] practices = wrapper.data;
        return new List<PracticeDto>(practices);
    }
    [Serializable]
    private class PracticeDtoArrayWrapper
    {
        public PracticeDto[] data;
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


    public async Task<ComponentDto> GetComponentByCodeAsync(string code)
    {
        string raw = await GetRawAsync(ApiConfig.ComponentByCode(code));
        if (string.IsNullOrEmpty(raw)) return null;
        return JsonUtility.FromJson<ComponentDto>(raw);
    }
    [Serializable]
    private class SePracticeListArrayWrapper
    {
        public SePracticeListDto[] items;
    }
    public async Task<List<SePracticeListDto>> GetFinalExamSePracticesAsync(int classId)
    {
        string url = ApiConfig.FinalExamSePractices(classId);
        string raw = await GetRawAsync(url);

        if (string.IsNullOrEmpty(raw) || raw == "[]") return new List<SePracticeListDto>();

        

        try
        {
            
            var wrappedJson = "{\"items\":" + raw + "}";
            SePracticeListDto[] practices = JsonUtility.FromJson<SePracticeListArrayWrapper>(wrappedJson).items;
            return new List<SePracticeListDto>(practices);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to parse SE Practice List from API: {ex.Message}\nRaw: {raw}");
            return null;
        }
    }

    // 2. POST check the available Exam code when start the practice in the simulation
    public async Task<object> ValidateFinalExamSeCodeAsync(int partialId, string examCode)
    {
        string url = ApiConfig.FinalExamSeValidateCode(partialId);
        var body = new ValidateExamCodeDto { examCode = examCode };

        string raw = await PostRawAsync(url, body);

        if (string.IsNullOrEmpty(raw)) return null;
        if (raw.Contains("\"message\""))
        {
            try
            {
                var err = JsonUtility.FromJson<ErrorMessageDto>(raw);
                return err;
            }
            catch
            {
                return new ErrorMessageDto { message = "Unknown error" };
            }
        }
        try
        {
            return JsonUtility.FromJson<FinalExamPartialDtoResponse>(raw);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to parse Validate SE Code response: {ex.Message}\nRaw: {raw}");
            return null;
        }
    }

    // 3. POST submit the final exam SE
    public async Task<string> SubmitFinalExamSeAsync(int partialId, SubmitSeFinalDto submission)
    {
        string url = ApiConfig.FinalExamSeSubmit(partialId);

        
        string raw = await PostRawAsync(url, submission);

        
        if (string.IsNullOrEmpty(raw)) return null;

        
        return raw;
    }
    #endregion

}
