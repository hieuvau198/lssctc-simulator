using UnityEngine;

public class CrawlerCraneController : MonoBehaviour
{
    public float moveSpeed = 10f;   // speed forward/backward
    public float turnSpeed = 50f;   // rotation speed

    public HingeJoint boomHingeJoint;      // Reference to the HingeJoint
    public float boomMotorSpeed = 30f;     // How fast the boom moves

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //boomHingeJoint = GetComponentInChildren<HingeJoint>();
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

        // Boom control
        HandleBoomControl();
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

    private void HandleBoomControl()
    {
        float boomInput = 0f;
        if (Input.GetKey(KeyCode.M)) boomInput += 1f;
        if (Input.GetKey(KeyCode.N)) boomInput -= 1f;

        JointMotor motor = boomHingeJoint.motor;
        motor.force = 1000f; // How strong the joint is
        motor.targetVelocity = boomInput * boomMotorSpeed;
        boomHingeJoint.motor = motor;
        boomHingeJoint.useMotor = Mathf.Abs(boomInput) > 0.01f;
    }

}
