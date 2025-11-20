using UnityEngine;

public class CarInteraction : MonoBehaviour
{
    public Transform exitPosition;  // Exit point next to the car
    public Camera carCamera;        // Camera inside the car
    public GameObject playerCapsule;        // The PlayerCapsule object (character)
    public GameObject playerFollowCamera;   // The PlayerFollowCamera object

    private CarController carController;
    private bool isInCar = false;
    private Vector3 cachedPlayerPosition;

    void Start()
    {
        carController = GetComponent<CarController>();
        carCamera.gameObject.SetActive(false);
        playerFollowCamera.SetActive(true);
        playerCapsule.SetActive(true);
        carController.SetControl(false);
    }

    void Update()
    {
        Vector3 referencePosition = isInCar ? cachedPlayerPosition : playerCapsule.transform.position;
        float distance = Vector3.Distance(referencePosition, exitPosition.position);
        //Debug.Log("Distance to exit point: " + distance);

        // Increase radius to make exit easier
        if (distance < 4f && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Attempt to enter/exit car");
            if (!isInCar)
                EnterCar();
            else
                ExitCar();
        }
    }

    private void EnterCar()
    {
        isInCar = true;
        cachedPlayerPosition = playerCapsule.transform.position; // Cache current position

        playerCapsule.SetActive(false);
        playerFollowCamera.SetActive(false);
        carController.SetControl(true);
        carCamera.gameObject.SetActive(true);

        Debug.Log("Entered Car");
    }

    private void ExitCar()
    {
        isInCar = false;

        carController.SetControl(false);
        carCamera.gameObject.SetActive(false);

        playerCapsule.SetActive(true);
        playerCapsule.transform.position = exitPosition.position;
        playerFollowCamera.SetActive(true);

        Debug.Log("Exited Car");
    }
}
