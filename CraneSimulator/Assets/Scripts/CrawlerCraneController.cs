using UnityEngine;

public class CrawlerCraneController : MonoBehaviour
{
    public float moveSpeed = 10f;   // speed forward/backward
    public float turnSpeed = 50f;   // rotation speed

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Get input from WASD or Arrow Keys
        float moveInput = Input.GetAxis("Vertical");   // W/S or Up/Down
        float turnInput = Input.GetAxis("Horizontal"); // A/D or Left/Right

        // Move forward/backward
        Vector3 move = transform.forward * moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);

        // Rotate left/right
        Quaternion turn = Quaternion.Euler(0f, turnInput * turnSpeed * Time.fixedDeltaTime, 0f);
        rb.MoveRotation(rb.rotation * turn);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Container")
        {
            // Stick the container to the truck
            AttachContainer(collision.gameObject);
        }
    }

    void AttachContainer(GameObject container)
    {
        // Make container follow the truck
        container.transform.SetParent(transform);

        // Optionally freeze container physics so it won t fall
        Rigidbody rb = container.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;  // Stops physics on container
        }
    }

}
