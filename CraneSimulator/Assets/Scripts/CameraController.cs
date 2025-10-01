using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera playerCamera; // Reference to the Camera attached to the player
    public GameObject fixedPointPrefab; // The point you want to spawn and track (can be a simple sphere or any object)

    private GameObject fixedPoint; // Instance of the fixed point in the world
    private Vector3 fixedPointOffset; // Offset from camera position (relative to camera's view)

    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main; // If no camera is assigned, use the main camera
        }

        if (fixedPointPrefab != null)
        {
            // Instantiate the fixed point object at the camera's initial position
            fixedPoint = Instantiate(fixedPointPrefab, playerCamera.transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogError("No fixed point prefab assigned!");
        }
    }

    void Update()
    {
        if (fixedPoint != null)
        {
            // Calculate the center of the camera's viewport in world space
            Vector3 screenCenter = new Vector3(0.5f, 0.5f, 10f); // Z is depth, adjust based on your need

            // Convert the screen center (0.5, 0.5) to world position
            Vector3 worldPos = playerCamera.ViewportToWorldPoint(screenCenter);

            // Set the fixed point to this position
            fixedPoint.transform.position = worldPos;

            // Optionally, maintain the fixed point's relative position to the camera
            fixedPointOffset = worldPos - playerCamera.transform.position;
        }
    }
}
