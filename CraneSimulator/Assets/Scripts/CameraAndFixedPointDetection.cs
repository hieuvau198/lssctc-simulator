using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft;
using Newtonsoft.Json;

public class CameraAndFixedPointDetection : MonoBehaviour
{
    public Camera playerCamera;
    public GameObject fixedPointPrefab;
    public TextMeshProUGUI discoveredText;
    public string targetObjectName = "Huyndai";
    public string apiUrl = "https://lssctc-simulation.azurewebsites.net/api/Components?page=1&pageSize=10";

    private GameObject fixedPoint;
    private List<ComponentData> componentList = new List<ComponentData>();
    private string currentDescription = "";

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;

        if (fixedPointPrefab != null)
            fixedPoint = Instantiate(fixedPointPrefab, playerCamera.transform.position, Quaternion.identity);
        else
            Debug.LogError("No fixed point prefab assigned!");

        if (discoveredText != null) discoveredText.enabled = false;

        StartCoroutine(GetComponentsFromApi());
    }

    private IEnumerator GetComponentsFromApi()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(apiUrl))
        {
            www.SetRequestHeader("accept", "application/json"); // Set the accept header to JSON
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = www.downloadHandler.text;

                // Log the entire response to see what the API is returning
                Debug.Log("API Response: " + jsonResponse);

                // Parse using Newtonsoft.Json
                try
                {
                    var response = JsonConvert.DeserializeObject<ComponentListResponse>(jsonResponse);
                    if (response != null)
                    {
                        componentList = response.items ?? new List<ComponentData>();
                        Debug.Log("Total components fetched: " + componentList.Count); // Debugging the fetched list
                        foreach (var component in componentList)
                        {
                            Debug.Log("Component name: " + component.name); // Debugging each component name
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("Failed to parse API response: " + ex.Message);
                }
            }
            else
            {
                Debug.LogError("Error fetching data from API: " + www.error);
            }
        }
    }


    void Update()
    {
        if (fixedPoint != null)
        {
            Vector3 worldPos = playerCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));
            fixedPoint.transform.position = worldPos;
        }

        Ray ray = playerCamera.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Debugging: Log the name of the object hit
            Debug.Log("Raycast hit: " + hit.collider.gameObject.name);

            bool isTargetFound = false;
            foreach (var component in componentList)
            {
                // Check if the object name matches the component name
                if (string.Equals(hit.collider.gameObject.name.Trim(),
                                  component.name.Trim(),
                                  System.StringComparison.OrdinalIgnoreCase))
                {
                    isTargetFound = true;
                    currentDescription = component.description; // Set the description when found
                    Debug.Log("Description found: " + currentDescription); // Debugging the description
                    break;
                }
            }

            if (isTargetFound)
            {
                discoveredText.enabled = true;
                discoveredText.text = currentDescription; // Display the description
            }
            else
            {
                discoveredText.enabled = false;
            }
        }
        else
        {
            discoveredText.enabled = false;
        }
    }

}

[System.Serializable]
public class ComponentData
{
    public int id;
    public string name;
    public string description;
    public string imageUrl;
    public bool isActive;
    public string createdDate;
}

[System.Serializable]
public class ComponentListResponse
{
    public List<ComponentData> items;
    public int totalCount;
    public int page;
    public int pageSize;
    public int totalPages;
}
