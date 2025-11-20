using UnityEngine;

public class OutTriggerRight : MonoBehaviour
{
    [Header("References")]
    public Transform arm; // Assign OutTrigger_Right
    public Transform leg; // Assign OutTrigger_Right_Leg

    [Header("Settings")]
    public float moveSpeed = 1f;
    public float armExtendDistance = 2f; // moves along +X
    public float legExtendDistance = 1f; // moves along -Z

    [Header("Key Controls")]
    public KeyCode extendKey = KeyCode.C;
    public KeyCode retractKey = KeyCode.V;

    private Vector3 armStartPos;
    private Vector3 legStartPos;
    private bool extendingHeld;
    private bool retractingHeld;

    void Start()
    {
        armStartPos = arm.localPosition;
        legStartPos = leg.localPosition;
    }

    void Update()
    {
        extendingHeld = Input.GetKey(extendKey);
        retractingHeld = Input.GetKey(retractKey);

        if (extendingHeld)
            ExtendManual();
        else if (retractingHeld)
            RetractManual();
    }

    private void ExtendManual()
    {
        // Step 1: move arm along +X
        Vector3 armTarget = armStartPos + new Vector3(armExtendDistance, 0, 0);
        if (Vector3.Distance(arm.localPosition, armTarget) > 0.01f)
        {
            arm.localPosition = Vector3.MoveTowards(
                arm.localPosition,
                armTarget,
                moveSpeed * Time.deltaTime
            );
        }
        else
        {
            // Step 2: move leg along -Z
            Vector3 legTarget = legStartPos - new Vector3(0, 0, legExtendDistance);
            leg.localPosition = Vector3.MoveTowards(
                leg.localPosition,
                legTarget,
                moveSpeed * Time.deltaTime
            );
        }
    }

    private void RetractManual()
    {
        // Step 1: retract leg first
        if (Vector3.Distance(leg.localPosition, legStartPos) > 0.01f)
        {
            leg.localPosition = Vector3.MoveTowards(
                leg.localPosition,
                legStartPos,
                moveSpeed * Time.deltaTime
            );
        }
        else
        {
            // Step 2: retract arm back
            arm.localPosition = Vector3.MoveTowards(
                arm.localPosition,
                armStartPos,
                moveSpeed * Time.deltaTime
            );
        }
    }
}
