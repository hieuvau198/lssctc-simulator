using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StarterAssets;

public class TutorialManager : MonoBehaviour
{
    public List<TutorialPage> pages;

    public GameObject tutorialPanel;

    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public Image imageHolder;

    public Button nextBtn;
    public Button prevBtn;
    public Button closeBtn;

    public KeyCode toggleKey = KeyCode.T;

    int index;
    bool isOpen;

    void Start()
    {
        nextBtn.onClick.AddListener(Next);
        prevBtn.onClick.AddListener(Prev);
        closeBtn.onClick.AddListener(CloseTutorial);

        OpenTutorial();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (isOpen) CloseTutorial();
            else OpenTutorial();
        }
    }

    void OpenTutorial()
    {
        isOpen = true;
        tutorialPanel.SetActive(true);

        FirstPersonController.CanLook = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ShowPage(0);
    }

    void CloseTutorial()
    {
        isOpen = false;
        tutorialPanel.SetActive(false);

        FirstPersonController.CanLook = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void ShowPage(int i)
    {
        index = i;
        var p = pages[i];

        titleText.text = p.title;
        descriptionText.text = p.description;

        imageHolder.gameObject.SetActive(p.image != null);
        if (p.image != null)
            imageHolder.sprite = p.image;

        prevBtn.gameObject.SetActive(i > 0);
        nextBtn.gameObject.SetActive(i < pages.Count - 1);
    }

    void Next() => ShowPage(index + 1);
    void Prev() => ShowPage(index - 1);
}
