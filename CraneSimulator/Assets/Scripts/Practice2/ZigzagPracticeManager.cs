using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Linq;

public class ZigzagPracticeManager : MonoBehaviour
{
    public enum PracticePhase
    {
        Zigzag = 0,
        MoveToSecondTruck = 1,
        Completed = 2
    }
    [Header("UI")]
    public TextMeshProUGUI phaseStatusText;
    [Header("References")]
    public Transform load; // your cargo object
    public Collider loadCollider;
    public Transform hook; // optional - for swing check
    public List<BoxCollider> waypoints;
    public BoxCollider secondTruckTarget;
    [Header("Practice Settings")]
    public float waypointRadius = 4f;
    public float truckTargetRadius = 1f;
    public float maxSwingAngle = 15f;
    public float maxTime = 300f;
    public float groundY = 0f; // the Y height of the ground plane

    [Header("Scoring")]
    public float totalPoints = 10;
    public float polePenalty = 0.5f;

    private int[] mistakesPerPhase = new int[2];
    private float phaseStartTime;
    private float[] phaseDurations = new float[2];
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
    //private void HandleZigzagPhase()
    //{
    //    float dist = Vector3.Distance(load.position, waypoints[currentWaypoint].position);
    //    if (dist < waypointRadius)
    //    {
    //        Debug.Log($"Waypoint {currentWaypoint + 1} reached!");
    //        currentWaypoint++;

    //        if (currentWaypoint >= waypoints.Count)
    //        {
    //            phaseDurations[0] =
    //            Time.timeSinceLevelLoad - phaseStartTime;
    //            currentPhase = PracticePhase.MoveToSecondTruck;
    //            phaseStartTime = Time.timeSinceLevelLoad;
    //            UpdatePhaseText();
    //        }
    //    }
    //}
    private void HandleZigzagPhase()
    {
        if (currentWaypoint >= waypoints.Count)
            return;

        if (IsOverlappingBox(waypoints[currentWaypoint]))
        {
            Debug.Log($"Waypoint {currentWaypoint + 1} reached!");

            currentWaypoint++;

            if (currentWaypoint >= waypoints.Count)
            {
                phaseDurations[0] = Time.timeSinceLevelLoad - phaseStartTime;
                currentPhase = PracticePhase.MoveToSecondTruck;
                phaseStartTime = Time.timeSinceLevelLoad;
                UpdatePhaseText();
            }
        }
    }

    // ================= PHASE 2 =================
    private void HandleSecondTruckPhase()
    {
        //float dist = Vector3.Distance(load.position, secondTruckTarget.position);
        //if (dist < truckTargetRadius)
        //{
        //    phaseDurations[1] =
        //    Time.timeSinceLevelLoad - phaseStartTime;
        //    currentPhase = PracticePhase.Completed;
        //    SuccessPractice();
        //}
        if (IsOverlappingBox(secondTruckTarget))
        {
            phaseDurations[1] = Time.timeSinceLevelLoad - phaseStartTime;
            currentPhase = PracticePhase.Completed;
            SuccessPractice();
        }
    }
    private bool IsOverlappingBox(BoxCollider targetBox)
    {
        if (targetBox == null || loadCollider == null)
            return false;

        // 1 << 8 represents Layer 8 (Cargo)
        int cargoLayerMask = 1 << 8;

        // Calculate the world-space center and half-extents
        Vector3 center = targetBox.transform.TransformPoint(targetBox.center);
        Vector3 halfExtents = Vector3.Scale(targetBox.size, targetBox.transform.lossyScale) * 0.5f;
        Quaternion orientation = targetBox.transform.rotation;

        // Perform the check
        Collider[] hitColliders = Physics.OverlapBox(center, halfExtents, orientation, cargoLayerMask);

        foreach (var hit in hitColliders)
        {
            // Check if the hit object is our specific load
            if (hit == loadCollider || hit.transform == load)
            {
                return true;
            }
        }

        return false;
    }

    public void StartPractice()
    {
        mistakesPerPhase[0] = 0;
        mistakesPerPhase[1] = 0;

        poleHitMistakes = 0;
        isActive = true;
        isFailed = false;
        isCompleted = false;
        currentWaypoint = 0;
        timer = 0f;
        currentPhase = PracticePhase.Zigzag;
        phaseStartTime = Time.timeSinceLevelLoad;
        UpdatePhaseText();
    }
    public void DeductPoints(float amount)
    {
        int phaseIndex = (int)currentPhase;
        if (phaseIndex < mistakesPerPhase.Length)
        {
            mistakesPerPhase[phaseIndex]++;
        }

        totalPoints -= amount;
        if (totalPoints < 0) totalPoints = 0;

        Debug.Log($"Hit pole in {currentPhase}! Mistake added to phase {phaseIndex}. Current Score: {totalPoints}");
    }
    //public void DeductPoints(float amount)
    //{

    //    poleHitMistakes++;
    //    totalPoints -= amount;
    //    Debug.Log($"Hit pole! -{amount} points. Current Score: {totalPoints}");
    //    if (totalPoints < 0)
    //    {
    //        totalPoints = 0;
    //    }
    //}

    public void FailPractice(string reason)
    {
        if (isFailed) return;

        isFailed = true;
        isActive = false;
        UpdatePhaseText($"<color=red>THẤT BẠI:</color> {reason}");
    }

    public void SuccessPractice()
    {
        if (isCompleted) return;
        isActive = false;
        isCompleted = true;
        UpdatePhaseText();
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
    public int GetZigzagMistakes() => mistakesPerPhase[0];
    public int GetTruckMistakes() => mistakesPerPhase[1];
    public int GetZigzagDuration() => Mathf.RoundToInt(phaseDurations[0]);
    public int GetTruckDuration() => Mathf.RoundToInt(phaseDurations[1]);

    public int TotalMistakes => mistakesPerPhase.Sum();
}
