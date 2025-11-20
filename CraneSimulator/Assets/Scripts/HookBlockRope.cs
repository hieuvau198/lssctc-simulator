
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
    void Start()
    {
        if (ropeJoint != null && ropeStartPoint != null)
        {
            ropeJoint.configuredInWorldSpace = true;
            ropeJoint.connectedAnchor = ropeStartPoint.position;

            // Configure angular X drive
            JointDrive angularXDrive = ropeJoint.angularXDrive;
            angularXDrive.positionSpring = 1000f;
            angularXDrive.positionDamper = 50f;
            ropeJoint.angularXDrive = angularXDrive;

            // Configure angular YZ drive
            JointDrive angularYZDrive = ropeJoint.angularYZDrive;
            angularYZDrive.positionSpring = 1000f;
            angularYZDrive.positionDamper = 50f;
            ropeJoint.angularYZDrive = angularYZDrive;

            // Configure linear drives
            JointDrive linearDrive = ropeJoint.xDrive;
            linearDrive.positionSpring = 1000f;
            linearDrive.positionDamper = 50f;
            ropeJoint.xDrive = linearDrive;
            ropeJoint.yDrive = linearDrive;
            ropeJoint.zDrive = linearDrive;

            // Limit angular motion to reduce swinging
            ropeJoint.lowAngularXLimit = new SoftJointLimit { limit = -10f };
            ropeJoint.highAngularXLimit = new SoftJointLimit { limit = 10f };
            ropeJoint.angularYLimit = new SoftJointLimit { limit = 10f };
            ropeJoint.angularZLimit = new SoftJointLimit { limit = 10f };

            // Make joint unbreakable
            ropeJoint.breakForce = Mathf.Infinity;
            ropeJoint.breakTorque = Mathf.Infinity;
        }
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
            //ropeJoint.configuredInWorldSpace = true;
            ropeJoint.connectedAnchor = ropeStartPoint.position;
        }
    }
}



