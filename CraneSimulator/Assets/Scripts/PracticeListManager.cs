using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class PracticeListManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject practiceCardPrefab;
    public Transform contentPanel;
    public TextMeshProUGUI errorText;

    [Header("Class List")]
    public TMP_Dropdown classDropdown;

    //api url
    private string traineePracticeApiUrl = "https://lssctc-simulation.azurewebsites.net/api/TraineePractices/";
    private string classApiUrl = "https://lssctc.azurewebsites.net/api/Classes/myclasses/";

    private int userId;
    void Start()
    {
        userId = PlayerPrefs.GetInt("UserID", 0);
        if (userId == 0)
        {
            errorText.text = "User ID not found in PlayerPrefs.";
            return;
        }

        StartCoroutine(FetchClasses(userId));
    }
    #region Class Fetching
    IEnumerator FetchClasses(int userId)
    {
        string apiUrl = classApiUrl + userId;
        //string token = PlayerPrefs.GetString("jwtToken", "");

        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        //if (!string.IsNullOrEmpty(token))
        //    request.SetRequestHeader("Authorization", "Bearer " + token);

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;

            // Since API returns a JSON array, wrap it for JsonUtility
            json = "{\"items\":" + json + "}";

            ClassListResponse response = JsonUtility.FromJson<ClassListResponse>(json);

            if (response.items == null || response.items.Count == 0)
            {
                errorText.text = "No classes found for this user.";
                yield break;
            }

            PopulateDropdown(response.items);
        }
        else
        {
            errorText.text = "Failed to load classes: " + request.error;
            Debug.LogError(request.downloadHandler.text);
        }
    }

    void PopulateDropdown(List<ClassItem> classes)
    {
        classDropdown.ClearOptions();

        List<string> classNames = new List<string>() { "Select a class" };
        foreach (var c in classes)
        {
            classNames.Add(c.className);
        }

        classDropdown.AddOptions(classNames);
        // Reset PlayerPrefs (no class selected yet)
        PlayerPrefs.DeleteKey("SelectedClassId");
        PlayerPrefs.DeleteKey("SelectedClassName");
        PlayerPrefs.DeleteKey("SelectedCourseName");
        PlayerPrefs.Save();
        // Clear the practice list panel
        foreach (Transform child in contentPanel)
            Destroy(child.gameObject);

        errorText.text = "Please select a class.";

        // Add listener for dropdown changes
        classDropdown.onValueChanged.AddListener((index) =>
        {
            // Skip if the first option ("Select a class") is chosen
            if (index == 0)
            {
                errorText.text = "Please select a class.";
                foreach (Transform child in contentPanel)
                    Destroy(child.gameObject);
                return;
            }

            // Get selected class (index - 1 because of default item)
            ClassItem selected = classes[index - 1];
            PlayerPrefs.SetInt("SelectedClassId", selected.classId);
            PlayerPrefs.SetString("SelectedClassName", selected.className);
            PlayerPrefs.SetString("SelectedCourseName", selected.courseName);
            PlayerPrefs.Save();

            errorText.text = "Loading practices...";
            StartCoroutine(FetchPractices(userId, selected.classId));
        });
    }
    #endregion
    IEnumerator FetchPractices(int userId, int classId)
    {
        
        // Clear old cards
        foreach (Transform child in contentPanel)
            Destroy(child.gameObject);

        string apiUrl = $"{traineePracticeApiUrl}trainee/{userId}/class/{classId}";
        //string token = PlayerPrefs.GetString("jwtToken", "");

        UnityWebRequest request = UnityWebRequest.Get(apiUrl);
        //if (!string.IsNullOrEmpty(token))
        //    request.SetRequestHeader("Authorization", "Bearer " + token);
        request.SetRequestHeader("Content-Type", "application/json");
        Debug.Log($"[DEBUG] Fetching practices from: {apiUrl}");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            json = "{\"items\":" + json + "}"; // wrap array for JsonUtility

            TraineePracticeResponse response = JsonUtility.FromJson<TraineePracticeResponse>(json);

            if (response.items == null || response.items.Count == 0)
            {
                errorText.text = "No practices available for this class.";
                yield break;
            }

            errorText.text = ""; // clear error message
            foreach (var practice in response.items)
            {
                GameObject card = Instantiate(practiceCardPrefab, contentPanel);

                // Find UI elements inside prefab
                TextMeshProUGUI nameText = card.transform.Find("PracticeNameText").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI descText = card.transform.Find("PracticeDescriptionText").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI durationText = card.transform.Find("PracticeDurationText").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI difficultyText = card.transform.Find("PracticeDifficultyText").GetComponent<TextMeshProUGUI>();
                Button startButton = card.transform.Find("StartButton").GetComponent<Button>();

                // Assign data
                nameText.text = practice.practiceName;
                descText.text = practice.practiceDescription;
                durationText.text = $"{practice.estimatedDurationMinutes} min";
                difficultyText.text = practice.difficultyLevel;

                startButton.onClick.AddListener(() =>
                {
                    PlayerPrefs.SetInt("selectedPracticeId", practice.practiceId);
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
    //IEnumerator FetchPractices()
    //{
    //    string token = PlayerPrefs.GetString("jwtToken", "");

    //    UnityWebRequest request = UnityWebRequest.Get(traineePracticeApiUrl);
    //    request.SetRequestHeader("Authorization", "Bearer " + token);
    //    request.SetRequestHeader("Content-Type", "application/json");

    //    yield return request.SendWebRequest();

    //    if (request.result == UnityWebRequest.Result.Success)
    //    {
    //        string json = request.downloadHandler.text;
    //        PracticeResponse response = JsonUtility.FromJson<PracticeResponse>(json);

    //        if (response.items == null || response.items.Count == 0)
    //        {
    //            errorText.text = "No practices available for this user.";
    //            yield break;
    //        }

    //        foreach (var practice in response.items)
    //        {
    //            GameObject card = Instantiate(practiceCardPrefab, contentPanel);

    //            // find children inside the prefab
    //            TextMeshProUGUI nameText = card.transform.Find("PracticeNameText").GetComponent<TextMeshProUGUI>();
    //            TextMeshProUGUI descText = card.transform.Find("PracticeDescriptionText").GetComponent<TextMeshProUGUI>();
    //            TextMeshProUGUI durationText = card.transform.Find("PracticeDurationText").GetComponent<TextMeshProUGUI>();
    //            TextMeshProUGUI difficultyText = card.transform.Find("PracticeDifficultyText").GetComponent<TextMeshProUGUI>();
    //            Button startButton = card.transform.Find("StartButton").GetComponent<Button>();

    //            nameText.text = practice.practiceName;
    //            descText.text = practice.practiceDescription;
    //            durationText.text = $"{practice.estimatedDurationMinutes} min";
    //            difficultyText.text = practice.difficultyLevel;

    //            startButton.onClick.AddListener(() =>
    //            {
    //                PlayerPrefs.SetInt("selectedPracticeId", practice.id);
    //                PlayerPrefs.SetString("selectedPracticeName", practice.practiceName);
    //                PlayerPrefs.SetString("selectedPracticeDescription", practice.practiceDescription);
    //                PlayerPrefs.SetString("selectedPracticeDifficulty", practice.difficultyLevel);
    //                PlayerPrefs.SetInt("selectedPracticeDuration", practice.estimatedDurationMinutes);
    //                UnityEngine.SceneManagement.SceneManager.LoadScene("SimulationScene");
    //            });
    //        }
    //    }
    //    else
    //    {
    //        errorText.text = "Failed to load practices: " + request.error;
    //        Debug.LogError(request.downloadHandler.text);
    //    }
    //}

}

//Practice
[System.Serializable]
public class TraineePracticeResponse
{
    public List<TraineePracticeItem> items;
}

[System.Serializable]
public class TraineePracticeItem
{
    public int sectionPracticeId;
    public int partitionId;
    public int practiceId;
    public string customDeadline;
    public string status;
    public bool isCompleted;
    public string practiceName;
    public string practiceDescription;
    public int estimatedDurationMinutes;
    public string difficultyLevel;
}


//Class
[System.Serializable]
public class ClassListResponse
{
    public List<ClassItem> items;
}

[System.Serializable]
public class ClassItem
{
    public int courseId;
    public string courseName;
    public string courseCode;
    public int durationHours;
    public string imageUrl;
    public int classId;
    public string className;
    public string classCode;
    public int status;
    public int instructorId;
    public string instructorName;
    public string startDate;
    public string endDate;
}