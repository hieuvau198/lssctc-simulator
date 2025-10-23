using UnityEngine;
using TMPro;
using System.Threading.Tasks;
using System.Collections;

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

    private const string CURRENT_STEP_KEY = "CurrentStepId";
    public int CurrentStepId => (steps != null && currentIndex < steps.Length) ? steps[currentIndex].stepId : -1;

    void Start()
    {
        quizPanel.SetActive(false);
        _ = FetchSteps();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            quizPanel.SetActive(!quizPanel.activeSelf);
            if (quizPanel.activeSelf && steps != null && steps.Length > 0)
                ShowStep(currentIndex, true);
        }

        if (quizPanel.activeSelf && steps != null && steps.Length > 0)
        {
            if (Input.GetKeyDown(KeyCode.Q))
                ShowStep(currentIndex - 1, true);
            if (Input.GetKeyDown(KeyCode.E))
                ShowStep(currentIndex + 1, true);
        }
    }

    private async Task FetchSteps()
    {
        int selectedPracticeId = PlayerPrefs.GetInt("selectedPracticeId", -1);
        int userId = PlayerPrefs.GetInt("UserID", 0);
        int atempId = PlayerPrefs.GetInt("practiceAttemptId", 0);
        if (selectedPracticeId == -1 || userId == 0 || atempId == 0)
        {
            Debug.LogError("Missing selected practice ID, user ID or attempt ID!");
            return;
        }

        var arr = await ApiService.Instance.GetPracticeAttemptStepsAsync(atempId);
        if (arr == null || arr.Length == 0)
        {
            Debug.LogWarning("No steps found for this practice!");
            return;
        }

        steps = arr;
        int savedStepId = PlayerPrefs.GetInt(CURRENT_STEP_KEY, steps[0].stepId);
        int index = System.Array.FindIndex(steps, s => s.stepId == savedStepId);
        currentIndex = index >= 0 ? index : 0;
        ShowStep(currentIndex);
    }

    public void MarkStepAsDone()
    {
        if (steps == null || steps.Length == 0) return;

        steps[currentIndex].isCompleted = true;
        PlayerPrefs.SetInt(CURRENT_STEP_KEY, steps[currentIndex].stepId);
        PlayerPrefs.Save();

        Debug.Log($"Step {steps[currentIndex].stepOrder} marked done.");

        if (currentIndex < steps.Length - 1)
        {
            currentIndex++;
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
            currentStepDisplay.text = $"Current Step: {currentIndex + 1}/{steps.Length} - {steps[currentIndex].stepName}";
    }

    IEnumerator AnimateStepChange()
    {
        CanvasGroup cg = quizPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = quizPanel.AddComponent<CanvasGroup>();

        RectTransform rect = quizPanel.GetComponent<RectTransform>();
        Vector3 originalScale = rect.localScale;

        float duration = 0.5f;
        float pauseTime = 0.2f;

        // Fade out + slight shrink (smooth)
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float normalized = t / duration;
            float smooth = Mathf.SmoothStep(1f, 0f, normalized);
            cg.alpha = 1f - smooth;
            rect.localScale = Vector3.Lerp(originalScale, originalScale * 0.97f, smooth);
            yield return null;
        }

        cg.alpha = 0;
        rect.localScale = originalScale * 0.97f;

        yield return new WaitForSeconds(pauseTime);

        // Fade in + grow back
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float normalized = t / duration;
            float smooth = Mathf.SmoothStep(0f, 1f, normalized);
            cg.alpha = smooth;
            rect.localScale = Vector3.Lerp(originalScale * 0.97f, originalScale, smooth);
            yield return null;
        }

        cg.alpha = 1;
        rect.localScale = originalScale;
    }
}
