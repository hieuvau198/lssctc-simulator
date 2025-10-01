using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; // Movement speed
    public float rotationSpeed = 700f; // Rotation speed

    private Rigidbody rb;
    private float moveInput;
    private float turnInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Get the Rigidbody component attached to the player
    }

    void FixedUpdate()
    {
        // Get input for movement (WASD or Arrow Keys)
        moveInput = Input.GetAxis("Vertical");   // W/S or Up/Down
        turnInput = Input.GetAxis("Horizontal"); // A/D or Left/Right

        // Apply movement (move forward/backward relative to player’s orientation)
        Vector3 move = transform.forward * moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        // Apply rotation (rotate left/right)
        Quaternion turn = Quaternion.Euler(0f, turnInput * rotationSpeed * Time.fixedDeltaTime, 0f);
        rb.MoveRotation(rb.rotation * turn);
    }
}
