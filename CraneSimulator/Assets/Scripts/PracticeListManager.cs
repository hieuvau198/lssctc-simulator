using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public class PracticeListManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject practiceCardPrefab;
    public Transform contentPanel;
    public TextMeshProUGUI errorText;
    public TMP_Dropdown dropdown;


    // Local data model
    private List<ClassDto> classList;
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

    private async void Start()
    {
        //int userId = PlayerPrefs.GetInt("UserId", 0);
        int userId = 8;
        await PopulateClassDropdown(userId);
        dropdown.onValueChanged.AddListener(OnClassSelected);
    }
    private async Task PopulateClassDropdown(int userId)
    {
        dropdown.ClearOptions();
        errorText.color = Color.white;
        errorText.text = "Loading classes...";
        classList = await ApiService.Instance.GetClassesForUserAsync(userId);
        List<string> options = new List<string>();

        options.Add("Select a class..."); // Null/default item
        if (classList != null)
            foreach (var c in classList)
                options.Add(c.name);

        dropdown.AddOptions(options);
        dropdown.value = 0; // Start with the default option
        errorText.text = "";
    }

    private async void OnClassSelected(int index)
    {
        // Index 0 is default/null, so ignore
        if (index == 0 || classList == null || index - 1 < 0 || index - 1 >= classList.Count)
        {
            // Optionally clear practices if default/null is selected
            ClearPracticeCards();
            return;
        }

        var selectedClass = classList[index - 1];
        errorText.color = Color.white;
        errorText.text = $"Loading practices for: {selectedClass.name}";

        // Get practice list from API
        var apiPractices = await ApiService.Instance.GetPracticesForClassAsync(selectedClass.id);
        if (apiPractices == null || apiPractices.Count == 0)
        {
            errorText.color = Color.red;
            errorText.text = "No practices found for this class.";
            ClearPracticeCards();
            return;
        }

        // Filter local practices to those whose IDs match API response
        var filteredPractices = localPractices.FindAll(
            p => apiPractices.Any(api => api.id == p.practiceId)
        );

        // Display
        DisplayPractices(filteredPractices);
        errorText.color = Color.white;
        errorText.text = filteredPractices.Count == 0 ? "No matching practices found." : "";
    }
    private void ClearPracticeCards()
    {
        foreach (Transform child in contentPanel)
        {
            if (child.name == "HeaderPanel") continue;
            Destroy(child.gameObject);
        }
    }
    private void DisplayPractices(List<PracticeItem> practices)
    {
        ClearPracticeCards();
        if (practices == null || practices.Count == 0) return;
        foreach (var practice in practices)
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
