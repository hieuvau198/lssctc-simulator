using UnityEngine;

public class HookBlockController2 : MonoBehaviour
{
    [Header("Rope & Hook Settings")]
    public Transform ropeStartPointLeft;     // Left pulley
    public Transform ropeStartPointRight;    // Right pulley
    public Transform ropeEndPoint;           // Hook attach point
    public Rigidbody hookBlock;

    [Header("Rope Visuals")]
    public LineRenderer ropeLeft;
    public LineRenderer ropeRight;

    [Header("Rope Length Settings")]
    public float minLength = 0.2f;
    public float maxLength = 3f;
    public float ropeSpeed = 0.25f;

    [Header("Key Controls")]
    public KeyCode dropKey = KeyCode.DownArrow;
    public KeyCode retractKey = KeyCode.UpArrow;

    [Header("Joint Settings")]
    public float jointSpring = 50f;
    public float jointDamper = 5f;

    private ConfigurableJoint ropeJoint;
    private float currentLength;

    void Start()
    {
        if (hookBlock == null)
        {
            Debug.LogError("HookBlock Rigidbody not assigned!");
            enabled = false;
            return;
        }

        ropeJoint = hookBlock.GetComponent<ConfigurableJoint>();
        if (ropeJoint == null)
        {
            Debug.LogError("HookBlock missing ConfigurableJoint!");
            enabled = false;
            return;
        }

        SetupLineRenderer(ropeLeft);
        SetupLineRenderer(ropeRight);

        currentLength = minLength;

        // --- THE FIX FOR SWINGING ---
        // Change Limited from Locked to allow X and Z movement (swinging)
        ropeJoint.xMotion = ConfigurableJointMotion.Locked;
        ropeJoint.yMotion = ConfigurableJointMotion.Limited;
        ropeJoint.zMotion = ConfigurableJointMotion.Locked;

        //// Ensure the angular motion is free so the block can rotate while swinging
        //ropeJoint.angularXMotion = ConfigurableJointMotion.Free;
        //ropeJoint.angularYMotion = ConfigurableJointMotion.Free;
        //ropeJoint.angularZMotion = ConfigurableJointMotion.Free;

        JointDrive drive = new JointDrive
        {
            positionSpring = jointSpring,
            positionDamper = jointDamper,
            maximumForce = Mathf.Infinity
        };

        // Apply drive to all axes to help stabilize the "rope" feel
        ropeJoint.xDrive = drive;
        ropeJoint.yDrive = drive;
        ropeJoint.zDrive = drive;
    }

    void Update()
    {
        HandleInput();
        UpdateRopeLength();
        UpdateRopeVisuals();
    }

    private void HandleInput()
    {
        if (Input.GetKey(dropKey))
            currentLength += ropeSpeed * Time.deltaTime;

        if (Input.GetKey(retractKey))
            currentLength -= ropeSpeed * Time.deltaTime;

        currentLength = Mathf.Clamp(currentLength, minLength, maxLength);
    }

    private void UpdateRopeLength()
    {
        SoftJointLimit limit = ropeJoint.linearLimit;
        limit.limit = currentLength;
        ropeJoint.linearLimit = limit;

        // Use midpoint of both pulleys as the pivot point for the swing
        Vector3 centerAnchor = (ropeStartPointLeft.position + ropeStartPointRight.position) * 0.5f;
        ropeJoint.connectedAnchor = centerAnchor;
    }

    private void UpdateRopeVisuals()
    {
        if (ropeLeft != null)
        {
            ropeLeft.SetPosition(0, ropeStartPointLeft.position);
            ropeLeft.SetPosition(1, ropeEndPoint.position);
        }

        if (ropeRight != null)
        {
            ropeRight.SetPosition(0, ropeStartPointRight.position);
            ropeRight.SetPosition(1, ropeEndPoint.position);
        }
    }

    private void SetupLineRenderer(LineRenderer lr)
    {
        if (lr == null) return;
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.startWidth = 0.03f;
        lr.endWidth = 0.03f;
    }
    public bool IsRopeMoving()
    {
        return Input.GetKey(dropKey) || Input.GetKey(retractKey);
    }
}