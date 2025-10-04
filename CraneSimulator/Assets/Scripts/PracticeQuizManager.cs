using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

[System.Serializable]
public class PracticeStep
{
    public int id;
    public int practiceId;
    public string stepName;
    public string stepDescription;
    public string expectedResult;
    public int stepOrder;
}

public class PracticeQuizManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject quizPanel;
    public TextMeshProUGUI stepTitle;
    public TextMeshProUGUI stepDescription;
    public TextMeshProUGUI expectedResult;
    public TextMeshProUGUI stepCounter;

    private PracticeStep[] steps;
    private int currentIndex = 0;

    private string apiUrl = "https://lssctc-simulation.azurewebsites.net/api/PracticeSteps/practice/";

    void Start()
    {
        quizPanel.SetActive(false);

        // Fetch steps at start so it's ready
        StartCoroutine(FetchSteps());
    }

    void Update()
    {
        // Toggle panel with Tab key
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            quizPanel.SetActive(!quizPanel.activeSelf);

            if (quizPanel.activeSelf && steps != null && steps.Length > 0)
            {
                ShowStep(currentIndex, true);
            }
        }

        if (quizPanel.activeSelf) // only works when panel is open
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
        if (selectedPracticeId == -1)
        {
            Debug.LogError("No practice selected!");
            yield break;
        }
        string fullUrl = apiUrl + selectedPracticeId;
        using (UnityWebRequest www = UnityWebRequest.Get(fullUrl))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to fetch steps: " + www.error);
            }
            else
            {
                string json = www.downloadHandler.text;
                steps = JsonHelper.FromJson<PracticeStep>(json);
            }
        }
    }

    void ShowStep(int index, bool animate = false)
    {
        if (steps == null || steps.Length == 0) return;

        currentIndex = Mathf.Clamp(index, 0, steps.Length - 1);

        if (animate) StartCoroutine(AnimateStepChange());

        stepTitle.text = steps[currentIndex].stepName;
        stepDescription.text = steps[currentIndex].stepDescription;
        expectedResult.text = steps[currentIndex].expectedResult;
        stepCounter.text = $"Step {currentIndex + 1}/{steps.Length}";
    }

    IEnumerator AnimateStepChange()
    {
        CanvasGroup cg = quizPanel.GetComponent<CanvasGroup>();

        if (cg == null) // Add CanvasGroup dynamically if missing
            cg = quizPanel.AddComponent<CanvasGroup>();

        // Fade out
        for (float t = 1; t >= 0; t -= Time.deltaTime * 3)
        {
            cg.alpha = t;
            yield return null;
        }

        // (Texts are updated in ShowStep before fade-in starts)

        // Fade in
        for (float t = 0; t <= 1; t += Time.deltaTime * 3)
        {
            cg.alpha = t;
            yield return null;
        }
    }
}
