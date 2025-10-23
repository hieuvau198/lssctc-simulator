using UnityEngine;

public class SettingsPanelController : MonoBehaviour
{
    [Header("Settings Panel Reference")]
    public GameObject settingsPanel;

    private bool isOpen = false;

    void Start()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false); // Hide by default
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettings();
        }
    }

    public void ToggleSettings()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning("Settings Panel not assigned!");
            return;
        }

        isOpen = !isOpen;
        settingsPanel.SetActive(isOpen);

        // Pause or resume the game (optional)
        Time.timeScale = isOpen ? 0f : 1f;

        // Optional: Lock or unlock the cursor
        Cursor.lockState = isOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isOpen;
    }

    public void CloseSettings()
    {
        if (settingsPanel == null) return;

        isOpen = false;
        settingsPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
