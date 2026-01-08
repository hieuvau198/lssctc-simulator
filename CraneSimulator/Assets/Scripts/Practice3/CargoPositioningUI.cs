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

        titleText.text = "Thử thách đặt hàng hóa";
        descriptionText.text =
            "Di chuyển hàng hóa và đặt chính xác vào vòng tròn mục tiêu trên mặt đất.";

        objectivesText.text =
           "- Hạ hàng hóa vào đúng vòng tròn mục tiêu.\n" +
            "- Tránh va chạm cột (-0.5 điểm mỗi lần).\n" +
            "- Không được thả hàng ra ngoài vòng tròn.\n" +
            "- Hoàn thành trước khi hết thời gian.";
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
