using UnityEngine;

public class CraneHook2 : MonoBehaviour
{
    [Header("Reference")]
    public HookBlockController2 hookBlockController;
    public BoomController2 boomController;
    [Header("Hook & Rope Settings")]
    public Transform hookAttachPoint;         // Bottom of hook block
    public Material ropeMaterial;             // Line material for 4 cargo ropes
    public float ropeWidth = 0.04f;

    [Header("Cargo Detection")]
    public float attachDistance = 1.5f;         // Detection range below hook
    public LayerMask cargoLayer;              // Layer for cargo objects
    public KeyCode attachKey = KeyCode.B;     // Press to attach/detach

    [Header("Joint Physics")]
    public float breakForce = Mathf.Infinity;         // Optional break force

    [Header("Decay Point Hook")]
    public Transform decayHookPoint;
    //public Transform decayHookOn;
    [Header("UI Text")]
    public TMPro.TextMeshProUGUI attachHintText;



    private GameObject connectedCargo;
    private LineRenderer cargoRopes;
    private bool isConnected = false;
    private Transform[] cargoPoints = new Transform[4];
    private Rigidbody hookRb;
    private ConfigurableJoint cargoJoint;

    void Start()
    {
        hookRb = GetComponent<Rigidbody>();
        // Optional setup (sizes can be adjusted to your model)
        if (decayHookPoint)
            decayHookPoint.localScale = new Vector3(0.07f, 0.07f, 0.07f);
        if (attachHintText)
            attachHintText.gameObject.SetActive(false);

    }

    void Update()
    {
        UpdateDecayPointRay();
        // Press key to attach/detach cargo
        if (Input.GetKeyDown(attachKey))
        {
            if (!isConnected) TryAttachCargo();
            else DetachCargo();
        }

        if (isConnected && connectedCargo != null)
        {
            StabilizeCargoDuringLift();
            UpdateCargoRopes();
        }
    }
    private void UpdateDecayPointRay()
    {
        if (isConnected)
        {
            // Ensure hint and decay point stay OFF when connected
            if (attachHintText) attachHintText.gameObject.SetActive(false);
            if (decayHookPoint) decayHookPoint.gameObject.SetActive(false);
            return;
        }
        if (!decayHookPoint || !hookAttachPoint) return;

        Ray ray = new Ray(hookAttachPoint.position, Vector3.down);
        int ignoreLayerMask = (1 << 8); // ignore "Ignore Raycast" layer

        // Always draw the ray for debug
        Debug.DrawRay(hookAttachPoint.position, Vector3.down * attachDistance, Color.cyan);

        // Step 1: Raycast down to align decayHookPoint (hits ground or cargo)
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, ignoreLayerMask))
        {
            decayHookPoint.gameObject.SetActive(true);
            decayHookPoint.position = hit.point + hit.normal * 0.01f;
            decayHookPoint.rotation = Quaternion.LookRotation(-hit.normal);
        }
        else
        {
            decayHookPoint.gameObject.SetActive(false);
        }

        // Step 2: Check if cargo is within attach distance
        bool cargoDetected = Physics.Raycast(ray, out RaycastHit cargoHit, attachDistance, cargoLayer);

        if (cargoDetected)
        {
            Debug.DrawRay(hookAttachPoint.position, Vector3.down * attachDistance, Color.green);

            // Optional: Align the decay hook to cargo surface directly
            decayHookPoint.position = cargoHit.point + cargoHit.normal * 0.01f;
            decayHookPoint.rotation = Quaternion.LookRotation(-cargoHit.normal);

            // Show decayHookOn
            if (attachHintText)
            {
                attachHintText.gameObject.SetActive(true);
                attachHintText.text = "Press B to Hook";
            }
        }
        else
        {
            if (attachHintText && !isConnected)
                attachHintText.gameObject.SetActive(false);
        }
    }


    private void TryAttachCargo()
    {
        // Cast ray downward to find cargo
        Ray ray = new Ray(hookAttachPoint.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, attachDistance, cargoLayer))
        {

            CargoCrane_L cargoScript = hit.collider.GetComponentInParent<CargoCrane_L>();
            if (cargoScript != null)
                AttachCargo(cargoScript);
        }
    }

    private void AttachCargo(CargoCrane_L cargo)
    {
        if (cargo == null) return;

        connectedCargo = cargo.gameObject;

        Rigidbody cargoRb = connectedCargo.GetComponent<Rigidbody>();
        if (cargoRb == null)
        {
            cargoRb = connectedCargo.AddComponent<Rigidbody>();
            cargoRb.mass = 100f;
            cargoRb.linearDamping = 0.2f;
            cargoRb.angularDamping = 0.3f;
        }

        // --- Add ConfigurableJoint for realistic rope physics ---
        cargoJoint = connectedCargo.AddComponent<ConfigurableJoint>();
        cargoJoint.connectedBody = hookRb;

        cargoJoint.anchor = Vector3.zero;
        cargoJoint.connectedAnchor = hookAttachPoint.localPosition;

        cargoJoint.xMotion = ConfigurableJointMotion.Limited;
        cargoJoint.yMotion = ConfigurableJointMotion.Limited;
        cargoJoint.zMotion = ConfigurableJointMotion.Limited;

        cargoJoint.angularXMotion = ConfigurableJointMotion.Free;
        cargoJoint.angularYMotion = ConfigurableJointMotion.Free;
        cargoJoint.angularZMotion = ConfigurableJointMotion.Free;

        cargoJoint.autoConfigureConnectedAnchor = false;
        cargoJoint.configuredInWorldSpace = true;

        SoftJointLimit limit = new SoftJointLimit();
        limit.limit = 0.75f; // small flexibility in rope
        cargoJoint.linearLimit = limit;

        JointDrive drive = new JointDrive
        {
            positionSpring = 60f,
            positionDamper = 8f,
            maximumForce = 10000f

        };
        cargoJoint.xDrive = drive;
        cargoJoint.yDrive = drive;
        cargoJoint.zDrive = drive;

        cargoJoint.breakForce = float.PositiveInfinity;
        cargoJoint.breakTorque = float.PositiveInfinity;

        // --- Setup visual ropes ---
        cargoPoints[0] = cargo.pointLineCargo1;
        cargoPoints[1] = cargo.pointLineCargo2;
        cargoPoints[2] = cargo.pointLineCargo3;
        cargoPoints[3] = cargo.pointLineCargo4;

        cargoRopes = connectedCargo.AddComponent<LineRenderer>();
        cargoRopes.material = ropeMaterial;
        cargoRopes.startWidth = ropeWidth;
        cargoRopes.endWidth = ropeWidth;
        cargoRopes.positionCount = 8;
        cargoRopes.numCapVertices = 4;

        if (attachHintText) attachHintText.gameObject.SetActive(false);
        if (decayHookPoint) decayHookPoint.gameObject.SetActive(false);
        isConnected = true;
    }

    private void DetachCargo()
    {
        if (!isConnected) return;

        if (cargoJoint != null) Destroy(cargoJoint);
        if (cargoRopes != null) Destroy(cargoRopes);

        connectedCargo = null;
        cargoJoint = null;
        isConnected = false;

        if (attachHintText)
            attachHintText.gameObject.SetActive(false);
    }

    private void UpdateCargoRopes()
    {
        if (cargoRopes == null || connectedCargo == null) return;

        Vector3 hookPos = hookAttachPoint.position;
        Vector3[] ropePoints = new Vector3[8];

        // 4 rope pairs (hook -> each cargo corner)
        ropePoints[0] = hookPos; ropePoints[1] = cargoPoints[0].position;
        ropePoints[2] = hookPos; ropePoints[3] = cargoPoints[1].position;
        ropePoints[4] = hookPos; ropePoints[5] = cargoPoints[2].position;
        ropePoints[6] = hookPos; ropePoints[7] = cargoPoints[3].position;

        cargoRopes.SetPositions(ropePoints);
    }
    private void StabilizeCargoDuringLift()
    {
        if (cargoJoint == null) return;

        bool moving = (hookBlockController != null && hookBlockController.IsRopeMoving()) || (boomController != null && boomController.IsBoomMoving());

        SoftJointLimitSpring limitSpring = cargoJoint.linearLimitSpring;

        if (moving)
        {
            // MAKE ROPE LENGTH STIFF BUT CONSTANT
            limitSpring.spring = 20000f;
            limitSpring.damper = 2000f;
        }
        else
        {
            // restore normal rope flexibility
            limitSpring.spring = 0f;
            limitSpring.damper = 0f;
        }

        cargoJoint.linearLimitSpring = limitSpring;
    }

    private void OnDrawGizmosSelected()
    {
        if (hookAttachPoint)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(hookAttachPoint.position, hookAttachPoint.position + Vector3.down * attachDistance);
        }
    }
}
