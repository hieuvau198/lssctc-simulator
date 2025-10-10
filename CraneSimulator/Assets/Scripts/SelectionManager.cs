using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;
public class SelectionManager : MonoBehaviour
{
    public GameObject interaction_Info_UI;
    private TextMeshProUGUI interaction_text;
    public GameObject itemInfoCard_UI; // Panel with Id, Name, Description texts
    public TextMeshProUGUI idText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public Image componentImage;

    public GameObject Player;
    public float maxInspectDistance = 3f;

    private InteractableObject currentHoveredObject;
    private bool itemInfoVisible = false;
    private Coroutine currentAnimation;

    //camera
    [Header("Crane Cameras")]
    public Camera playerCamera;
    public GameObject craneCamera;

    private bool inCraneView = false;
    private void Start()
    {
        interaction_text = interaction_Info_UI.GetComponent<TextMeshProUGUI>();
        interaction_Info_UI.SetActive(false);
        itemInfoCard_UI.SetActive(false);
        itemInfoCard_UI.transform.localScale = Vector3.zero; // Start scaled down

        //camera
        if (playerCamera != null) playerCamera.enabled = true;
        if (craneCamera != null) craneCamera.SetActive(false);
        
    }

    void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            var interactable = hit.transform.GetComponent<InteractableObject>();
            if (interactable != null)
            {
                // Check distance from player to object
                float distance = Vector3.Distance(Player.transform.position, interactable.transform.position);

                if (distance <= maxInspectDistance)
                {
                    if (currentHoveredObject != interactable)
                    {
                        ClearHighlight();
                        currentHoveredObject = interactable;
                        currentHoveredObject.Highlight(true);
                    }

                    interaction_text.text = "\nPress 'E' to inspect";
                    interaction_Info_UI.SetActive(true);

                    if (Keyboard.current.eKey.wasPressedThisFrame)
                    {
                        if (!itemInfoVisible)
                        {
                            ShowItemInfo(interactable);
                        }
                        else
                        {
                            HideItemInfo();
                        }
                    }
                    if (Keyboard.current.fKey.wasPressedThisFrame)
                    {
                        if (!inCraneView)
                            EnterCraneView();
                        else
                            ExitCraneView();
                    }
                }
                else
                {
                    // Too far → clear prompt and highlight
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

    void EnterCraneView()
    {
        inCraneView = true;
        interaction_Info_UI.SetActive(false);
        Player.SetActive(false);

        //if (playerCamera != null) playerCamera.enabled = false;
        if (craneCamera != null) craneCamera.SetActive(true);
        

        Debug.Log("Entered crane camera view");
    }

    void ExitCraneView()
    {
        inCraneView = false;
        interaction_Info_UI.SetActive(true);
        Player.SetActive(true);

        //if (playerCamera != null) playerCamera.enabled = true;
        if (craneCamera != null) craneCamera.SetActive(false);
        

        Debug.Log("Returned to player view");
    }
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
            {
                StartCoroutine(LoadImage(data.imageUrl));
            }
        }
        else
        {
            idText.text = "Error";
            nameText.text = "Could not load";
            descriptionText.text = "";
            componentImage.sprite = null; // Clear image
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
            elapsed += Time.deltaTime;
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
            elapsed += Time.deltaTime;
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
            Debug.LogError("⚠️ No UI Image assigned to SelectionManager.componentImage!");
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


}
