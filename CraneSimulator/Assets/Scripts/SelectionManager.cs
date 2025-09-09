using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections;

public class SelectionManager : MonoBehaviour
{
    public GameObject interaction_Info_UI;
    private TextMeshProUGUI interaction_text;
    public GameObject itemInfoCard_UI; // Panel with Id, Name, Description texts
    public TextMeshProUGUI idText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public GameObject Player;
    private InteractableObject currentHoveredObject;
    private bool itemInfoVisible = false;

    private Coroutine currentAnimation;

    private void Start()
    {
        interaction_text = interaction_Info_UI.GetComponent<TextMeshProUGUI>();
        interaction_Info_UI.SetActive(false);
        itemInfoCard_UI.SetActive(false);
        itemInfoCard_UI.transform.localScale = Vector3.zero; // Start scaled down
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
                if (currentHoveredObject != interactable)
                {
                    ClearHighlight();
                    currentHoveredObject = interactable;
                    currentHoveredObject.Highlight(true);
                }
                interaction_text.text = interactable.GetItemName() + "\nPress 'E' to inspect";
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

    void ShowItemInfo(InteractableObject item)
    {
        idText.text = "ID: " + item.GetItemID();
        nameText.text = "Name: " + item.GetItemName();
        descriptionText.text = "Description: " + item.GetItemDescription();

        interaction_Info_UI.SetActive(false);
        Player.SetActive(false);
        itemInfoVisible = true;

        if (currentAnimation != null)
            StopCoroutine(currentAnimation);
        itemInfoCard_UI.SetActive(true);
        currentAnimation = StartCoroutine(ScaleRectTransform(itemInfoCard_UI.transform, Vector3.zero, Vector3.one, 0.3f));
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
}
