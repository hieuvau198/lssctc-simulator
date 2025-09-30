using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public class ApiService : MonoBehaviour
{
    private static readonly HttpClient httpClient = new HttpClient();

    public async Task<ComponentDto> GetComponentByIdAsync(int id)
    {
        string url = $"https://lssctc-simulation.azurewebsites.net/api/Components/{id}";
        Debug.Log("Calling API: " + url);

        try
        {
            // 🔥 Make sure no leftover headers are there
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("accept", "text/plain");

            HttpResponseMessage response = await httpClient.GetAsync(url);

            string responseText = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                Debug.Log("API JSON: " + responseText);

                ComponentDto component = JsonUtility.FromJson<ComponentDto>(responseText);
                return component;
            }
            else
            {
                Debug.LogError($"API Error: {(int)response.StatusCode} {response.ReasonPhrase}, Response: {responseText}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Exception: " + ex.Message);
            return null;
        }
    }
}
