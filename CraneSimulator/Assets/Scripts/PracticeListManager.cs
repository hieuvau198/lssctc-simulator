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
        public string practiceCode;
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
                practiceCode = "PRACTICE_08",
                practiceName = "Component Inspection",
                practiceDescription = "Trainee must walk around the crane and inspect all required components including hook block, boom, outriggers, column, and control panel.",
                estimatedDurationMinutes = 10,
                difficultyLevel = "Entry",
                sceneName = "Practice1"
            },

            new PracticeItem {
                practiceCode = "PRACTICE_09",
                practiceName = "Zigzag Cargo Navigation",
                practiceDescription = "Lift a cargo and move it through a zigzag path without hitting obstacles while keeping the cargo stable.",
                estimatedDurationMinutes = 15,
                difficultyLevel = "Intermediate",
                sceneName = "Practice2"
            },

            new PracticeItem {
                practiceCode = "PRACTICE_10",
                practiceName = "Cargo Positioning Challenge",
                practiceDescription = "Move the cargo and place it accurately inside the designated circle on the ground.",
                estimatedDurationMinutes = 8,
                difficultyLevel = "Entry",
                sceneName = "Practice3"
            }
    };

    private async void Start()
    {
        await PopulateClassDropdown();
        dropdown.onValueChanged.AddListener(OnClassSelected);
    }
    private async Task PopulateClassDropdown()
    {
        dropdown.ClearOptions();
        errorText.color = Color.white;
        errorText.text = "Loading classes...";
        classList = await ApiService.Instance.GetClassesForUserAsync();
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

        PlayerPrefs.SetInt("SelectedClassId", selectedClass.id);
        PlayerPrefs.Save();

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
            p => apiPractices.Any(api => api.practiceCode == p.practiceCode)
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
                PlayerPrefs.SetString("selectedPracticeCode", practice.practiceCode);
                PlayerPrefs.Save();

                // Load each practice’s own scene
                UnityEngine.SceneManagement.SceneManager.LoadScene(practice.sceneName);
            });
        }
    }
    
}
