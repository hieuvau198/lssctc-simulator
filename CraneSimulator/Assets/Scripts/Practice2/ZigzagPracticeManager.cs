using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ZigzagPracticeManager : MonoBehaviour
{
    public enum PracticePhase
    {
        Zigzag,
        MoveToSecondTruck,
        Completed
    }
    [Header("UI")]
    public TextMeshProUGUI phaseStatusText;
    [Header("References")]
    public Transform load; // your cargo object
    public Transform hook; // optional - for swing check
    public List<Transform> waypoints;
    public Transform secondTruckTarget;
    [Header("Practice Settings")]
    public float waypointRadius = 4f;
    public float truckTargetRadius = 1f;
    public float maxSwingAngle = 15f;
    public float maxTime = 120f;
    public float groundY = 0f; // the Y height of the ground plane

    [Header("Scoring")]
    public float totalPoints = 10;
    public float polePenalty = 0.5f;

    private int currentWaypoint = 0;
    private float timer = 0f;
    private bool isActive = false;
    private bool isFailed = false;
    private bool isCompleted = false;

    private int poleHitMistakes = 0;
    private PracticePhase currentPhase = PracticePhase.Zigzag;
    void Start()
    {
        // Automatically start when the scene is loaded
        StartPractice();
    }

    void Update()
    {
        if (!isActive || isFailed || isCompleted) return;

        timer += Time.deltaTime;

        // --- Time limit ---
        if (timer > maxTime)
        {
            FailPractice("Vượt quá thời gian cho phép!");
            return;
        }

        //// --- Reaching next waypoint ---
        //float dist = Vector3.Distance(load.position, waypoints[currentWaypoint].position);
        //if (dist < waypointRadius)
        //{
        //    Debug.Log($"Waypoint {currentWaypoint + 1} reached!");
        //    currentWaypoint++;
        //    if (currentWaypoint >= waypoints.Count)
        //    {
        //        SuccessPractice();
        //        return;
        //    }
        //}

        // --- Cargo touches ground ---
        if (load.position.y <= groundY + 0.1f) // small offset for collider bottom
        {
            FailPractice("Hàng đã chạm mặt đất!");
        }
        switch (currentPhase)
        {
            case PracticePhase.Zigzag:
                HandleZigzagPhase();
                break;

            case PracticePhase.MoveToSecondTruck:
                HandleSecondTruckPhase();
                break;
        }
    }
    private void HandleZigzagPhase()
    {
        float dist = Vector3.Distance(load.position, waypoints[currentWaypoint].position);
        if (dist < waypointRadius)
        {
            Debug.Log($"Waypoint {currentWaypoint + 1} reached!");
            currentWaypoint++;

            if (currentWaypoint >= waypoints.Count)
            {
                Debug.Log("Zigzag completed! Move cargo to the second truck.");
                currentPhase = PracticePhase.MoveToSecondTruck;
                UpdatePhaseText();
            }
        }
    }

    // ================= PHASE 2 =================
    private void HandleSecondTruckPhase()
    {
        float dist = Vector3.Distance(load.position, secondTruckTarget.position);
        if (dist < truckTargetRadius)
        {
            SuccessPractice();
        }
    }
    public void StartPractice()
    {
        poleHitMistakes = 0;
        isActive = true;
        isFailed = false;
        isCompleted = false;
        currentWaypoint = 0;
        timer = 0f;
        currentPhase = PracticePhase.Zigzag;
        UpdatePhaseText();
        Debug.Log("Zigzag practice started!");
    }

    public void DeductPoints(float amount)
    {
        poleHitMistakes++;
        totalPoints -= amount;
        Debug.Log($"Hit pole! -{amount} points. Current Score: {totalPoints}");
        if (totalPoints < 0)
        {
            totalPoints = 0;
        }
    }

    public void FailPractice(string reason)
    {
        if (isFailed) return;
        isFailed = true;
        isActive = false;
        UpdatePhaseText($"<color=red>THẤT BẠI:</color> {reason}");
        Debug.Log($"Practice failed: {reason} | Final Score: {totalPoints}");
    }

    public void SuccessPractice()
    {
        if (isCompleted) return;
        isActive = false;
        isCompleted = true;
        UpdatePhaseText();
        Debug.Log($"Practice completed successfully in {timer:F1}s! Final Score: {totalPoints}");
    }
    private void UpdatePhaseText(string customMessage = "")
    {
        if (phaseStatusText == null) return;

        if (!string.IsNullOrEmpty(customMessage))
        {
            phaseStatusText.text = customMessage;
            return;
        }

        switch (currentPhase)
        {
            case PracticePhase.Zigzag:
                phaseStatusText.text =
                    "<color=yellow>Giai đoạn 1:</color> Di chuyển zigzag – <b>Đang thực hiện</b>";
                break;

            case PracticePhase.MoveToSecondTruck:
                phaseStatusText.text =
                    "<color=yellow>Giai đoạn 2:</color> Đưa hàng lên xe tải thứ hai";
                break;

            case PracticePhase.Completed:
                phaseStatusText.text =
                    "<color=green>HOÀN THÀNH:</color> <b>ĐẠT</b>";
                break;
        }
    }
    public bool IsCompleted => isCompleted;
    public bool IsFailed => isFailed;
    public bool IsActive => isActive;
    public PracticePhase CurrentPhase => currentPhase;
    public int TotalMistakes => poleHitMistakes;
}
