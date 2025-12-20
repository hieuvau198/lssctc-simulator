using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using StarterAssets;

public class OptionsMenu : MonoBehaviour
{
    [Header("UI References")]
    public Slider sensitivitySlider;
    public TextMeshProUGUI sensitivityValueText;
    public Slider soundSlider;
    public TextMeshProUGUI soundValueText;

    [Header("Audio Sources")]
    public List<AudioSource> audioSources = new List<AudioSource>();

    [Header("Player Controller")]
    public FirstPersonController playerController;

    [Header("Default Settings")]
    [Range(0.1f, 10f)] public float defaultSensitivity = 1f;
    [Range(0f, 1f)] public float defaultSoundVolume = 1f;

    // Saved values
    private float savedSensitivity;
    private float savedSound;

    // Preview values
    private float previewSensitivity;
    private float previewSound;

    private void Start()
    {
        // Load SAVED values
        savedSensitivity = PlayerPrefs.GetFloat("sensitivity", defaultSensitivity);
        savedSound = PlayerPrefs.GetFloat("soundVolume", defaultSoundVolume);

        // Set sliders to saved values
        sensitivitySlider.value = savedSensitivity;
        soundSlider.value = savedSound;

        // Apply saved values
        ApplySensitivity(savedSensitivity);
        ApplySound(savedSound);

        UpdateSensitivityText(savedSensitivity);
        UpdateSoundText(savedSound);

        // Listeners (PREVIEW only)
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityPreview);
        soundSlider.onValueChanged.AddListener(OnSoundPreview);
    }

    // ===================== PREVIEW =====================

    private void OnSensitivityPreview(float value)
    {
        previewSensitivity = value;
        ApplySensitivity(previewSensitivity);
        UpdateSensitivityText(previewSensitivity);
    }

    private void OnSoundPreview(float value)
    {
        previewSound = value;
        ApplySound(previewSound);
        UpdateSoundText(previewSound);
    }

    // ===================== APPLY =====================

    private void ApplySensitivity(float value)
    {
        if (playerController != null)
        {
            playerController.RotationSpeed = value;
        }
    }

    private void ApplySound(float value)
    {
        foreach (AudioSource source in audioSources)
        {
            if (source != null)
            {
                source.volume = value;
            }
        }
    }

    // ===================== SAVE =====================

    public void SaveChanges()
    {
        savedSensitivity = sensitivitySlider.value;
        savedSound = soundSlider.value;

        PlayerPrefs.SetFloat("sensitivity", savedSensitivity);
        PlayerPrefs.SetFloat("soundVolume", savedSound);
        PlayerPrefs.Save();

        Debug.Log("Options saved.");
    }

    // ===================== CANCEL / CLOSE =====================

    public void CancelChanges()
    {
        // Revert to saved values
        sensitivitySlider.value = savedSensitivity;
        soundSlider.value = savedSound;

        ApplySensitivity(savedSensitivity);
        ApplySound(savedSound);

        UpdateSensitivityText(savedSensitivity);
        UpdateSoundText(savedSound);

        Debug.Log("Options reverted.");
    }

    // ===================== UI TEXT =====================

    private void UpdateSensitivityText(float value)
    {
        sensitivityValueText.text = $"{value:F2}";
    }

    private void UpdateSoundText(float value)
    {
        soundValueText.text = $"{Mathf.RoundToInt(value * 100)}%";
    }
}
