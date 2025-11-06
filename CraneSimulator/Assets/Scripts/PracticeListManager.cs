using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PracticeListManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject practiceCardPrefab;
    public Transform contentPanel;
    public TextMeshProUGUI errorText;

    // Local data model
    [System.Serializable]
    public class PracticeItem
    {
        public int practiceId;
        public string practiceName;
        public string practiceDescription;
        public int estimatedDurationMinutes;
        public string difficultyLevel;
        public string sceneName; 
    }

    // Local dummy data
    private List<PracticeItem> localPractices = new List<PracticeItem>()
    {
        new PracticeItem {
            practiceId = 1,
            practiceName = "Hook Operation",
            practiceDescription = "Learn to attach and lift cargo correctly.",
            estimatedDurationMinutes = 10,
            difficultyLevel = "Beginner",
            sceneName = "Practice1" 
        },
        new PracticeItem {
            practiceId = 2,
            practiceName = "Crane Movement",
            practiceDescription = "Control the crane and move loads safely.",
            estimatedDurationMinutes = 15,
            difficultyLevel = "Intermediate",
            sceneName = "Practice2"
        },
        
        // new PracticeItem {
        //     practiceId = 3,
        //     practiceName = "Precision Challenge",
        //     practiceDescription = "Place the load accurately into position.",
        //     estimatedDurationMinutes = 20,
        //     difficultyLevel = "Advanced",
        //     sceneName = "PrecisionChallengeScene"
        // }
    };

    void Start()
    {
        LoadLocalPractices();
    }

    private void LoadLocalPractices()
    {
        // Clear old cards (except header)
        foreach (Transform child in contentPanel)
        {
            if (child.name == "HeaderPanel") continue;
            Destroy(child.gameObject);
        }

        if (localPractices == null || localPractices.Count == 0)
        {
            errorText.text = "No practices available.";
            return;
        }

        errorText.text = "";

        foreach (var practice in localPractices)
        {
            GameObject card = Instantiate(practiceCardPrefab, contentPanel);

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
                PlayerPrefs.SetInt("selectedPracticeId", practice.practiceId);
                PlayerPrefs.SetString("selectedPracticeName", practice.practiceName);
                PlayerPrefs.SetString("selectedPracticeDescription", practice.practiceDescription);
                PlayerPrefs.SetString("selectedPracticeDifficulty", practice.difficultyLevel);
                PlayerPrefs.SetInt("selectedPracticeDuration", practice.estimatedDurationMinutes);
                PlayerPrefs.Save();

                // Load each practice’s own scene
                UnityEngine.SceneManagement.SceneManager.LoadScene(practice.sceneName);
            });
        }
    }
}
