using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenuUI;       // Main pause menu panel
    public GameObject optionsMenuUI;     // Settings/options panel
    public Button resumeButton;
    public Button optionsButton;
    public Button backFromOptionsButton;
    public Button finishButton;

    [Header("Result UI")]
    public GameObject resultPanel;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI statusText;
    public Transform taskListParent;
    public GameObject taskResultPrefab; 

    public Button resultBackButton;

    [Header("Practice Manager")]
    public PracticeTaskManager practiceTaskManager; // Reference in scene with multiple tasks
    public ZigzagPracticeManager zigzagPracticeManager; // Reference in zigzag practice scene
    public CargoPositioningManager cargoPositioningManager; // Reference in cargo positioning practice scene

    private bool isPaused = false;

    private void Start()
    {
        int attemptId = PlayerPrefs.GetInt("practiceAttemptId", 0);

        // Button setup
        resumeButton.onClick.AddListener(Resume);
        optionsButton.onClick.AddListener(OpenOptions);
        backFromOptionsButton.onClick.AddListener(CloseOptions);

        
        finishButton.onClick.AddListener(() =>
        {
            _ = FinishPracticeAttempt();
        });
        

        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);
        resultPanel.SetActive(false);
        resultBackButton.onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("PracticeListScene");
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && isPaused == false)
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        optionsMenuUI.SetActive(false);
        Time.timeScale = 0f;
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OpenOptions()
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    
    private async Task FinishPracticeAttempt()
    {
        int classId = PlayerPrefs.GetInt("SelectedClassId", 0);
        string practiceCode = PlayerPrefs.GetString("selectedPracticeCode", "");
        int partialId = PlayerPrefs.GetInt("selectedPracticePartialId", 0);
        bool isFinalExam = PlayerPrefs.GetInt("IsFinalExam", 0) == 1;

        if (classId == 0 || string.IsNullOrEmpty(practiceCode))
        {
            Debug.LogError("ClassCode or PracticeCode not set, can't finish attempt.");
            return;
        }

        // 1. Collect results from managers
        List<PracticeAttemptTaskDto> practiceTasks = new List<PracticeAttemptTaskDto>(); // For Normal Practice
        List<SubmitSeTaskDto> examTasks = new List<SubmitSeTaskDto>();                   // For Final Exam

        float calculatedScore = 0; // Changed to float
        bool isPassed = false;

        if (practiceTaskManager != null)
        {
            foreach (var task in practiceTaskManager.tasks)
            {
                bool taskPassed = task.isCompleted;
                int score = taskPassed ? 100 : 0;

                // Add to standard practice list
                practiceTasks.Add(new PracticeAttemptTaskDto
                {
                    taskCode = task.taskCode,
                    score = score,
                    isPass = taskPassed
                });

                // Add to Exam list
                examTasks.Add(new SubmitSeTaskDto
                {
                    taskCode = task.taskCode,
                    isPass = taskPassed,
                    durationSecond = 0 // You can calculate duration per task if your manager tracks it
                });
            }
            if (practiceTasks.Count > 0)
            {
                calculatedScore = practiceTasks.Sum(t => t.score) / (float)practiceTasks.Count;
                isPassed = practiceTasks.All(t => t.isPass);
            }
        }
        else if (zigzagPracticeManager != null)
        {
            // Single task logic
            calculatedScore = zigzagPracticeManager.totalPoints ;
            isPassed = zigzagPracticeManager.IsCompleted && !zigzagPracticeManager.IsFailed;

            string code = "TASK_06"; // Make sure this matches your DB

            practiceTasks.Add(new PracticeAttemptTaskDto { taskCode = code, score = (int)calculatedScore, isPass = isPassed });

            examTasks.Add(new SubmitSeTaskDto
            {
                taskCode = code,
                isPass = isPassed,
                durationSecond = (int)Time.timeSinceLevelLoad // Example duration 
            });
        }
        else if (cargoPositioningManager != null)
        {
            calculatedScore = cargoPositioningManager.totalPoints;
            isPassed = cargoPositioningManager.IsCompleted && !cargoPositioningManager.IsFailed;

            string code = "TASK_07";

            practiceTasks.Add(new PracticeAttemptTaskDto { taskCode = code, score = (int)calculatedScore, isPass = isPassed });

            examTasks.Add(new SubmitSeTaskDto
            {
                taskCode = code,
                isPass = isPassed,
                durationSecond = (int)Time.timeSinceLevelLoad
            });
        }
        else
        {
            Debug.LogError("No practice manager assigned.");
            return;
        }

        // 2. Submit Logic based on Type
        if (isFinalExam)
        {
            if (partialId == 0)
            {
                Debug.LogError("Final exam partialId missing.");
                return;
            }

            // Create the Final Exam DTO with the list of tasks
            SubmitSeFinalDto dto = new SubmitSeFinalDto
            {
                marks = calculatedScore / 10.0f, // Normalize if backend expects 0-10, else keep raw
                isPass = isPassed,
                description = isPassed ? "Exam completed successfully" : "Exam failed",
                completeTime = System.DateTime.UtcNow.AddHours(7).ToString("o"),
                tasks = examTasks // <--- SENDING TASKS HERE
            };

            // Call the updated API which returns FinalExamPartial (containing task results)
            var finalRes = await ApiService.Instance.SubmitFinalExamSeAsync(partialId, dto);

            if (finalRes == null)
            {
                Debug.LogError("Final exam submit failed.");
                return;
            }

            Debug.Log("Final exam submitted. Partial ID=" + finalRes.id);
            ShowFinalExamResult(finalRes);
        }
        else
        {
            // Standard Practice Submission
            var attemptDto = new PracticeAttemptCompleteDto
            {
                classId = classId,
                practiceCode = practiceCode,
                score = (int)calculatedScore,
                description = isPassed ? "Practice completed successfully" : "Practice failed",
                isPass = isPassed,
                practiceAttemptTasks = practiceTasks
            };

            var response = await ApiService.Instance.CompletePracticeAttemptAsync(attemptDto);
            if (!string.IsNullOrEmpty(response))
            {
                Debug.Log("[DEBUG] Attempt completed: " + response);
                PlayerPrefs.Save();
                var result = JsonUtility.FromJson<PracticeAttemptCompleteResponse>(response);
                ShowResult(result);
            }
            else
            {
                Debug.LogError("Failed to complete attempt.");
            }
        }
    }
    private void ShowResult(PracticeAttemptCompleteResponse result)
    {
        // Freeze game
        Time.timeScale = 0f;

        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);
        resultPanel.SetActive(true);

        scoreText.text = "Score: " + result.score;
        statusText.text = result.isPass ? "Result: <color=green>Pass</color>" : "Result: <color=red>Fail</color>";

        // Clear old items
        foreach (Transform child in taskListParent)
            Destroy(child.gameObject);

        // Create task result items
        foreach (var task in result.practiceAttemptTasks)
        {
            GameObject item = Instantiate(taskResultPrefab, taskListParent);

            item.transform.Find("TaskName").GetComponent<TextMeshProUGUI>().text = task.taskCode;
            item.transform.Find("TaskScore").GetComponent<TextMeshProUGUI>().text = "Score: " + task.score;
            item.transform.Find("TaskStatus").GetComponent<TextMeshProUGUI>().text =
                task.isPass ? "Pass" : "Fail";
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    private void ShowFinalExamResult(FinalExamPartial res)
    {
        Time.timeScale = 0f;

        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);
        resultPanel.SetActive(true);

        // Display Overall Exam Results
        scoreText.text = $"Total Marks: {res.marks:F2} / 10"; // Assuming 10 is max
        statusText.text = res.isPass ? "Result: <color=green>Pass</color>" : "Result: <color=red>Fail</color>";

        foreach (Transform child in taskListParent)
            Destroy(child.gameObject);

        // [NEW] Iterate through the returned tasks list from the partial
        if (res.tasks != null && res.tasks.Count > 0)
        {
            foreach (var task in res.tasks)
            {
                GameObject item = Instantiate(taskResultPrefab, taskListParent);

                var nameLabel = item.transform.Find("TaskName").GetComponent<TextMeshProUGUI>();
                var scoreLabel = item.transform.Find("TaskScore").GetComponent<TextMeshProUGUI>();
                var statusLabel = item.transform.Find("TaskStatus").GetComponent<TextMeshProUGUI>();

                // Display Task Name/Code
                if (nameLabel != null)
                    nameLabel.text = !string.IsNullOrEmpty(task.name) ? task.name : task.taskCode;

                // SE Tasks don't usually have individual scores returned in this specific DTO, 
                // but we can show duration or just leave it blank/custom string
                if (scoreLabel != null)
                    scoreLabel.text = task.durationSecond > 0 ? $"{task.durationSecond}s" : "-";

                // Display Pass/Fail status
                if (statusLabel != null)
                    statusLabel.text = task.isPass ? "<color=green>Pass</color>" : "<color=red>Fail</color>";
            }
        }
        else
        {
            // Fallback if no tasks returned (just show main exam info)
            GameObject item = Instantiate(taskResultPrefab, taskListParent);
            item.transform.Find("TaskName").GetComponent<TextMeshProUGUI>().text = "Final Exam";
            item.transform.Find("TaskStatus").GetComponent<TextMeshProUGUI>().text = res.status;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
