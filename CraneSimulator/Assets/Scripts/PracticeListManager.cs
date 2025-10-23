using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Threading.Tasks;

public class PracticeListManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject practiceCardPrefab;
    public Transform contentPanel;
    public TextMeshProUGUI errorText;
    public TMP_Dropdown classDropdown;

    private int userId;
    private List<ClassItem> classesCache;

    async void Start()
    {
        userId = PlayerPrefs.GetInt("UserID", 0);
        if (userId == 0)
        {
            errorText.text = "User ID not found in PlayerPrefs.";
            return;
        }

        await LoadClasses();
    }

    private async Task LoadClasses()
    {
        var res = await ApiService.Instance.GetMyClassesAsync(userId);
        if (res == null || res.items == null || res.items.Count == 0)
        {
            errorText.text = "No classes found for this user.";
            return;
        }

        classesCache = res.items;
        PopulateDropdown(classesCache);
    }

    void PopulateDropdown(List<ClassItem> classes)
    {
        classDropdown.ClearOptions();

        List<string> classNames = new List<string>() { "Select a class" };
        foreach (var c in classes)
            classNames.Add(c.className);

        classDropdown.AddOptions(classNames);

        // Reset PlayerPrefs
        PlayerPrefs.DeleteKey("SelectedClassId");
        PlayerPrefs.DeleteKey("SelectedClassName");
        PlayerPrefs.DeleteKey("SelectedCourseName");
        PlayerPrefs.Save();

        foreach (Transform child in contentPanel)
            Destroy(child.gameObject);

        errorText.text = "Please select a class.";

        classDropdown.onValueChanged.RemoveAllListeners();
        classDropdown.onValueChanged.AddListener((index) =>
        {
            if (index == 0)
            {
                errorText.text = "Please select a class.";
                foreach (Transform child in contentPanel)
                    Destroy(child.gameObject);
                return;
            }

            ClassItem selected = classes[index - 1];
            PlayerPrefs.SetInt("SelectedClassId", selected.classId);
            PlayerPrefs.SetString("SelectedClassName", selected.className);
            PlayerPrefs.SetString("SelectedCourseName", selected.courseName);
            PlayerPrefs.Save();

            _ = LoadPractices(selected.classId);
        });
    }

    private async Task LoadPractices(int classId)
    {
        // clear old
        foreach (Transform child in contentPanel)
            Destroy(child.gameObject);

        errorText.text = "Loading practices...";
        var res = await ApiService.Instance.GetTraineePracticesAsync(userId, classId);
        if (res == null || res.items == null || res.items.Count == 0)
        {
            errorText.text = "No practices available for this class.";
            return;
        }

        errorText.text = "";
        foreach (var practice in res.items)
        {
            GameObject card = Instantiate(practiceCardPrefab, contentPanel);

            TextMeshProUGUI nameText = card.transform.Find("PracticeNameText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI descText = card.transform.Find("PracticeDescriptionText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI durationText = card.transform.Find("PracticeDurationText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI difficultyText = card.transform.Find("PracticeDifficultyText").GetComponent<TextMeshProUGUI>();
            UnityEngine.UI.Button startButton = card.transform.Find("StartButton").GetComponent<UnityEngine.UI.Button>();

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
                PlayerPrefs.Save();

                _ = StartPracticeAttempt(practice.sectionPracticeId);
            });
        }
    }

    private async Task StartPracticeAttempt(int sectionPracticeId)
    {
        var attempt = await ApiService.Instance.CreatePracticeAttemptAsync(sectionPracticeId, userId);
        if (attempt == null)
        {
            errorText.text = "Failed to start practice. Please try again.";
            return;
        }

        PlayerPrefs.SetInt("practiceAttemptId", attempt.practiceAttemptId);
        PlayerPrefs.Save();

        UnityEngine.SceneManagement.SceneManager.LoadScene("SimulationScene");
    }
}
