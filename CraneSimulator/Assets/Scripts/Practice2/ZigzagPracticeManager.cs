using UnityEngine;
using System.Collections.Generic;

public class ZigzagPracticeManager : MonoBehaviour
{
    [Header("References")]
    public Transform load; // your cargo object
    public Transform hook; // optional - for swing check
    public List<Transform> waypoints;

    [Header("Practice Settings")]
    public float waypointRadius = 4f;
    public float maxSwingAngle = 15f;
    public float maxTime = 120f;
    public float groundY = 0f; // the Y height of the ground plane

    [Header("Scoring")]
    public int totalPoints = 100;
    public int polePenalty = 5;

    private int currentWaypoint = 0;
    private float timer = 0f;
    private bool isActive = false;
    private bool isFailed = false;
    private bool isCompleted = false;

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
            FailPractice("Time limit exceeded!");
            return;
        }

        // --- Reaching next waypoint ---
        float dist = Vector3.Distance(load.position, waypoints[currentWaypoint].position);
        if (dist < waypointRadius)
        {
            Debug.Log($"Waypoint {currentWaypoint + 1} reached!");
            currentWaypoint++;
            if (currentWaypoint >= waypoints.Count)
            {
                SuccessPractice();
                return;
            }
        }

        // --- Cargo touches ground ---
        if (load.position.y <= groundY + 0.1f) // small offset for collider bottom
        {
            FailPractice("Cargo touched the ground!");
        }
    }

    public void StartPractice()
    {
        isActive = true;
        isFailed = false;
        isCompleted = false;
        currentWaypoint = 0;
        timer = 0f;
        totalPoints = 100;
        Debug.Log("Zigzag practice started! Score: " + totalPoints);
    }

    public void DeductPoints(int amount)
    {
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
        Debug.Log($"Practice failed: {reason} | Final Score: {totalPoints}");
    }

    public void SuccessPractice()
    {
        isActive = false;
        isCompleted = true;
        Debug.Log($"Practice completed successfully in {timer:F1}s! Final Score: {totalPoints}");
    }
    public bool IsCompleted => isCompleted;
    public bool IsFailed => isFailed;
    public bool IsActive => isActive;
}
