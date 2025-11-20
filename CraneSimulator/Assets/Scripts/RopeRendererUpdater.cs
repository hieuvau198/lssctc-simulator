using UnityEngine;

[ExecuteAlways] // Keeps rope visible even in Edit mode
public class RopeRendererUpdater : MonoBehaviour
{
    [Header("Rope Settings")]
    public LineRenderer rope;
    public Transform startPoint;
    public Transform endPoint;

    void Update()
    {
        if (rope == null || startPoint == null || endPoint == null)
            return;

        // Ensure rope has exactly two points
        if (rope.positionCount != 2)
            rope.positionCount = 2;

        // Continuously update rope position between start and end
        rope.SetPosition(0, startPoint.position);
        rope.SetPosition(1, endPoint.position);
    }
}
