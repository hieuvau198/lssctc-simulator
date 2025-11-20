using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsMenu : MonoBehaviour
{
    [Header("UI References")]
    public Slider sensitivitySlider;
    public TextMeshProUGUI sensitivityValueText;
    public Slider soundSlider;
    public TextMeshProUGUI soundValueText;

    [Header("Settings")]
    [Range(0.1f, 10f)] public float defaultSensitivity = 1f;
    [Range(0f, 1f)] public float defaultSoundVolume = 1f;

    private void Start()
    {
        // Load saved preferences
        float savedSensitivity = PlayerPrefs.GetFloat("sensitivity", defaultSensitivity);
        float savedSound = PlayerPrefs.GetFloat("soundVolume", defaultSoundVolume);

        sensitivitySlider.value = savedSensitivity;
        soundSlider.value = savedSound;

        UpdateSensitivityText(savedSensitivity);
        UpdateSoundText(savedSound);

        // Listeners
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        soundSlider.onValueChanged.AddListener(OnSoundChanged);
    }

    private void OnSensitivityChanged(float value)
    {
        UpdateSensitivityText(value);
        PlayerPrefs.SetFloat("sensitivity", value);
    }

    private void OnSoundChanged(float value)
    {
        UpdateSoundText(value);
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("soundVolume", value);
    }

    private void UpdateSensitivityText(float value)
    {
        sensitivityValueText.text = $"{value:F2}";
    }

    private void UpdateSoundText(float value)
    {
        soundValueText.text = $"{(int)(value * 100)}%";
    }

    public void ApplySettings()
    {
        PlayerPrefs.Save();
        Debug.Log("Settings saved.");
    }
}
