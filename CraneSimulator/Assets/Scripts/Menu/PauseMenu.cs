using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System;
using System.Collections;

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
    [Header("Audio")]
    public AudioSource backgroundAudio;
    [Header("Practice Manager")]
    public PracticeTaskManager practiceTaskManager; // Reference in scene with multiple tasks
    public ZigzagPracticeManager zigzagPracticeManager; // Reference in zigzag practice scene
    public CargoPositioningManager cargoPositioningManager; // Reference in cargo positioning practice scene

    [Header("External UI Managers")]
    public TutorialManager tutorialManager;       
    public SelectionManager selectionManager;      
    public ZigzagPracticeUI zigzagUI;               
    public PracticeTaskManager practiceUI;          
    public CargoPositioningUI cargoUI;
    private bool isPaused = false;


    private bool hasSubmitted = false;
    private bool isQuitting = false;

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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            StartCoroutine(RestoreCursorState());
        }
    }


    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            if (!isPaused && !resultPanel.activeSelf)
            {
                Pause();
            }
        }
        else
        {
            StartCoroutine(RestoreCursorState());
        }
    }

    private void OnApplicationQuit()
    {
        if (hasSubmitted) return;
        isQuitting = true;
        SubmitFailOnQuit();
    }

    

    private IEnumerator RestoreCursorState()
    {
        yield return new WaitForEndOfFrame();
        RefreshCursorState();
    }


    public void RefreshCursorState()
    {
        if (pauseMenuUI.activeSelf || resultPanel.activeSelf)
        {
            SetCursor(true);
            return;
        }

        if (tutorialManager != null && tutorialManager.tutorialPanel.activeSelf)
        {
            SetCursor(true);
            return;
        }

        if (zigzagUI != null && zigzagUI.panel.activeSelf)
        {
            SetCursor(true);
            return;
        }

        if (practiceUI != null && practiceUI.taskPanel.activeSelf)
        {
            SetCursor(true);
            return;
        }

        if (cargoUI != null && cargoUI.panel.activeSelf)
        {
            SetCursor(true);
            return;
        }

        if (selectionManager != null && selectionManager.IsAnyPanelOpen())
        {
            SetCursor(true);
            return;
        }

        SetCursor(false);
    }

    private void SetCursor(bool unlocked)
    {
        Cursor.visible = unlocked;
        Cursor.lockState = unlocked ? CursorLockMode.None : CursorLockMode.Locked;
    }
    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        optionsMenuUI.SetActive(false);
        Time.timeScale = 0f;
        isPaused = true;
        if (backgroundAudio != null && backgroundAudio.isPlaying)
            backgroundAudio.Pause();
        SetCursor(true);

    }
    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        if (backgroundAudio != null)
            backgroundAudio.UnPause();
        RefreshCursorState();
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
        if (hasSubmitted) return;
        hasSubmitted = true;

        string startTimeStr = PlayerPrefs.GetString("PracticeStartTime", "");
        DateTimeOffset startTime;
        if (!DateTimeOffset.TryParse(startTimeStr, out startTime))
        {
            Debug.LogWarning("Start time missing, using current time.");
            startTime = GetVietnamNow();
        }

        DateTimeOffset endTime = GetVietnamNow();

        int durationSeconds = (int)(endTime - startTime).TotalSeconds;
        int classId = PlayerPrefs.GetInt("SelectedClassId", 0);
        string practiceCode = PlayerPrefs.GetString("selectedPracticeCode", "");
        int partialId = PlayerPrefs.GetInt("selectedPracticePartialId", 0);
        int activityRecordId = PlayerPrefs.GetInt("activityRecordId", 0);
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
        int mistake = 0;

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

            mistake = zigzagPracticeManager.TotalMistakes;
            practiceTasks.Add(new PracticeAttemptTaskDto
            {
                taskCode = code,
                score = Mathf.RoundToInt(calculatedScore),
                isPass = isPassed,
                mistakes = zigzagPracticeManager.TotalMistakes
            });

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
            mistake = cargoPositioningManager.TotalMistakes;
            practiceTasks.Add(new PracticeAttemptTaskDto 
            { 
                taskCode = code, 
                score = (int)calculatedScore,
                isPass = isPassed ,
                mistakes = cargoPositioningManager.TotalMistakes
            });

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
                marks = calculatedScore ,
                isPass = isPassed,
                description = isPassed ? "Exam completed successfully" : "Exam failed",
                startTime = startTime.ToString("o"),
                completeTime = endTime.ToString("o"),
                tasks = examTasks 
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
                activityRecordId = activityRecordId,
                classId = classId,
                practiceCode = practiceCode,
                score = (int)calculatedScore,
                description = isPassed ? "Practice completed successfully" : "Practice failed",
                startTime = startTime.ToString("o"),
                endTime = endTime.ToString("o"),
                totalMistakes = practiceTasks.Sum(t => t.mistakes),
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
    private void SubmitFailOnQuit()
    {
        try
        {
            if (hasSubmitted) return;
            hasSubmitted = true;

            bool isFinalExam = PlayerPrefs.GetInt("IsFinalExam", 0) == 1;

            int classId = PlayerPrefs.GetInt("SelectedClassId", 0);
            string practiceCode = PlayerPrefs.GetString("selectedPracticeCode", "");
            int activityRecordId = PlayerPrefs.GetInt("activityRecordId", 0);
            int partialId = PlayerPrefs.GetInt("selectedPracticePartialId", 0);

            if (classId == 0 || string.IsNullOrEmpty(practiceCode))
                return;

            DateTimeOffset endTime = GetVietnamNow();

            
            List<PracticeAttemptTaskDto> practiceTasks = new();
            List<SubmitSeTaskDto> examTasks = new();

            
            if (practiceTaskManager != null)
            {
                foreach (var task in practiceTaskManager.tasks)
                {
                    practiceTasks.Add(new PracticeAttemptTaskDto
                    {
                        taskCode = task.taskCode,
                        score = 0,
                        isPass = false,
                        mistakes = 0
                    });

                    examTasks.Add(new SubmitSeTaskDto
                    {
                        taskCode = task.taskCode,
                        isPass = false,
                        durationSecond = 0
                    });
                }
            }
            
            else if (zigzagPracticeManager != null)
            {
                string code = "TASK_06";

                practiceTasks.Add(new PracticeAttemptTaskDto
                {
                    taskCode = code,
                    score = 0,
                    isPass = false,
                    mistakes = zigzagPracticeManager.TotalMistakes
                });

                examTasks.Add(new SubmitSeTaskDto
                {
                    taskCode = code,
                    isPass = false,
                    durationSecond = (int)Time.timeSinceLevelLoad
                });
            }
            
            else if (cargoPositioningManager != null)
            {
                string code = "TASK_07";

                practiceTasks.Add(new PracticeAttemptTaskDto
                {
                    taskCode = code,
                    score = 0,
                    isPass = false,
                    mistakes = cargoPositioningManager.TotalMistakes
                });

                examTasks.Add(new SubmitSeTaskDto
                {
                    taskCode = code,
                    isPass = false,
                    durationSecond = (int)Time.timeSinceLevelLoad
                });
            }
            else
            {
                Debug.LogWarning("❌ Không tìm thấy Practice Manager để submit fail.");
                return;
            }

            
            if (isFinalExam)
            {
                if (partialId == 0) return;

                SubmitSeFinalDto dto = new SubmitSeFinalDto
                {
                    marks = 0,
                    isPass = false,
                    description = "Bài thi thất bại (Thoát ứng dụng)",
                    startTime = PlayerPrefs.GetString("PracticeStartTime", endTime.ToString("o")),
                    completeTime = endTime.ToString("o"),
                    tasks = examTasks
                };

                // Fire & Forget (không await khi quit)
                _ = ApiService.Instance.SubmitFinalExamSeAsync(partialId, dto);
            }
            else
            {
                PracticeAttemptCompleteDto dto = new PracticeAttemptCompleteDto
                {
                    activityRecordId = activityRecordId,
                    classId = classId,
                    practiceCode = practiceCode,
                    score = 0,
                    description = "Bài thực hành thất bại (Thoát ứng dụng)",
                    startTime = PlayerPrefs.GetString("PracticeStartTime", endTime.ToString("o")),
                    endTime = endTime.ToString("o"),
                    totalMistakes = practiceTasks.Sum(t => t.mistakes),
                    isPass = false,
                    practiceAttemptTasks = practiceTasks
                };

                _ = ApiService.Instance.CompletePracticeAttemptAsync(dto);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Lỗi khi submit fail on quit: " + ex.Message);
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

            item.transform.Find("TaskName").GetComponent<TextMeshProUGUI>().text = task.description;
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
    //Helpers
    private DateTimeOffset GetVietnamNow()
    {
        return DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7));
    }
}
