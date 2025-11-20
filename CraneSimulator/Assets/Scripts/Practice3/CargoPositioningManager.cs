using UnityEngine;

public class CargoPositioningManager : MonoBehaviour
{
    [Header("References")]
    public Transform cargo;          // The lifted cargo
    public Transform targetZone;     // Center point of the target circle

    [Header("Settings")]
    public float targetRadius = 3f;  // Size of landing zone
    public float requiredHeight = 0.5f; // Cargo must be below this to check placement
    public float maxTime = 300f;      // Time limit
    public float groundY = 0f;

    [Header("Scoring")]
    public int totalPoints = 100;
    public int polePenalty = 5;

    private bool isActive = false;
    private bool isFailed = false;
    private bool isCompleted = false;
    private float timer = 0f;

    void Start()
    {
        StartPractice();
    }

    void Update()
    {
        if (!isActive || isFailed || isCompleted)
            return;

        timer += Time.deltaTime;

        // --- Time limit ---
        if (timer > maxTime)
        {
            FailPractice("Time limit exceeded!");
            return;
        }

        // --- Cargo touches ground OUTSIDE target zone ---
        if (cargo.position.y <= groundY + 0.1f)
        {
            float distFromTarget = Vector3.Distance(
                new Vector3(cargo.position.x, 0, cargo.position.z),
                new Vector3(targetZone.position.x, 0, targetZone.position.z)
            );

            if (distFromTarget > targetRadius)
            {
                FailPractice("Cargo dropped outside the target zone!");
                return;
            }
        }

        // --- Successful placement check ---
        if (cargo.position.y <= requiredHeight)
        {
            float dist = Vector3.Distance(
                new Vector3(cargo.position.x, 0, cargo.position.z),
                new Vector3(targetZone.position.x, 0, targetZone.position.z)
            );

            if (dist <= targetRadius)
            {
                SuccessPractice();
            }
        }
    }

    public void StartPractice()
    {
        isActive = true;
        isFailed = false;
        isCompleted = false;
        timer = 0f;
        totalPoints = 100;

        Debug.Log("Cargo Positioning Challenge started!");
    }

    public void DeductPoints(int amount)
    {
        totalPoints -= amount;
        if (totalPoints < 0) totalPoints = 0;

        Debug.Log($"Hit pole! -{amount} pts. Current score: {totalPoints}");
    }

    public void FailPractice(string reason)
    {
        if (isFailed) return;
        isFailed = true;
        isActive = false;

        Debug.Log($"Practice Failed: {reason} | Score: {totalPoints}");
    }

    public void SuccessPractice()
    {
        isCompleted = true;
        isActive = false;

        Debug.Log($"Practice Completed! Final score: {totalPoints}, Time: {timer:F1}s");
    }

    public bool IsCompleted => isCompleted;
    public bool IsFailed => isFailed;
    public bool IsActive => isActive;
    public float Timer => timer;
}
