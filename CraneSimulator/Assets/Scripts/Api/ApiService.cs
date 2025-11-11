using System;
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
