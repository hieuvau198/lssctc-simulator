using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FinishAttempt : MonoBehaviour
{
    [Header("UI References")]
    public Button finishButton; // assign this in the Inspector
    //public TextMeshProUGUI finishStatusText;

    private string completeAttemptApiUrl = "https://lssctc-simulation.azurewebsites.net/api/PracticeAttempts/attempt/";

    void Start()
    {
        // Optional: Hide finish button if no attempt is active
        int attemptId = PlayerPrefs.GetInt("practiceAttemptId", 0);
        if (attemptId == 0)
        {
            //finishStatusText.text = "No active practice attempt found.";
            finishButton.interactable = false;
        }
        else
        {
            finishButton.onClick.AddListener(() =>
            {
                StartCoroutine(FinishPracticeAttempt(attemptId));
                PlayerPrefs.DeleteAll();
            });
        }
    }

    IEnumerator FinishPracticeAttempt(int attemptId)
    {
        string url = completeAttemptApiUrl + attemptId + "/complete";
        UnityWebRequest request = UnityWebRequest.Put(url, ""); // PUT with empty body
        request.SetRequestHeader("Content-Type", "application/json");

        //finishStatusText.text = "Completing attempt...";
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            //finishStatusText.text = "Practice attempt completed successfully!";
            Debug.Log("[DEBUG] Attempt completed: " + request.downloadHandler.text);

            // Optional: Clear PlayerPrefs or go back to menu
            PlayerPrefs.DeleteKey("practiceAttemptId");
            PlayerPrefs.Save();

            // Example: load a summary scene or return to main menu
            // UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
        else
        {
            //finishStatusText.text = "Failed to complete attempt: " + request.error;
            Debug.LogError(request.downloadHandler.text);
        }
    }
}
