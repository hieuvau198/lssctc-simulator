using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

public class ControlHintUI : MonoBehaviour
{
    public static ControlHintUI Instance;

    [Header("UI Reference")]
    public RectTransform HintContentPanel;
    public TextMeshProUGUI TextTemplate;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return; 
        }

        // Ensure the template is inactive
        if (TextTemplate != null)
        {
            TextTemplate.gameObject.SetActive(false);
        }

        // Start with the entire hint UI panel disabled
        if (HintContentPanel != null)
        {
            HintContentPanel.gameObject.SetActive(false);
        }
    }

    // CHANGE 3: The ShowHint method now accepts keys and the SelectionManager instance
    public void ShowHint(string message, string[] keyNames, SelectionManager manager)
    {
        if (HintContentPanel == null || TextTemplate == null)
        {
            Debug.LogError("ControlHintUI not fully configured. Missing HintContentPanel or TextTemplate.");
            return;
        }

        HintContentPanel.gameObject.SetActive(true);

        List<GameObject> childrenToDestroy = new List<GameObject>();
        foreach (Transform child in HintContentPanel)
        {
            
            if (child.gameObject != TextTemplate.gameObject)
            {
                childrenToDestroy.Add(child.gameObject);
            }
        }

        foreach (GameObject child in childrenToDestroy)
        {
            Destroy(child);
        }

        // Use the placeholder defined in SelectionManager
        const string ICON_PLACEHOLDER = "<ICON>";

        // Split the text hint using the placeholder
        string[] parts = message.Split(new[] { ICON_PLACEHOLDER }, StringSplitOptions.None);
        int keyIndex = 0;

        for (int i = 0; i < parts.Length; i++)
        {
            // 2. Add Text Segment
            string textPart = parts[i].Trim();
            if (!string.IsNullOrEmpty(textPart))
            {
                // Instantiate the Text Template
                TextMeshProUGUI tmp = Instantiate(TextTemplate, HintContentPanel);
                tmp.gameObject.SetActive(true);
                tmp.enabled = true;
                tmp.text = textPart;
            }

            // 3. Add Icon Prefab (Inserts the icon *before* the next text segment)
            if (keyIndex < keyNames.Length && i < parts.Length - 1)
            {
                string keyName = keyNames[keyIndex];

                // Use the SelectionManager to get the actual prefab GameObject
                GameObject prefabToInstantiate = manager.GetPrefabForControlKey(keyName);

                if (prefabToInstantiate != null)
                {
                    // Instantiate the prefab into the layout group container
                    Instantiate(prefabToInstantiate, HintContentPanel);
                }
                else
                {
                    // Fallback: If no prefab is found, show the key name as text
                    TextMeshProUGUI tmpFallback = Instantiate(TextTemplate, HintContentPanel);
                    tmpFallback.gameObject.SetActive(true);
                    tmpFallback.enabled = true;
                    tmpFallback.text = $"[{keyName}]";
                }
                keyIndex++;
            }
        }
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(HintContentPanel);
    }

    public void HideHint()
    {
        if (HintContentPanel != null)
        {
            HintContentPanel.gameObject.SetActive(false);
        }
    }
}
