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
        titleText.text = "Thực Hành Vận Chuyển Hàng Zigzag";
        descriptionText.text =
            "Di chuyển hàng cẩn thận qua từng điểm đánh dấu theo mô hình zigzag, "
            + "không để chạm đất hoặc va vào chướng ngại vật.";

        objectivesText.text =
            "- Đi qua tất cả các điểm theo đúng thứ tự.\n" +
            "- Tránh va vào cột (-0.5 điểm mỗi lần).\n" +
            "- Giữ hàng cách mặt đất (không để chạm xuống).";
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
                statusText.text = "Trạng thái: " + "<color=#4CAF50>Hoàn thành</color>";
            else if (practiceManager.IsFailed)
                statusText.text = "Trạng thái: " + "<color=#FF5252>Thất bại</color>";
            else
                statusText.text = "Trạng thái: " + "<color=#FFEB3B>Đang thực hiện</color>";
        }
    }
}
