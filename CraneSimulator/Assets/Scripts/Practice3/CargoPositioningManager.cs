using TMPro;
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
    public float totalPoints = 10;
    public float polePenalty = 0.5f;

    private bool isActive = false;
    private bool isFailed = false;
    private bool isCompleted = false;
    private float timer = 0f;

    [Header("Result UI")]
    public TextMeshProUGUI resultText;

    private int poleHitMistakes = 0;
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
            FailPractice("Vượt quá thời gian cho phép!");
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
                FailPractice("Hàng rơi ngoài khu vực đặt!");
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
        poleHitMistakes = 0;
        if (resultText)
        {
            resultText.gameObject.SetActive(true);
            resultText.text = "<color=#FFEB3B>ĐANG LÀM</color>";
        }
    }

    public void DeductPoints(float amount)
    {
        poleHitMistakes++;
        totalPoints -= amount;
        if (totalPoints < 0) totalPoints = 0;

        Debug.Log($"Hit pole! -{amount} pts. Current score: {totalPoints}");
    }

    public void FailPractice(string reason)
    {
        if (isFailed) return;
        isFailed = true;
        isActive = false;

        if (resultText)
        {
            resultText.gameObject.SetActive(true);
            resultText.text =
                "<color=#FF5252>THẤT BẠI</color>";
        }

    }

    public void SuccessPractice()
    {
        isCompleted = true;
        isActive = false;

        if (resultText)
        {
            resultText.gameObject.SetActive(true);
            resultText.text =
                "<color=#4CAF50>HOÀN THÀNH</color>";
        }

        Debug.Log($"Practice Completed! Final score: {totalPoints}, Time: {timer:F1}s");
    }

    public bool IsCompleted => isCompleted;
    public bool IsFailed => isFailed;
    public bool IsActive => isActive;
    public float Timer => timer;
    public int TotalMistakes => poleHitMistakes;
}
