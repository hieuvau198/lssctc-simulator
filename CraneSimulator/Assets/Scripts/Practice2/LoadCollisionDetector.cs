using UnityEngine;

public class LoadCollisionDetector : MonoBehaviour
{
    public ZigzagPracticeManager practiceManager;
    public CargoPositioningManager cargoPositioningManager;
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
        if (!practiceManager && !cargoPositioningManager) return;

        if (practiceManager && collision.gameObject.CompareTag("Pole"))
        {
            practiceManager.DeductPoints(practiceManager.polePenalty);
        }
        if (cargoPositioningManager && collision.gameObject.CompareTag("Pole"))
        {
            cargoPositioningManager.DeductPoints(cargoPositioningManager.polePenalty);
        }

    }
}
