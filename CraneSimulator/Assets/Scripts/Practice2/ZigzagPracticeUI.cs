using UnityEngine;
using TMPro;

public class ZigzagPracticeUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI objectivesText;
    public TextMeshProUGUI statusText;

    [Header("References")]
    public ZigzagPracticeManager practiceManager;

    private bool isVisible = false;

    void Start()
    {
        if (panel != null)
            panel.SetActive(false);

        titleText.text = "Zigzag Cargo Practice";
        descriptionText.text = "Carefully move the cargo through each waypoint in a zigzag pattern without touching the ground or hitting obstacles.";
        objectivesText.text =
            "- Reach all waypoints in order.\n" +
            "- Avoid hitting poles (-5 pts each).\n" +
            "- Keep cargo height above the ground.";
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isVisible = !isVisible;
            panel.SetActive(isVisible);
        }

        if (practiceManager != null)
        {
            if (practiceManager.IsCompleted)
                statusText.text = "Status: "+ "<color=#4CAF50>Completed</color>";
            else if (practiceManager.IsFailed)
                statusText.text = "Status: "+ "<color=#FF5252>Failed</color>";
            else
                statusText.text = "Status: "+ "<color=#FFEB3B>Ongoing</color>";
        }
    }
}
