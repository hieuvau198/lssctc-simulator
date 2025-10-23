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


    [Header("Optional Always-On Display")]
    public TextMeshProUGUI currentStepDisplay;

    private TraineePracticeStep[] steps;
    private int currentIndex = 0;

    //current step key for PlayerPrefs
    private const string CURRENT_STEP_KEY = "CurrentStepId";
    public int CurrentStepId => (steps != null && currentIndex < steps.Length) ? steps[currentIndex].stepId : -1;

    private string apiUrl = "https://lssctc-simulation.azurewebsites.net/api/TraineePractices/attempt/";

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
        int atempId = PlayerPrefs.GetInt("practiceAttemptId", 0);
        if (selectedPracticeId == -1 || userId == 0)
        {
            Debug.LogError("Missing selected practice ID or user ID!");
            yield break;
        }

        string fullUrl = $"{apiUrl}{atempId}/steps";
        Debug.Log($"Fetching steps from: {fullUrl}");

        using (UnityWebRequest www = UnityWebRequest.Get(fullUrl))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to fetch steps: {www.error}\nURL: {fullUrl}");
            }
            else
            {
                string json = www.downloadHandler.text;
                Debug.Log("Steps JSON:\n" + json);

                steps = JsonHelper.FromJson<TraineePracticeStep>(json);

                if (steps != null && steps.Length > 0)
                {
                    int savedStepId = PlayerPrefs.GetInt(CURRENT_STEP_KEY, steps[0].stepId);
                    int index = System.Array.FindIndex(steps, s => s.stepId == savedStepId);
                    currentIndex = index >= 0 ? index : 0;
                    ShowStep(currentIndex);
                }
                else
                {
                    Debug.LogWarning("No steps found for this practice!");
                }
            }
        }
    }
    public void MarkStepAsDone()
    {
        if (steps == null || steps.Length == 0) return;

        steps[currentIndex].isCompleted = true;
        PlayerPrefs.SetInt(CURRENT_STEP_KEY, steps[currentIndex].stepId);
        PlayerPrefs.Save();

        Debug.Log($"Step {steps[currentIndex].stepOrder} marked done.");

        // Move to next step automatically if available
        if (currentIndex < steps.Length - 1)
        {
            currentIndex++;
            Debug.Log($"Moving to next step: {steps[currentIndex].stepName}");
            ShowStep(currentIndex, true);
        }
        else
        {
            Debug.Log("All steps completed!");
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

        if (currentStepDisplay != null)
        {
            currentStepDisplay.text = $"Current Step: {currentIndex + 1}/{steps.Length} - {steps[currentIndex].stepName}";
        }
    }

    //IEnumerator AnimateStepChange()
    //{
    //    CanvasGroup cg = quizPanel.GetComponent<CanvasGroup>();
    //    if (cg == null) cg = quizPanel.AddComponent<CanvasGroup>();

    //    // Fade out
    //    for (float t = 1; t >= 0; t -= Time.deltaTime * 3)
    //    {
    //        cg.alpha = t;
    //        yield return null;
    //    }

    //    // Fade in
    //    for (float t = 0; t <= 1; t += Time.deltaTime * 3)
    //    {
    //        cg.alpha = t;
    //        yield return null;
    //    }
    //}

    IEnumerator AnimateStepChange()
    {
        CanvasGroup cg = quizPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = quizPanel.AddComponent<CanvasGroup>();

        RectTransform rect = quizPanel.GetComponent<RectTransform>();
        Vector3 originalScale = rect.localScale;

        float duration = 0.5f; // smoother timing
        float pauseTime = 0.2f; // short delay between fade out and fade in

        // === Fade Out with scale shrink ===
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float normalized = t / duration;
            float smooth = Mathf.SmoothStep(1f, 0f, normalized);
            cg.alpha = 1f - smooth;
            rect.localScale = Vector3.Lerp(originalScale, originalScale * 0.97f, smooth); // slight shrink
            yield return null;
        }

        cg.alpha = 0;
        rect.localScale = originalScale * 0.97f;

        // === Pause before showing new step ===
        yield return new WaitForSeconds(pauseTime);

        // === Fade In with scale grow ===
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float normalized = t / duration;
            float smooth = Mathf.SmoothStep(0f, 1f, normalized);
            cg.alpha = smooth;
            rect.localScale = Vector3.Lerp(originalScale * 0.97f, originalScale, smooth); // gentle grow
            yield return null;
        }

        cg.alpha = 1;
        rect.localScale = originalScale;
    }

}

