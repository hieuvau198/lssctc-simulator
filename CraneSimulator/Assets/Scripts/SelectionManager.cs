using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Collections;

public class SelectionManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject interaction_Info_UI;
    private TextMeshProUGUI interaction_text;
    public GameObject itemInfoCard_UI;
    public TextMeshProUGUI idText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public UnityEngine.UI.Image componentImage;

    [Header("Settings Panel")]
    public GameObject settingsPanel;
    private bool settingsOpen = false;

    [Header("Crane Cameras")]
    public Camera playerCamera;
    public GameObject craneCamera;

    [Header("Player")]
    public GameObject Player;
    public float maxInspectDistance = 3f;

    private InteractableObject currentHoveredObject;
    private bool itemInfoVisible = false;
    private Coroutine currentAnimation;

    private InteractableObject currentControlledObject;
    private MonoBehaviour currentControlledScript;
    private bool inControlMode = false;

    // === PANEL MANAGEMENT ===
    private bool quizOpen = false;
    public void RegisterPanelState(string panelName, bool open)
    {
        if (panelName == "quiz") quizOpen = open;
    }

    public bool IsAnyPanelOpen()
    {
        return settingsOpen || itemInfoVisible || inControlMode || quizOpen;
    }
    private void Start()
    {
        interaction_text = interaction_Info_UI.GetComponent<TextMeshProUGUI>();
        interaction_Info_UI.SetActive(false);
        itemInfoCard_UI.SetActive(false);
        itemInfoCard_UI.transform.localScale = Vector3.zero;

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (playerCamera != null) playerCamera.enabled = true;
        if (craneCamera != null) craneCamera.SetActive(false);
    }

    void Update()
    {
        HandleEscapeKey();

        
        if (IsAnyPanelOpen())
            return;

        HandleObjectSelection();
    }

    // === ESC Key Logic ===
    void HandleEscapeKey()
    {
        if (!Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

        if (itemInfoVisible)
        {
            HideItemInfo();
        }
        else if (inControlMode)
        {
            ExitControlMode();
        }
        else
        {
            ToggleSettingsPanel();
        }
    }

    // === Object Interaction ===
    void HandleObjectSelection()
    {
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
                    UpdateHoveredObject(interactable);
                    interaction_text.text = interactable.name;
                    interaction_Info_UI.SetActive(true);

                    if (Keyboard.current.eKey.wasPressedThisFrame)
                    {
                        ToggleItemInfo(interactable);
                        //_ = SubmitStepProgress(interactable.GetItemID(), "I");
                    }

                    if (Keyboard.current.fKey.wasPressedThisFrame && interactable.controlScript != null)
                    {
                        ToggleObjectControl(interactable);
                        //_ = SubmitStepProgress(interactable.GetItemID(), "F");
                    }
                }
                else ClearSelection();
            }
            else ClearSelection();
        }
        else ClearSelection();
    }

    void UpdateHoveredObject(InteractableObject interactable)
    {
        if (currentHoveredObject != interactable)
        {
            ClearHighlight();
            currentHoveredObject = interactable;
            currentHoveredObject.Highlight(true);
        }
    }

    // === Toggle Settings ===
    void ToggleSettingsPanel()
    {

        if (IsAnyPanelOpen() && !settingsOpen) return;
        // Close other panels first
        if (itemInfoVisible) HideItemInfo();
        if (inControlMode) ExitControlMode();

        settingsOpen = !settingsOpen;
        settingsPanel.SetActive(settingsOpen);

        Time.timeScale = settingsOpen ? 0f : 1f;
        Cursor.lockState = settingsOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = settingsOpen;

        Player.SetActive(!settingsOpen);
        interaction_Info_UI.SetActive(!settingsOpen);
    }

    // === Toggle Object Control ===
    void ToggleObjectControl(InteractableObject interactable)
    {
        if (IsAnyPanelOpen() && !inControlMode)
            return;

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

            string hint = GetHintForControl(currentControlledScript);
            ControlHintUI.Instance?.ShowHint(hint);
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

        ControlHintUI.Instance?.HideHint();
    }

    // === Toggle Item Info ===
    void ToggleItemInfo(InteractableObject item)
    {
        if (IsAnyPanelOpen() && !itemInfoVisible)
            return;

        if (!itemInfoVisible)
            ShowItemInfo(item);
        else
            HideItemInfo();
    }

    async void ShowItemInfo(InteractableObject item)
    {
        // Close other panels
        if (settingsOpen) ToggleSettingsPanel();
        if (inControlMode) ExitControlMode();

        interaction_Info_UI.SetActive(false);
        Player.SetActive(false);
        itemInfoVisible = true;

        if (currentAnimation != null) StopCoroutine(currentAnimation);
        itemInfoCard_UI.SetActive(true);
        currentAnimation = StartCoroutine(ScaleRectTransform(itemInfoCard_UI.transform, Vector3.zero, Vector3.one, 0.3f));

        idText.text = "Loading...";
        nameText.text = "";
        descriptionText.text = "";
        componentImage.sprite = null;

        int componentId = item.GetItemID();
        var data = await ApiService.Instance.GetComponentByIdAsync(componentId);

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
        }
    }

    void HideItemInfo()
    {
        interaction_Info_UI.SetActive(true);
        Player.SetActive(true);
        itemInfoVisible = false;

        if (currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(ScaleAndDisable(itemInfoCard_UI.transform, Vector3.one, Vector3.zero, 0.3f));
    }

    // === Helpers ===
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
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                componentImage.sprite = sprite;
            }
        }
    }
    string GetHintForControl(MonoBehaviour controlScript)
    {
        if (controlScript is BoomForward bf)
            return $"[ {bf.extendKey} ] Extend  |  [ {bf.retractKey} ] Retract";
        if (controlScript is BoomController bc)
            return $"[ {bc.upBoom} ] Boom Up  |  [ {bc.downBoom} ] Boom Down";
        if (controlScript is RotationColumn rc)
            return $"[ {rc.leftRotationColumn} ] Rotate Left  |  [ {rc.rightRotationColumn} ] Rotate Right";
        if (controlScript is OutTriggerLeft otl)
            return $"[ {otl.extendKey} ] Extend Left Trigger  |  [ {otl.retractKey} ] Retract";
        if (controlScript is OutTriggerRight otr)
            return $"[ {otr.extendKey} ] Extend Right Trigger  |  [ {otr.retractKey} ] Retract";
        if (controlScript is HookBlockController hook)
            return $"[ {hook.dropKey} ] Drop Hook  |  [ {hook.retractKey} ] Raise Hook";

        return "Use assigned control keys to operate this component.";
    }

    // === Submit Step ===
    private async Task SubmitStepProgress(int componentId, string actionKey)
    {
        int attemptId = PlayerPrefs.GetInt("practiceAttemptId", 0);
        int userId = PlayerPrefs.GetInt("UserID", 0);

        var quizManager = FindObjectOfType<PracticeQuizManager>();
        if (quizManager == null)
        {
            Debug.LogError("PracticeQuizManager not found in scene!");
            return;
        }

        int currentStepId = quizManager.CurrentStepId;
        if (attemptId == 0 || userId == 0 || currentStepId == -1)
        {
            Debug.LogError("Missing attemptId, userId, or currentStepId!");
            return;
        }

        var dto = new SubmitStepData
        {
            currentStepId = currentStepId,
            componentId = componentId,
            actionKey = actionKey
        };

        string raw = await ApiService.Instance.SubmitStepAsync(attemptId, userId, dto);
        if (string.IsNullOrEmpty(raw))
        {
            Debug.LogError("Submit failed or empty response");
            return;
        }

        if (raw.Contains("\"message\":\"Trainee step attempt submitted successfully.\""))
        {
            Debug.Log("Step submission confirmed by server.");
            quizManager.MarkStepAsDone();
        }
        else
        {
            Debug.LogWarning("Step not marked done — server did not return success message. Response: " + raw);
        }
    }
}
