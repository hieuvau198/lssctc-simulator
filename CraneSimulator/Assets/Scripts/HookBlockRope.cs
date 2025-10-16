
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class HookBlockRope : MonoBehaviour
{
    [Header("References")]
    public Transform ropeStartPoint;     // Boom tip
    public Transform ropeEndPoint;       // Hook block attach point
    public ConfigurableJoint ropeJoint;  // Hook block's joint

    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
    }

    void LateUpdate()
    {
        // --- Update rope visuals ---
        if (ropeStartPoint && ropeEndPoint)
        {
            lineRenderer.SetPosition(0, ropeStartPoint.position);
            lineRenderer.SetPosition(1, ropeEndPoint.position);
        }

        // --- Keep rope joint connected to boom tip ---
        if (ropeJoint != null && ropeStartPoint != null)
        {
            ropeJoint.configuredInWorldSpace = true;
            ropeJoint.connectedAnchor = ropeStartPoint.position;
        }
    }
}



