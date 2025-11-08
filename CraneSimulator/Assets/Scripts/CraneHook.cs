using UnityEngine;

public class CraneHook : MonoBehaviour
{
    [Header("Hook & Rope Settings")]
    public Transform hookAttachPoint;         // Bottom of hook block
    public Material ropeMaterial;             // Line material for 4 cargo ropes
    public float ropeWidth = 0.04f;

    [Header("Cargo Detection")]
    public float attachDistance = 0.5f;         // Detection range below hook
    public LayerMask cargoLayer;              // Layer for cargo objects
    public KeyCode attachKey = KeyCode.B;     // Press to attach/detach

    [Header("Joint Physics")]
    public float breakForce = Mathf.Infinity;         // Optional break force

    private GameObject connectedCargo;
    private LineRenderer cargoRopes;
    private bool isConnected = false;
    private Transform[] cargoPoints = new Transform[4];
    private Rigidbody hookRb;
    private ConfigurableJoint cargoJoint;

    void Start()
    {
        hookRb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Press key to attach/detach cargo
        if (Input.GetKeyDown(attachKey))
        {
            if (!isConnected) TryAttachCargo();
            else DetachCargo();
        }

        if (isConnected && connectedCargo != null)
        {
            UpdateCargoRopes();
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

    private void OnDrawGizmosSelected()
    {
        if (hookAttachPoint)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(hookAttachPoint.position, hookAttachPoint.position + Vector3.down * attachDistance);
        }
    }
}
