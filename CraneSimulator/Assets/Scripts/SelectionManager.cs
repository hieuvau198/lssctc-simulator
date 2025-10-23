using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class SelectionManager : MonoBehaviour
{
    // === UI ===
    [Header("UI References")]
    public GameObject interaction_Info_UI;
    private TextMeshProUGUI interaction_text;
    public GameObject itemInfoCard_UI; // Panel with Id, Name, Description texts
    public TextMeshProUGUI idText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public Image componentImage;

    [Header("Settings Panel")]
    public GameObject settingsPanel; // Assign in Inspector
    private bool settingsOpen = false;

    // === Player ===
    public GameObject Player;
    public float maxInspectDistance = 3f;

    // === Interaction ===
    private InteractableObject currentHoveredObject;
    private bool itemInfoVisible = false;
    private Coroutine currentAnimation;

    // === Control ===
    private InteractableObject currentControlledObject;
    private MonoBehaviour currentControlledScript;
    private bool inControlMode = false;

    // === Camera ===
    [Header("Crane Cameras")]
    public Camera playerCamera;
    public GameObject craneCamera;

    private void Start()
    {
        interaction_text = interaction_Info_UI.GetComponent<TextMeshProUGUI>();
        interaction_Info_UI.SetActive(false);
        itemInfoCard_UI.SetActive(false);
        itemInfoCard_UI.transform.localScale = Vector3.zero;

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        // Setup cameras
        if (playerCamera != null) playerCamera.enabled = true;
        if (craneCamera != null) craneCamera.SetActive(false);
    }

    void Update()
    {
        // --- SETTINGS MENU TOGGLE ---
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // If inspecting or controlling, close those first
            if (itemInfoVisible)
            {
                HideItemInfo();
                return;
            }
            else if (inControlMode)
            {
                ExitControlMode();
                return;
            }
            else
            {
                ToggleSettingsPanel();
                return;
            }
        }

        if (settingsOpen || itemInfoVisible || inControlMode)
            return;

        // --- Raycast for interactions ---
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            var interactable = hit.transform.GetComponent<InteractableObject>();
            if (interactable != null)
            {
                float distance = Vector3.Distance(Player.transform.position, interactable.transform.position);

                if (distance <= maxInspectDistance)
                {
                    if (currentHoveredObject != interactable)
                    {
                        ClearHighlight();
                        currentHoveredObject = interactable;
                        currentHoveredObject.Highlight(true);
                    }

                    interaction_text.text = "Press 'E' to inspect";
                    if (interactable.controlScript != null)
                        interaction_text.text += "\nPress 'F' to control";

                    interaction_Info_UI.SetActive(true);

                    // --- Inspect Info ---
                    if (Keyboard.current.eKey.wasPressedThisFrame)
                    {
                        if (!itemInfoVisible)
                            ShowItemInfo(interactable);
                        else
                            HideItemInfo();
                        StartCoroutine(SubmitStepProgress(interactable.GetItemID(), "I"));
                    }

                    // --- Control Mode (if available) ---
                    if (Keyboard.current.fKey.wasPressedThisFrame && interactable.controlScript != null)
                    {
                        ToggleObjectControl(interactable);
                        StartCoroutine(SubmitStepProgress(interactable.GetItemID(), "F"));
                    }
                }
                else
                {
                    ClearSelection();
                }
            }
            else
            {
                ClearSelection();
            }
        }
        else
        {
            ClearSelection();
        }
    }

    // === SETTINGS ===
    void ToggleSettingsPanel()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning("Settings Panel not assigned!");
            return;
        }

        settingsOpen = !settingsOpen;
        settingsPanel.SetActive(settingsOpen);

        // Pause or resume the game
        Time.timeScale = settingsOpen ? 0f : 1f;

        // Unlock or lock the cursor
        Cursor.lockState = settingsOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = settingsOpen;

        // Disable player camera control while in settings
        Player.SetActive(!settingsOpen);
        interaction_Info_UI.SetActive(!settingsOpen);

        Debug.Log(settingsOpen ? "Settings opened" : "Settings closed");
    }

    // === Control Logic ===
    void ToggleObjectControl(InteractableObject interactable)
    {
        if (currentControlledObject == interactable)
        {
            ExitControlMode();
            return;
        }

        if (currentControlledScript != null)
            currentControlledScript.enabled = false;

        currentControlledObject = interactable;
        currentControlledScript = interactable.controlScript;

        if (currentControlledScript != null)
        {
            currentControlledScript.enabled = true;
            Player.SetActive(false);
            if (craneCamera != null) craneCamera.SetActive(true);
            inControlMode = true;
            interaction_Info_UI.SetActive(false);
            Debug.Log($"Now controlling: {interactable.name}");
        }
    }

    void ExitControlMode()
    {
        if (currentControlledScript != null)
            currentControlledScript.enabled = false;

        Player.SetActive(true);
        if (craneCamera != null) craneCamera.SetActive(false);

        currentControlledObject = null;
        currentControlledScript = null;
        inControlMode = false;

        Debug.Log("Exited control mode");
    }

    // === Inspect Info ===
    async void ShowItemInfo(InteractableObject item)
    {
        interaction_Info_UI.SetActive(false);
        Player.SetActive(false);
        itemInfoVisible = true;

        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
        itemInfoCard_UI.SetActive(true);
        currentAnimation = StartCoroutine(ScaleRectTransform(itemInfoCard_UI.transform, Vector3.zero, Vector3.one, 0.3f));

        idText.text = "Loading...";
        nameText.text = "";
        descriptionText.text = "";

        var apiService = FindObjectOfType<ApiService>();
        if (apiService == null)
        {
            Debug.LogError("No ApiService found in scene!");
            return;
        }

        int componentId = item.GetItemID();
        ComponentDto data = await apiService.GetComponentByIdAsync(componentId);

        if (data != null)
        {
            idText.text = "ID: " + data.id;
            nameText.text = "Name: " + data.name;
            descriptionText.text = "Description: " + data.description;
            if (!string.IsNullOrEmpty(data.imageUrl))
                StartCoroutine(LoadImage(data.imageUrl));
        }
        else
        {
            idText.text = "Error";
            nameText.text = "Could not load";
            descriptionText.text = "";
            componentImage.sprite = null;
        }
    }

    void HideItemInfo()
    {
        interaction_Info_UI.SetActive(true);
        Player.SetActive(true);
        itemInfoVisible = false;

        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(ScaleAndDisable(itemInfoCard_UI.transform, Vector3.one, Vector3.zero, 0.3f));
    }

    IEnumerator ScaleRectTransform(Transform target, Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            target.localScale = Vector3.Lerp(from, to, elapsed / duration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        target.localScale = to;
    }

    IEnumerator ScaleAndDisable(Transform target, Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            target.localScale = Vector3.Lerp(from, to, elapsed / duration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        target.localScale = to;
        itemInfoCard_UI.SetActive(false);
    }

    void ClearHighlight()
    {
        if (currentHoveredObject != null)
        {
            currentHoveredObject.Highlight(false);
            currentHoveredObject = null;
        }
    }

    void ClearSelection()
    {
        ClearHighlight();
        interaction_Info_UI.SetActive(false);
        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
        itemInfoCard_UI.SetActive(false);
        itemInfoCard_UI.transform.localScale = Vector3.zero;
        itemInfoVisible = false;
    }

    IEnumerator LoadImage(string url)
    {
        if (componentImage == null)
        {
            Debug.LogError("No UI Image assigned to SelectionManager.componentImage!");
            yield break;
        }

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Image load failed: " + request.error);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f)
                );
                componentImage.sprite = sprite;
            }
        }
    }

    // === Submit Step API ===
    IEnumerator SubmitStepProgress(int componentId, string actionKey)
    {
        int attemptId = PlayerPrefs.GetInt("practiceAttemptId", 0);
        int userId = PlayerPrefs.GetInt("UserID", 0);

        var quizManager = FindObjectOfType<PracticeQuizManager>();
        if (quizManager == null)
        {
            Debug.LogError("PracticeQuizManager not found in scene!");
            yield break;
        }

        int currentStepId = quizManager.CurrentStepId;
        if (attemptId == 0 || userId == 0 || currentStepId == -1)
        {
            Debug.LogError("Missing attemptId, userId, or currentStepId!");
            yield break;
        }

        string apiUrl = $"https://lssctc-simulation.azurewebsites.net/api/TraineePractices/attempt/{attemptId}/trainee/{userId}/submit";
        Debug.Log($"Submitting step: {apiUrl}");

        string jsonBody = JsonUtility.ToJson(new SubmitStepData
        {
            currentStepId = currentStepId,
            componentId = componentId,
            actionKey = actionKey
        });

        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Submit failed: {request.error}\nResponse: {request.downloadHandler.text}");
                yield break;
            }

            string response = request.downloadHandler.text.Trim();
            Debug.Log($"Submit response: {response}");

            if (response.Contains("\"message\":\"Trainee step attempt submitted successfully.\""))
            {
                Debug.Log("Step submission confirmed by server.");
                quizManager.MarkStepAsDone();
            }
            else
            {
                Debug.LogWarning(" Step not marked done — server did not return success message.");
            }
        }
    }

    [System.Serializable]
    public class SubmitStepData
    {
        public int currentStepId;
        public int componentId;
        public string actionKey;
    }
}
