using UnityEngine;
using TMPro;

public class CargoPositioningUI : MonoBehaviour
{
    public GameObject panel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI objectivesText;
    public TextMeshProUGUI statusText;

    public CargoPositioningManager manager;

    private bool isVisible;

    void Start()
    {
        if (panel) panel.SetActive(false);

        titleText.text = "Cargo Positioning Challenge";
        descriptionText.text =
            "Move the cargo and place it precisely inside the designated target circle on the ground.";

        objectivesText.text =
            "- Lower cargo into the target circle.\n" +
            "- Avoid poles (-5 pts each).\n" +
            "- Do not drop cargo outside the circle.\n" +
            "- Complete before time runs out.";
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isVisible = !isVisible;
            panel.SetActive(isVisible);
        }

        if (!manager) return;

        if (manager.IsCompleted)
            statusText.text = "Status: <color=#4CAF50>Completed</color>";
        else if (manager.IsFailed)
            statusText.text = "Status: <color=#FF5252>Failed</color>";
        else
            statusText.text = "Status: <color=#FFEB3B>Ongoing</color>";
    }
}
