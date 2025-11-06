using UnityEngine;

public class LoadCollisionDetector : MonoBehaviour
{
    public ZigzagPracticeManager practiceManager;
    public float minYPosition = 1.72f;

    private void Update()
    {
        if (!practiceManager) return;

        // Check Y height every frame
        if (transform.position.y < minYPosition)
        {
            practiceManager.FailPractice("Cargo dropped too low (touched the ground)!");
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!practiceManager) return;

        if (collision.gameObject.CompareTag("Pole"))
        {
            practiceManager.DeductPoints(practiceManager.polePenalty);
        }

    }
}
