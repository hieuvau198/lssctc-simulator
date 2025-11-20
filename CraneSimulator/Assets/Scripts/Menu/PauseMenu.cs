using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenuUI;       // Main pause menu panel
    public GameObject optionsMenuUI;     // Settings/options panel
    public Button resumeButton;
    public Button optionsButton;
    public Button backFromOptionsButton;
    public Button finishButton;

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
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
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
        int practiceId = PlayerPrefs.GetInt("selectedPracticeId", 0);

        if (classId == 0 || practiceId == 0)
        {
            Debug.LogError("ClassId or PracticeId not set, can't finish attempt.");
            return;
        }

        List<PracticeAttemptTaskDto> practiceAttemptTasks = new List<PracticeAttemptTaskDto>();
        int totalScore = 0;
        bool isPassed = false;

        if (practiceTaskManager != null)
        {
            // Build attempt from multiple tasks
            foreach (var task in practiceTaskManager.tasks)
            {
                practiceAttemptTasks.Add(new PracticeAttemptTaskDto
                {
                    taskId = task.taskId,
                    score = task.isCompleted ? 100 : 0,
                    isPass = task.isCompleted
                });
            }
            totalScore = practiceAttemptTasks.Sum(t => t.score) / practiceAttemptTasks.Count;
            isPassed = practiceAttemptTasks.All(t => t.isPass);
        }
        else if (zigzagPracticeManager != null)
        {
            // Single task/zag practice - just summary pass and score
            totalScore = zigzagPracticeManager.totalPoints;
            isPassed = zigzagPracticeManager.IsCompleted && !zigzagPracticeManager.IsFailed;

            practiceAttemptTasks.Add(new PracticeAttemptTaskDto
            {
                taskId = 22,   
                score = totalScore,
                isPass = isPassed
            });
        }
        else if (cargoPositioningManager != null)
        {
            // Single task/zag practice - just summary pass and score
            totalScore = cargoPositioningManager.totalPoints;
            isPassed = cargoPositioningManager.IsCompleted && !cargoPositioningManager.IsFailed;

            practiceAttemptTasks.Add(new PracticeAttemptTaskDto
            {
                taskId = 23,
                score = totalScore,
                isPass = isPassed
            });
        }
        else
        {
            Debug.LogError("No practice manager assigned.");
            return;
        }

        var attemptDto = new PracticeAttemptCompleteDto
        {
            classId = classId,
            practiceId = practiceId,
            score = totalScore,
            description = isPassed ? "Practice completed successfully" : "Practice failed",
            isPass = isPassed,
            practiceAttemptTasks = practiceAttemptTasks
        };

        var response = await ApiService.Instance.CompletePracticeAttemptAsync(attemptDto);
        if (!string.IsNullOrEmpty(response))
        {
            Debug.Log("[DEBUG] Attempt completed: " + response);
            PlayerPrefs.Save();
            Time.timeScale = 1f;
            SceneManager.LoadScene("PracticeListScene");
        }
        else
        {
            Debug.LogError("Failed to complete attempt.");
        }
    }


}
