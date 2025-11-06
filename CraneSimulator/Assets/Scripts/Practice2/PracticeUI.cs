using UnityEngine;
using TMPro;

public class PracticeUI : MonoBehaviour
{
    public ZigzagPracticeManager manager;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;

    void Update()
    {
        if (!manager) return;
        scoreText.text = "Score: " + manager.totalPoints;
        timerText.text = "Time: " + Time.timeSinceLevelLoad.ToString("F1") + "s";
    }
}
