using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenuUI;       // Main pause menu panel
    public GameObject optionsMenuUI;     // Settings/options panel
    public Button resumeButton;
    public Button optionsButton;
    public Button backFromOptionsButton;
    public Button finishButton;

    private bool isPaused = false;

    private void Start()
    {
        int attemptId = PlayerPrefs.GetInt("practiceAttemptId", 0);

        // Button setup
        resumeButton.onClick.AddListener(Resume);
        optionsButton.onClick.AddListener(OpenOptions);
        backFromOptionsButton.onClick.AddListener(CloseOptions);

        if (attemptId == 0)
        {
            finishButton.interactable = false;
        }
        else
        {
            finishButton.onClick.AddListener(() =>
            {
                _ = FinishPracticeAttempt(attemptId);
            });
        }

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

    private async Task FinishPracticeAttempt(int attemptId)
    {
        var raw = await ApiService.Instance.CompleteAttemptAsync(attemptId);
        if (!string.IsNullOrEmpty(raw))
        {
            Debug.Log("[DEBUG] Attempt completed: " + raw);
            PlayerPrefs.DeleteKey("practiceAttemptId");
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
