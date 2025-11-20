using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class PracticeTaskManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject taskPanel;
    public Transform taskListContainer;
    public GameObject taskItemPrefab; // prefab with TextMeshProUGUI fields

    public List<PracticeTask> tasks = new List<PracticeTask>();
    private Dictionary<int, TextMeshProUGUI> taskStatusLabels = new Dictionary<int, TextMeshProUGUI>();

    private SelectionManager selectionManager;

    void Start()
    {
        taskPanel.SetActive(false);
        selectionManager = FindObjectOfType<SelectionManager>();
        InitializeLocalTasks();
        PopulateTaskList();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // If the panel is open, close it
            if (taskPanel.activeSelf)
            {
                taskPanel.SetActive(false);
                selectionManager?.RegisterPanelState("task", false);
            }
            else
            {
                // Check if another panel is already open
                if (selectionManager != null && selectionManager.IsAnyPanelOpen())
                {
                    Debug.Log("Cannot open task panel — another panel is already open.");
                    return;
                }

                // Open the panel
                taskPanel.SetActive(true);
                selectionManager?.RegisterPanelState("task", true);
            }
        }
    }

    private void InitializeLocalTasks()
    {
        tasks = new List<PracticeTask>
        {
            new PracticeTask { taskId = 17, componentId=3, taskName = "Inspect Hook Block", taskDescription = "Press E on the Hook to view its info.", isCompleted = false },
            new PracticeTask { taskId = 18, componentId=1, taskName = "Inspect Boom", taskDescription = "Press E on the Boom to view its info.", isCompleted = false },
            new PracticeTask { taskId = 19, componentId=5, taskName = "Inspect Out Trigger", taskDescription = "Press E on the Left Trigger.", isCompleted = false },
            new PracticeTask { taskId = 20, componentId=2, taskName = "Inspect Column", taskDescription = "Press E on the Right Trigger.", isCompleted = false },
            new PracticeTask { taskId = 21, componentId=4, taskName = "Inspect Control Panel", taskDescription = "Press E on the Control Panel.", isCompleted = false },
        };
    }

    private void PopulateTaskList()
    {
        foreach (Transform child in taskListContainer)
            Destroy(child.gameObject);

        taskStatusLabels.Clear();

        foreach (var task in tasks)
        {
            var item = Instantiate(taskItemPrefab, taskListContainer);
            var texts = item.GetComponentsInChildren<TextMeshProUGUI>();

            // Expect prefab to have 2 TMP fields: [0] name, [1] status
            texts[0].text = $"{task.taskName}\n<size=80%><color=#555555>{task.taskDescription}</color></size>";
            texts[1].text = task.isCompleted ? "<color=green>Done</color>" : "<color=red>Not Done</color>";

            taskStatusLabels[task.taskId] = texts[1];
        }
    }

    public void MarkTaskAsDone(int componentId)
    {
        var task = tasks.Find(t => t.componentId == componentId);
        if (task != null && !task.isCompleted)
        {
            task.isCompleted = true;
            taskStatusLabels[task.taskId].text = "<color=green>Done</color>";
            Debug.Log($"Task '{task.taskName}' marked as done.");
        }
    }
}
