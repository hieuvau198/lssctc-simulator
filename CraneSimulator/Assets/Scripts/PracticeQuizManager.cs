using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

[System.Serializable]
public class TraineePracticeStep
{
    public int stepId;
    public string stepName;
    public string stepDescription;
    public string expectedResult;
    public int stepOrder;
    public bool isCompleted;
    public int practiceId;

    // Optional extra fields if you want to expand UI later
    public string actionName;
    public string actionDescription;
    public string actionKey;
    public string componentName;
    public string componentDescription;
    public string componentImageUrl;
}
public class PracticeQuizManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject quizPanel;
    public TextMeshProUGUI stepTitle;
    public TextMeshProUGUI stepDescription;
    public TextMeshProUGUI expectedResult;
    public TextMeshProUGUI stepCounter;

    private TraineePracticeStep[] steps;
    private int currentIndex = 0;

    // New API URL
    private string apiUrl = "https://lssctc-simulation.azurewebsites.net/api/TraineePractices/practice/";

    void Start()
    {
        quizPanel.SetActive(false);
        StartCoroutine(FetchSteps());
    }

    void Update()
    {
        // Toggle quiz panel
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            quizPanel.SetActive(!quizPanel.activeSelf);

            if (quizPanel.activeSelf && steps != null && steps.Length > 0)
            {
                ShowStep(currentIndex, true);
            }
        }

        // Navigation with Q / E
        if (quizPanel.activeSelf && steps != null && steps.Length > 0)
        {
            if (Input.GetKeyDown(KeyCode.Q))
                ShowStep(currentIndex - 1, true);

            if (Input.GetKeyDown(KeyCode.E))
                ShowStep(currentIndex + 1, true);
        }
    }

    IEnumerator FetchSteps()
    {
        int selectedPracticeId = PlayerPrefs.GetInt("selectedPracticeId", -1);
        int userId = PlayerPrefs.GetInt("UserID", 0);

        if (selectedPracticeId == -1 || userId == 0)
        {
            Debug.LogError("Missing selected practice ID or user ID!");
            yield break;
        }

        string fullUrl = $"{apiUrl}{selectedPracticeId}/trainee/{userId}/steps";
        Debug.Log($"Fetching steps from: {fullUrl}");

        using (UnityWebRequest www = UnityWebRequest.Get(fullUrl))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to fetch steps: {www.error}\nURL: {fullUrl}");
                Debug.LogError("Response: " + www.downloadHandler.text);
            }
            else
            {
                string json = www.downloadHandler.text;
                Debug.Log("Steps JSON:\n" + json);

                steps = JsonHelper.FromJson<TraineePracticeStep>(json);

                if (steps != null && steps.Length > 0)
                {
                    currentIndex = 0;
                    ShowStep(currentIndex);
                }
                else
                {
                    Debug.LogWarning("No steps found for this practice!");
                }
            }
        }
    }

    void ShowStep(int index, bool animate = false)
    {
        if (steps == null || steps.Length == 0) return;

        currentIndex = Mathf.Clamp(index, 0, steps.Length - 1);

        if (animate)
            StartCoroutine(AnimateStepChange());

        stepTitle.text = steps[currentIndex].stepName;
        stepDescription.text = steps[currentIndex].stepDescription;
        expectedResult.text = steps[currentIndex].expectedResult;
        stepCounter.text = $"Step {currentIndex + 1}/{steps.Length}";
    }

    IEnumerator AnimateStepChange()
    {
        CanvasGroup cg = quizPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = quizPanel.AddComponent<CanvasGroup>();

        // Fade out
        for (float t = 1; t >= 0; t -= Time.deltaTime * 3)
        {
            cg.alpha = t;
            yield return null;
        }

        // Fade in
        for (float t = 0; t <= 1; t += Time.deltaTime * 3)
        {
            cg.alpha = t;
            yield return null;
        }
    }
}

