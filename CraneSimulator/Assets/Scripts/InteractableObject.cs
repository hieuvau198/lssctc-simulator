using UnityEngine;
using UnityEngine.UI;

public class InteractableObject : MonoBehaviour
{
    public string ItemID;
    public string ItemName;
    public string ItemDescription;

    //private Renderer objectRenderer;
    private Outline outline;
    //public Color highlightColor = Color.yellow;

    private void Start()
    {
        //objectRenderer = GetComponent<Renderer>();
        //if (objectRenderer != null)
        //    originalColor = objectRenderer.material.color;


        outline = GetComponent<Outline>();
        if (outline != null)
            outline.enabled = false;
    }

    public void Highlight(bool enable)
    {
        //if (objectRenderer != null)
        //{
        //    objectRenderer.material.color = enable ? highlightColor : originalColor;
        //}
        if (outline != null)
        {
            outline.enabled = enable;
        }
    }

    public string GetItemName()
    {
        return ItemName;
    }

    public string GetItemID()
    {
        return ItemID;
    }

    public string GetItemDescription()
    {
        return ItemDescription;
    }
}
