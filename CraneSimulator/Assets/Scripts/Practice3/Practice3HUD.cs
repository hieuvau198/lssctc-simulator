using UnityEngine;
using TMPro;

public class Practice3HUD : MonoBehaviour
{
    public CargoPositioningManager manager;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;

    void Update()
    {
        if (!manager) return;

        scoreText.text = "Score: " + manager.totalPoints;
        timerText.text = "Time: " + manager.Timer.ToString("F1") + "s";
    }
}
