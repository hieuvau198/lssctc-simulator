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
        scoreText.text = "Điểm: " + manager.totalPoints;
        timerText.text = "Thời gian: " + Time.timeSinceLevelLoad.ToString("F1") + "s";
    }
}
