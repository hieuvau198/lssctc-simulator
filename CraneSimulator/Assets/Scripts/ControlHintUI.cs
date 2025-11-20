using UnityEngine;
using TMPro;

public class ControlHintUI : MonoBehaviour
{
    public static ControlHintUI Instance;

    [Header("UI Reference")]
    public GameObject hintPanel;
    public TextMeshProUGUI hintText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (hintPanel != null)
            hintPanel.SetActive(false);
    }

    public void ShowHint(string message)
    {
        if (hintPanel == null || hintText == null) return;

        hintPanel.SetActive(true);
        hintText.text = message;
    }

    public void HideHint()
    {
        if (hintPanel != null)
            hintPanel.SetActive(false);
    }
}
