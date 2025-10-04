using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PracticeListManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject practiceCardPrefab;
    public Transform contentPanel;
    public TextMeshProUGUI errorText;

    private string apiUrl = "https://lssctc-simulation.azurewebsites.net/api/Practices";

    void Start()
    {
        StartCoroutine(FetchPractices());
    }

    IEnumerator FetchPractices()
    {
        string token = PlayerPrefs.GetString("jwtToken", "");

        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        request.SetRequestHeader("Authorization", "Bearer " + token);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            PracticeResponse response = JsonUtility.FromJson<PracticeResponse>(json);

            if (response.items == null || response.items.Count == 0)
            {
                errorText.text = "No practices available for this user.";
                yield break;
            }

            foreach (var practice in response.items)
            {
                GameObject card = Instantiate(practiceCardPrefab, contentPanel);

                // find children inside the prefab
                TextMeshProUGUI nameText = card.transform.Find("PracticeNameText").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI descText = card.transform.Find("PracticeDescriptionText").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI durationText = card.transform.Find("PracticeDurationText").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI difficultyText = card.transform.Find("PracticeDifficultyText").GetComponent<TextMeshProUGUI>();
                Button startButton = card.transform.Find("StartButton").GetComponent<Button>();

                nameText.text = practice.practiceName;
                descText.text = practice.practiceDescription;
                durationText.text = $"{practice.estimatedDurationMinutes} min";
                difficultyText.text = practice.difficultyLevel;

                startButton.onClick.AddListener(() =>
                {
                    PlayerPrefs.SetInt("selectedPracticeId", practice.id);
                    PlayerPrefs.SetString("selectedPracticeName", practice.practiceName);
                    PlayerPrefs.SetString("selectedPracticeDescription", practice.practiceDescription);
                    PlayerPrefs.SetString("selectedPracticeDifficulty", practice.difficultyLevel);
                    PlayerPrefs.SetInt("selectedPracticeDuration", practice.estimatedDurationMinutes);
                    UnityEngine.SceneManagement.SceneManager.LoadScene("SimulationScene");
                });
            }
        }
        else
        {
            errorText.text = "Failed to load practices: " + request.error;
            Debug.LogError(request.downloadHandler.text);
        }
    }
}

[System.Serializable]
public class PracticeResponse
{
    public List<PracticeItem> items;
    public int totalCount;
    public int page;
    public int pageSize;
    public int totalPages;
}

[System.Serializable]
public class PracticeItem
{
    public int id;
    public string practiceName;
    public string practiceDescription;
    public int estimatedDurationMinutes;
    public string difficultyLevel;
    public int maxAttempts;
    public string createdDate;
    public bool isActive;
}
