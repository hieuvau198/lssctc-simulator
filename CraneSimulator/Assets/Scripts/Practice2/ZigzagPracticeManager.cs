using UnityEngine;
using System.Collections.Generic;

public class ZigzagPracticeManager : MonoBehaviour
{
    [Header("References")]
    public Transform load; // your cargo hook or load object
    public List<Transform> waypoints;
    public float waypointRadius = 2f;
    public float maxSwingAngle = 15f;
    public float maxTime = 120f;

    private int currentWaypoint = 0;
    private float timer = 0f;
    private bool isActive = false;
    private bool isFailed = false;

    void Update()
    {
        if (!isActive || isFailed) return;

        timer += Time.deltaTime;

        if (timer > maxTime)
        {
            FailPractice("Time limit exceeded");
            return;
        }

        // Check reaching waypoint
        float dist = Vector3.Distance(load.position, waypoints[currentWaypoint].position);
        if (dist < waypointRadius)
        {
            currentWaypoint++;
            if (currentWaypoint >= waypoints.Count)
            {
                SuccessPractice();
                return;
            }
        }
    }

    public void StartPractice()
    {
        isActive = true;
        timer = 0f;
        currentWaypoint = 0;
        isFailed = false;
        Debug.Log("Zigzag practice started!");
    }

    public void FailPractice(string reason)
    {
        isFailed = true;
        Debug.Log("Practice failed: " + reason);
    }

    public void SuccessPractice()
    {
        isActive = false;
        Debug.Log("Zigzag practice completed successfully in " + timer.ToString("F1") + " seconds!");
    }
}
