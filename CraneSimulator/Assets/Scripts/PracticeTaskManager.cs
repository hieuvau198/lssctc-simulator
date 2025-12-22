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
    private Dictionary<string, TextMeshProUGUI> taskStatusLabels = new Dictionary<string, TextMeshProUGUI>();

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
            new PracticeTask { taskCode = "TASK_01", componentCode="COMPONENT_03", taskName = "Xem thông tin Móc cẩu (Hook Block)", taskDescription = "Nhấn E vào móc cẩu để xem thông tin.", isCompleted = false },
            new PracticeTask { taskCode = "TASK_02", componentCode="COMPONENT_01", taskName = "Xem thông tin Cần cẩu (Boom)", taskDescription = "Nhấn E vào cần cẩu để xem thông tin.", isCompleted = false },
            new PracticeTask { taskCode = "TASK_03", componentCode="COMPONENT_05", taskName = "Xem thông tin Chân trụ (Outrigger)", taskDescription = "Nhấn E vào chân chống.", isCompleted = false },
            new PracticeTask { taskCode = "TASK_04", componentCode="COMPONENT_02", taskName = "Xem thông tin Trụ xoay (Column)", taskDescription = "Nhấn E vào trụ xoay để xem thông tin.", isCompleted = false },
            new PracticeTask { taskCode = "TASK_05", componentCode="COMPONENT_04", taskName = "Xem thông tin Cabin điều khiển", taskDescription = "Nhấn E vào cabin điều khiển.", isCompleted = false },
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
            texts[1].text = task.isCompleted ? "<color=green>Hoàn thành</color>" : "<color=red>Chưa hoàn thành</color>";

            taskStatusLabels[task.taskCode] = texts[1];
        }
    }

    public void MarkTaskAsDone(string componentCode)
    {
        var task = tasks.Find(t => t.componentCode == componentCode);
        if (task != null && !task.isCompleted)
        {
            task.isCompleted = true;
            taskStatusLabels[task.taskCode].text = "<color=green>Hoàn thành</color>";
            Debug.Log($"Nhiệm vụ '{task.taskName}' đã hoàn thành.");
        }
    }
}
