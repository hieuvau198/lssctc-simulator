using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static SelectionManager;

/// <summary>
/// Centralized API service. Put this on an object in your initial scene (e.g. "ApiService" GameObject).
/// Uses HttpClient for async calls. All methods return Task<T>.
/// </summary>
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

    // ---------- Specific API wrappers (use the same DTO names you had) ----------

    public async Task<ClassListResponse> GetMyClassesAsync(int userId)
    {
        string raw = await GetRawAsync(ApiConfig.MyClasses(userId));
        if (string.IsNullOrEmpty(raw)) return null;
        // original code wrapped array into { "items": [...] }
        string wrapped = "{\"items\":" + raw + "}";
        return JsonUtility.FromJson<ClassListResponse>(wrapped);
    }

    public async Task<TraineePracticeResponse> GetTraineePracticesAsync(int userId, int classId)
    {
        string raw = await GetRawAsync(ApiConfig.TraineePractices(userId, classId));
        if (string.IsNullOrEmpty(raw)) return null;
        string wrapped = "{\"items\":" + raw + "}";
        return JsonUtility.FromJson<TraineePracticeResponse>(wrapped);
    }

    public async Task<PracticeAttemptResponse> CreatePracticeAttemptAsync(int sectionPracticeId, int userId)
    {
        string raw = await PostRawAsync(ApiConfig.CreatePracticeAttempt(sectionPracticeId, userId), null);
        if (string.IsNullOrEmpty(raw)) return null;
        return JsonUtility.FromJson<PracticeAttemptResponse>(raw);
    }

    public async Task<TraineePracticeStep[]> GetPracticeAttemptStepsAsync(int attemptId)
    {
        string raw = await GetRawAsync(ApiConfig.PracticeAttemptSteps(attemptId));
        if (string.IsNullOrEmpty(raw)) return null;
        // original code used JsonHelper.FromJson<TraineePracticeStep>(json) — we provide compatibility
        return JsonHelper.FromJsonArray<TraineePracticeStep>(raw);
    }

    /// <summary>
    /// Submits a step (the original route you used inside SelectionManager)
    /// </summary>
    public async Task<string> SubmitStepAsync(int attemptId, int userId, SubmitStepData dto)
    {
        // original used: https://.../attempt/{attemptId}/trainee/{userId}/submit
        string url = ApiConfig.SubmitStep(attemptId, userId);
        string raw = await PostRawAsync(url, dto);
        return raw; // raw contains response JSON (caller can check message string)
    }

    public async Task<string> CompleteAttemptAsync(int attemptId)
    {
        string url = ApiConfig.CompleteAttempt(attemptId);
        string raw = await PutRawAsync(url, null);
        return raw;
    }

    public async Task<ComponentDto> GetComponentByIdAsync(int id)
    {
        string raw = await GetRawAsync(ApiConfig.ComponentById(id));
        if (string.IsNullOrEmpty(raw)) return null;
        return JsonUtility.FromJson<ComponentDto>(raw);
    }
}
