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
            "- Keep cargo height above 1.72.\n" +
            "- Complete within 120 seconds.";
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
                statusText.text = "Status: Completed";
            else if (practiceManager.IsFailed)
                statusText.text = "Status: Failed";
            else
                statusText.text = "Status: Ongoing";
        }
    }
}
