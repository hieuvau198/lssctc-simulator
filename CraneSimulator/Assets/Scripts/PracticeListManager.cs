using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.SceneManagement;

public class PracticeListManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject practiceCardPrefab;
    public Transform contentPanel;
    public TextMeshProUGUI errorText;
    public TMP_Dropdown dropdown;
    public Button logoutButton;
    [Header("Final Exam UI")]
    public GameObject codePopup;             
    public TMP_InputField codeInput;         
    public Button confirmCodeButton;         
    public Button cancelCodeButton;          

    // Internal cache
    private string selectedSceneName = "";
    private string selectedPracticeCode = "";
    private int selectedPracticePartialId = 0;

    // Local data model
    private List<ClassDto> classList;
    private bool isFinalExam;
    private int selectedPracticePartialCode;
    [System.Serializable]
    public class PracticeItem
    {
        public int partialId;
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
                partialId = 0,
                practiceCode = "PRACTICE_08",
                practiceName = "Component Inspection",
                practiceDescription = "Trainee must walk around the crane and inspect all required components including hook block, boom, outriggers, column, and control panel.",
                estimatedDurationMinutes = 10,
                difficultyLevel = "Entry",
                sceneName = "Practice1"
            },

            new PracticeItem {
                partialId = 0,
                practiceCode = "PRACTICE_09",
                practiceName = "Zigzag Cargo Navigation",
                practiceDescription = "Lift a cargo and move it through a zigzag path without hitting obstacles while keeping the cargo stable.",
                estimatedDurationMinutes = 15,
                difficultyLevel = "Intermediate",
                sceneName = "Practice2"
            },

            new PracticeItem {
                partialId = 0,
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
        isFinalExam = PlayerPrefs.GetInt("IsFinalExam", 0) == 1;
        if (codePopup != null) codePopup.SetActive(false);
        if (confirmCodeButton != null) confirmCodeButton.onClick.AddListener(OnConfirmCode);
        if (cancelCodeButton != null) cancelCodeButton.onClick.AddListener(() => { if (codePopup != null) codePopup.SetActive(false); });
        if (logoutButton != null)
            logoutButton.onClick.AddListener(OnLogout);
        await PopulateClassDropdown();
        dropdown.onValueChanged.AddListener(OnClassSelected);
    }
    private async Task PopulateClassDropdown()
    {
        if (dropdown == null) return;
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
    private void OnLogout()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        ApiService.Instance.SetAuthorizationToken(null);
        SceneManager.LoadScene("Login");
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
        //var apiPractices = await ApiService.Instance.GetPracticesForClassAsync(selectedClass.id);
        //if (apiPractices == null || apiPractices.Count == 0)
        //{
        //    errorText.color = Color.red;
        //    errorText.text = "No practices found for this class.";
        //    ClearPracticeCards();
        //    return;
        //}

        //// Filter local practices to those whose IDs match API response
        //var filteredPractices = localPractices.FindAll(
        //    p => apiPractices.Any(api => api.practiceCode == p.practiceCode)
        //);
        

        // Branch by mode
        List<PracticeItem> uiPractices = new List<PracticeItem>();
        if (!isFinalExam)
        {
            // Normal practices
            var apiPractices = await ApiService.Instance.GetPracticesForClassAsync(selectedClass.id);

            if (apiPractices == null || apiPractices.Count == 0)
            {
                if (errorText != null)
                {
                    errorText.color = Color.red;
                    errorText.text = "No practices found for this class.";
                }
                ClearPracticeCards();
                return;
            }

            uiPractices = localPractices
                .Where(lp => apiPractices.Any(api => api.practiceCode == lp.practiceCode))
                .ToList();
        }
        else
        {
            // Final exam SE practices
            var examPractices = await ApiService.Instance.GetFinalExamSePracticesAsync(selectedClass.id);

            if (examPractices == null || examPractices.Count == 0)
            {
                if (errorText != null)
                {
                    errorText.color = Color.red;
                    errorText.text = "No final exam practices available for this class.";
                }
                ClearPracticeCards();
                return;
            }
            uiPractices = localPractices
                .Where(lp => examPractices.Any(api => api.practiceCode == lp.practiceCode))
                .ToList();
            foreach (var lp in uiPractices)
            {
                var match = examPractices.FirstOrDefault(api => api.practiceCode == lp.practiceCode);
                if (match != null)
                    lp.partialId = match.finalExamPartialId;  
            }
        }
        // Display
        DisplayPractices(uiPractices);
        errorText.color = Color.white;
        errorText.text = uiPractices.Count == 0 ? "No matching practices found." : "";
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
                selectedPracticeCode = practice.practiceCode;
                selectedSceneName = practice.sceneName;
                selectedPracticePartialId = practice.partialId;
                PlayerPrefs.SetString("selectedPracticeCode", practice.practiceCode);
                PlayerPrefs.SetInt("selectedPracticePartialId", practice.partialId);
                PlayerPrefs.Save();
                if (isFinalExam)

                    ShowExamPopup();
                else
                    LoadPracticeScene();
                // Load each practice’s own scene
                
            });
        }
    }
    private void ShowExamPopup()
    {
        codeInput.text = "";
        codePopup.SetActive(true);
    }
    private async void OnConfirmCode()
    {
        string examCode = codeInput.text.Trim();

        if (string.IsNullOrEmpty(examCode))
        {
            errorText.color = Color.red;
            errorText.text = "Please enter a code.";
            return;
        }

        
        if (selectedPracticePartialId == 0)
        {
            errorText.color = Color.red;
            errorText.text = "System error: partialId missing.";
            return;
        }
        errorText.text = "Validating exam code...";
        errorText.color = Color.white;
        object apiResponse = await ApiService.Instance.ValidateFinalExamSeCodeAsync(selectedPracticePartialId, examCode);

        if (apiResponse == null)
        {
            errorText.color = Color.red;
            errorText.text = "Validation failed. Try again.";
            return;
        }

        // CASE 1: invalid code → ErrorMessageDto
        if (apiResponse is ErrorMessageDto err)
        {
            errorText.color = Color.red;
            errorText.text = err.message;
            return;
        }
        FinalExamPartialDtoResponse exam = apiResponse as FinalExamPartialDtoResponse;
        if (exam == null)
        {
            errorText.color = Color.red;
            errorText.text = "Unexpected server response.";
            return;
        }
        PlayerPrefs.SetInt("FinalExamRecordId", exam.id);
        PlayerPrefs.Save();
        // Close popup and load scene
        codePopup.SetActive(false);
        LoadPracticeScene();
    }

    private void LoadPracticeScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(selectedSceneName);
    }

}
