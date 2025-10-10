using UnityEngine;

public class CraneFollowCamera : MonoBehaviour
{
    public Transform crane; // Reference to the crane's transform
    public Vector3 offset;
    void Update()
    {
        transform.transform.position = crane.position + offset;
    }
}
