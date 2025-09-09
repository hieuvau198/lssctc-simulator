using UnityEngine;

public class CarController : MonoBehaviour
{
    public enum ControlMode { MoveBase, ControlCrane }

    public Rigidbody wheelBaseRigidbody;
    public HingeJoint bodyToWheelJoint;
    public HingeJoint hookToBodyJoint;

    public float moveSpeed = 5f;
    public float turnSpeed = 60f;
    public float bodyMotorSpeed = 100f;
    public float hookMotorSpeed = 50f;

    private bool isControllable = false;
    private ControlMode currentMode = ControlMode.MoveBase;

    private Quaternion bodyInitialLocalRotation;
    private Quaternion hookInitialLocalRotation;

    private float moveInput = 0f;
    private float turnInput = 0f;      // Used for base turning in MoveBase mode
    private float bodyRotateInput = 0f;
    private float hookInput = 0f;

    void Awake()
    {
        bodyInitialLocalRotation = bodyToWheelJoint.transform.localRotation;
        hookInitialLocalRotation = hookToBodyJoint.transform.localRotation;
    }

    void Start()
    {
        DisableHinge(bodyToWheelJoint);
        DisableHinge(hookToBodyJoint);
    }

    void OnEnable()
    {
        bodyToWheelJoint.transform.localRotation = bodyInitialLocalRotation;
        hookToBodyJoint.transform.localRotation = hookInitialLocalRotation;
        wheelBaseRigidbody.WakeUp();
    }

    void OnDisable()
    {
        bodyToWheelJoint.transform.localRotation = bodyInitialLocalRotation;
        hookToBodyJoint.transform.localRotation = hookInitialLocalRotation;
        wheelBaseRigidbody.Sleep();
    }

    void Update()
    {
        if (!isControllable)
            return;

        // Mode toggle on Tab key
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleMode();
        }

        // Read inputs according to current mode
        if (currentMode == ControlMode.MoveBase)
        {
            moveInput = Input.GetKey(KeyCode.W) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0f;
            turnInput = Input.GetKey(KeyCode.A) ? -1f : Input.GetKey(KeyCode.D) ? 1f : 0f;

            // Disable crane motors when moving base
            DisableHinge(bodyToWheelJoint);
            DisableHinge(hookToBodyJoint);
        }
        else if (currentMode == ControlMode.ControlCrane)
        {
            bodyRotateInput = Input.GetKey(KeyCode.A) ? -1f : Input.GetKey(KeyCode.D) ? 1f : 0f;
            hookInput = Input.GetKey(KeyCode.W) ? 1f : Input.GetKey(KeyCode.S) ? -1f : 0f;

            // Disable base movement inputs
            moveInput = 0f;
            turnInput = 0f;
        }
    }

    void FixedUpdate()
    {
        if (!isControllable)
            return;

        if (currentMode == ControlMode.MoveBase)
        {
            MoveWheelBase(moveInput, turnInput);
        }
        else if (currentMode == ControlMode.ControlCrane)
        {
            ApplyMotor(bodyToWheelJoint, bodyRotateInput, bodyMotorSpeed);
            ApplyMotor(hookToBodyJoint, hookInput, hookMotorSpeed);
        }
    }

    void MoveWheelBase(float move, float turn)
    {
        Vector3 moveDir = transform.forward * move * moveSpeed * Time.fixedDeltaTime;
        wheelBaseRigidbody.MovePosition(wheelBaseRigidbody.position + moveDir);

        Quaternion turnRot = Quaternion.Euler(0, turn * turnSpeed * Time.fixedDeltaTime, 0);
        wheelBaseRigidbody.MoveRotation(wheelBaseRigidbody.rotation * turnRot);
    }

    void ApplyMotor(HingeJoint joint, float input, float speed)
    {
        JointMotor motor = joint.motor;
        if (input != 0)
        {
            motor.targetVelocity = input * speed;
            motor.force = 1000f;
            joint.motor = motor;
            joint.useMotor = true;
        }
        else
        {
            motor.targetVelocity = 0;
            motor.force = 0;
            joint.motor = motor;
            joint.useMotor = false;
        }
    }

    void DisableHinge(HingeJoint hinge)
    {
        JointMotor motor = hinge.motor;
        motor.force = 0;
        motor.targetVelocity = 0;
        hinge.motor = motor;
        hinge.useMotor = false;

        hinge.useLimits = true;
        JointLimits limits = hinge.limits;
        limits.min = 0;
        limits.max = 0;
        hinge.limits = limits;
    }
    void EnableHinge(HingeJoint hinge)
    {
        // Remove angle limits to allow free rotation during control
        hinge.useLimits = false;

        JointMotor motor = hinge.motor;
        motor.force = 1000f;  // Set a reasonable motor force when enabled
        hinge.motor = motor;
        hinge.useMotor = false; // Don't run motor immediately - wait for input
    }
    void ToggleMode()
    {
        if (currentMode == ControlMode.MoveBase)
        {
            currentMode = ControlMode.ControlCrane;
            EnableHinge(bodyToWheelJoint);
            EnableHinge(hookToBodyJoint);
            Debug.Log("Switched to Crane Control Mode");
        }
        else
        {
            currentMode = ControlMode.MoveBase;
            DisableHinge(bodyToWheelJoint);
            DisableHinge(hookToBodyJoint);
            Debug.Log("Switched to Base Movement Mode");
        }
    }

    public void SetControl(bool enable)
    {
        isControllable = enable;
        if (!enable)
        {
            DisableHinge(bodyToWheelJoint);
            DisableHinge(hookToBodyJoint);
            bodyToWheelJoint.transform.localRotation = bodyInitialLocalRotation;
            hookToBodyJoint.transform.localRotation = hookInitialLocalRotation;
        }
    }
}
