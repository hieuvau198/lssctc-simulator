using UnityEngine;

public class HookBlockController : MonoBehaviour
{
    [Header("Rope & Hook Settings")]
    public Transform ropeStartPoint;          // Tip of boom
    public Transform ropeEndPoint;            // Rope attach point on HookBlock
    public Rigidbody hookBlock;               // Hook block Rigidbody
    public LineRenderer ropeRenderer;         // Assign manually in Inspector

    [Header("Rope Length Settings")]
    public float minLength = 0.2f;
    public float maxLength = 2f;
    public float ropeSpeed = 0.25f;

    [Header("Key Controls")]
    public KeyCode dropKey = KeyCode.N;       // Extend rope (drop)
    public KeyCode retractKey = KeyCode.M;    // Retract rope (pull up)

    [Header("Joint Settings")]
    public float jointSpring = 50f;           // Rope stiffness
    public float jointDamper = 5f;            // Rope damping

    private ConfigurableJoint ropeJoint;
    private float currentLength;
    private bool isInitialized = false;

    void Start()
    {
        if (hookBlock == null)
        {
            Debug.LogError("HookBlock Rigidbody is not assigned!");
            enabled = false;
            return;
        }

        ropeJoint = hookBlock.GetComponent<ConfigurableJoint>();
        if (ropeJoint == null)
        {
            Debug.LogError("HookBlock is missing a ConfigurableJoint!");
            enabled = false;
            return;
        }

        if (ropeRenderer == null)
        {
            Debug.LogWarning("No LineRenderer assigned — rope will be invisible.");
        }
        else
        {
            ropeRenderer.positionCount = 2;
        }

        // Initialize current rope length
        currentLength = minLength;

        // Configure joint motion
        ropeJoint.xMotion = ConfigurableJointMotion.Locked;
        ropeJoint.yMotion = ConfigurableJointMotion.Limited;
        ropeJoint.zMotion = ConfigurableJointMotion.Locked;

        JointDrive drive = new JointDrive
        {
            positionSpring = jointSpring,
            positionDamper = jointDamper,
            maximumForce = Mathf.Infinity
        };

        ropeJoint.xDrive = ropeJoint.yDrive = ropeJoint.zDrive = drive;

        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized) return;

        HandleInput();
        UpdateRopeLength();
        UpdateRopeVisual();
    }

    private void HandleInput()
    {
        if (Input.GetKey(dropKey))
            currentLength += ropeSpeed * Time.deltaTime;
        else if (Input.GetKey(retractKey))
            currentLength -= ropeSpeed * Time.deltaTime;

        currentLength = Mathf.Clamp(currentLength, minLength, maxLength);
    }

    private void UpdateRopeLength()
    {
        SoftJointLimit limit = ropeJoint.linearLimit;
        limit.limit = currentLength;
        ropeJoint.linearLimit = limit;

        // Keep rope connected to boom tip
        ropeJoint.connectedAnchor = ropeStartPoint.position;
    }

    private void UpdateRopeVisual()
    {
        if (ropeRenderer == null) return;

        ropeRenderer.SetPosition(0, ropeStartPoint.position);
        ropeRenderer.SetPosition(1, ropeEndPoint.position);
    }
}
