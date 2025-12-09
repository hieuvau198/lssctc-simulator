using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using System.Collections;
using System.Threading.Tasks;

public class SelectionManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject interactionInfoUI;
    public GameObject itemInfoCardUI;
    public TextMeshProUGUI idText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public UnityEngine.UI.Image componentImage;

    [Header("Key Icon Prefabs")]
    public GameObject ArrowUpPrefab;
    public GameObject ArrowDownPrefab;
    public GameObject ArrowLeftPrefab;
    public GameObject ArrowRightPrefab;
    private const string ICON_PLACEHOLDER = "<ICON>";
    [Header("Cameras")]
    public Camera playerCamera;
    public GameObject craneCamera;

    [Header("Player")]
    public GameObject player;
    public float maxInspectDistance = 3f;

    [Header("Practice Manager")]
    public PracticeTaskManager practiceTaskManager;

    private TextMeshProUGUI interactionText;
    private Coroutine currentAnimation;

    private InteractableObject hoveredObject;
    private InteractableObject controlledObject;
    private MonoBehaviour controlledScript;

    private bool isItemInfoVisible;
    private bool isControlMode;
    private bool isSettingsOpen;
    private bool isQuizOpen;
    private bool isTaskOpen;

   
    private void Start()
    {
        interactionText = interactionInfoUI.GetComponent<TextMeshProUGUI>();

        interactionInfoUI.SetActive(false);
        itemInfoCardUI.SetActive(false);
        itemInfoCardUI.transform.localScale = Vector3.zero;

        playerCamera.enabled = true;
        craneCamera?.SetActive(false);
    }

    private void Update()
    {
        HandleObjectSelection();
    }

    // =========================== //
    // ==== Panel Management  ==== //
    // =========================== //
    public void RegisterPanelState(string panelName, bool isOpen)
    {
        switch (panelName)
        {
            case "quiz": isQuizOpen = isOpen; break;
            case "task": isTaskOpen = isOpen; break;
            case "settings": isSettingsOpen = isOpen; break;
        }
    }

    public bool IsAnyPanelOpen()
    {
        // Settings panel doesn’t block other panels
        return isItemInfoVisible || isQuizOpen || isTaskOpen;
    }

    

    private void HandleObjectSelection()
    {
        
        if (isControlMode && Keyboard.current.fKey.wasPressedThisFrame)
        {
            ExitControlMode();
            return;
        }
        if (isItemInfoVisible && Keyboard.current.eKey.wasPressedThisFrame)
        {
            HideItemInfo();
            return;
        }
        if (isControlMode)
        {
            ClearSelection(); // remove any hovered highlight
            return;
        }
        // Prevent interaction if other panels are open
        if (IsAnyPanelOpen())
            return;

        if (!Camera.main) return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit))
        {
            ClearSelection();
            return;
        }

        InteractableObject interactable = hit.transform.GetComponent<InteractableObject>();
        if (interactable == null)
        {
            ClearSelection();
            return;
        }

        float distance = Vector3.Distance(player.transform.position, interactable.transform.position);
        if (distance > maxInspectDistance)
        {
            ClearSelection();
            return;
        }

        UpdateHoveredObject(interactable);
        interactionText.text = interactable.name;
        interactionInfoUI.SetActive(true);

        // === Press E to Inspect ===
        if (Keyboard.current.eKey.wasPressedThisFrame)
            ShowItemInfo(interactable);

        // === Press F to Control ===
        if (Keyboard.current.fKey.wasPressedThisFrame && interactable.controlScript != null)
            ToggleObjectControl(interactable);
    }
    

    // =========================== //
    // ==== Control Handling  ==== //
    // =========================== //
    private void ToggleObjectControl(InteractableObject interactable)
    {

        if (controlledScript != null)
            controlledScript.enabled = false;
        controlledObject = interactable;
        controlledScript = interactable.controlScript;

        if (controlledScript == null) return;
        player.SetActive(false);
        controlledScript.enabled = true;
        craneCamera?.SetActive(true);
        isControlMode = true;
        interactionInfoUI.SetActive(false);

        string hint = GetHintForControl(controlledScript);
        string[] keys = GetKeysForControl(controlledScript);
        ControlHintUI.Instance?.ShowHint(hint, keys, this);
    }

    private void ExitControlMode()
    {
        if (controlledScript != null)
            controlledScript.enabled = false;
        player.SetActive(true);
        craneCamera?.SetActive(false);
        controlledObject = null;
        controlledScript = null;
        isControlMode = false;

        ControlHintUI.Instance?.HideHint();
    }

    // =========================== //
    // ==== Item Info Panel  ==== //
    // =========================== //
    private async void ShowItemInfo(InteractableObject item)
    {
        interactionInfoUI.SetActive(false);
        player.SetActive(false);
        isItemInfoVisible = true;

        if (currentAnimation != null) StopCoroutine(currentAnimation);
        itemInfoCardUI.SetActive(true);
        currentAnimation = StartCoroutine(ScaleRectTransform(itemInfoCardUI.transform, Vector3.zero, Vector3.one, 0.3f));

        idText.text = "Đang tải...";
        nameText.text = descriptionText.text = "";
        componentImage.sprite = null;
        if(practiceTaskManager != null)
        {
            practiceTaskManager.MarkTaskAsDone(item.ItemCode);
        }
        var data = await ApiService.Instance.GetComponentByCodeAsync(item.GetItemCode());

        if (data != null)
        {

            idText.text = $"Mã: {data.id}";
            nameText.text = $"Tên: {data.name}";
            descriptionText.text = $"Mô tả: {data.description}";
            if (!string.IsNullOrEmpty(data.imageUrl))
                StartCoroutine(LoadImage(data.imageUrl));
        }
        else
        {
            idText.text = "Lỗi";
            nameText.text = "Không thể tải dữ liệu";
            descriptionText.text = "Vui lòng thử lại.";
        }
    }

    private void HideItemInfo()
    {
        interactionInfoUI.SetActive(true);
        player.SetActive(true);
        isItemInfoVisible = false;

        if (currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(ScaleAndDisable(itemInfoCardUI.transform, Vector3.one, Vector3.zero, 0.3f));
    }

    // =========================== //
    // ==== UI Animation  ==== //
    // =========================== //
    private IEnumerator ScaleRectTransform(Transform target, Vector3 from, Vector3 to, float duration)
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

    private IEnumerator ScaleAndDisable(Transform target, Vector3 from, Vector3 to, float duration)
    {
        yield return ScaleRectTransform(target, from, to, duration);
        itemInfoCardUI.SetActive(false);
    }

    // =========================== //
    // ==== Utility Helpers  ==== //
    // =========================== //
    private void UpdateHoveredObject(InteractableObject interactable)
    {
        if (hoveredObject == interactable) return;
        ClearHighlight();
        hoveredObject = interactable;
        hoveredObject.Highlight(true);
    }

    private void ClearHighlight()
    {
        if (hoveredObject == null) return;
        hoveredObject.Highlight(false);
        hoveredObject = null;
    }

    private void ClearSelection()
    {
        ClearHighlight();
        interactionInfoUI.SetActive(false);
    }

    private IEnumerator LoadImage(string url)
    {
        if (componentImage == null)
        {
            Debug.LogError("No componentImage assigned!");
            yield break;
        }

        using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Image load failed: " + request.error);
            yield break;
        }

        Texture2D texture = DownloadHandlerTexture.GetContent(request);
        componentImage.sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );
    }
    public GameObject GetPrefabForControlKey(string keyName)
    {
        switch (keyName)
        {
            case "UpArrow": return ArrowUpPrefab;
            case "DownArrow": return ArrowDownPrefab;
            case "LeftArrow": return ArrowLeftPrefab;
            case "RightArrow": return ArrowRightPrefab;
            default: return null;
        }
    }
    private string[] GetKeysForControl(MonoBehaviour script)
    {
        // Ensure you call .ToString() on all KeyCode properties
        return script switch
        {
            BoomForward bf => new[] { bf.extendKey.ToString(), bf.retractKey.ToString() },
            BoomController bc => new[] { bc.upBoom.ToString(), bc.downBoom.ToString() },
            RotationColumn rc => new[] { rc.leftRotationColumn.ToString(), rc.rightRotationColumn.ToString() },
            OutTriggerLeft otl => new[] { otl.extendKey.ToString(), otl.retractKey.ToString() },
            OutTriggerRight otr => new[] { otr.extendKey.ToString(), otr.retractKey.ToString() },
            HookBlockController hook => new[] { hook.dropKey.ToString(), hook.retractKey.ToString() },
            _ => new string[0]
        };
    }
    private string GetHintForControl(MonoBehaviour script)
    {
        // The ICON_PLACEHOLDER replaces the key text (e.g., "[ UpArrow ]")
        return script switch
        {
            BoomForward bf => $"{ICON_PLACEHOLDER} Đẩy cần ra  |  {ICON_PLACEHOLDER} Thu cần vào",
            BoomController bc => $"{ICON_PLACEHOLDER} Nâng cần  |  {ICON_PLACEHOLDER} Hạ cần",
            RotationColumn rc => $"{ICON_PLACEHOLDER} Quay trái  |  {ICON_PLACEHOLDER} Quay phải",
            OutTriggerLeft otl => $"{ICON_PLACEHOLDER} Mở chân chống trái  |  {ICON_PLACEHOLDER} Thu vào",
            OutTriggerRight otr => $"{ICON_PLACEHOLDER} Mở chân chống phải  |  {ICON_PLACEHOLDER} Thu vào",
            HookBlockController hook => $"{ICON_PLACEHOLDER} Hạ móc  |  {ICON_PLACEHOLDER} Nâng móc",
            _ => "Sử dụng các phím điều khiển được chỉ định để vận hành bộ phận này."
        };
    }
    //private string GetHintForControl(MonoBehaviour script)
    //{
    //    return script switch
    //    {
    //        BoomForward bf => $"[ {bf.extendKey} ] Đẩy cần ra  |  [ {bf.retractKey} ] Thu cần vào",
    //        BoomController bc => $"[ {bc.upBoom} ] Nâng cần  |  [ {bc.downBoom} ] Hạ cần",
    //        RotationColumn rc => $"[ {rc.leftRotationColumn} ] Quay trái  |  [ {rc.rightRotationColumn} ] Quay phải",
    //        OutTriggerLeft otl => $"[ {otl.extendKey} ] Mở chân chống trái  |  [ {otl.retractKey} ] Thu vào",
    //        OutTriggerRight otr => $"[ {otr.extendKey} ] Mở chân chống phải  |  [ {otr.retractKey} ] Thu vào",
    //        HookBlockController hook => $"[ {hook.dropKey} ] Hạ móc  |  [ {hook.retractKey} ] Nâng móc",
    //        _ => "Sử dụng các phím điều khiển được chỉ định để vận hành bộ phận này."
    //    };
    //}
}
