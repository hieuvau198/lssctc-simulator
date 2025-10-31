using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class FinishAttempt : MonoBehaviour
{
    [Header("UI References")]
    public Button finishButton;

    private void Start()
    {
        int attemptId = PlayerPrefs.GetInt("practiceAttemptId", 0);
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
    }

    private async Task FinishPracticeAttempt(int attemptId)
    {
        var raw = await ApiService.Instance.CompleteAttemptAsync(attemptId);
        if (!string.IsNullOrEmpty(raw))
        {
            Debug.Log("[DEBUG] Attempt completed: " + raw);
            PlayerPrefs.DeleteKey("practiceAttemptId");
            PlayerPrefs.Save();
            SceneManager.LoadScene("PracticeListScene");
            // optionally load menu or show success UI
        }
        else
        {
            Debug.LogError("Failed to complete attempt.");
        }
    }
}
