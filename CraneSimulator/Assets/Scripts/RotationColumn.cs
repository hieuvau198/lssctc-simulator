using UnityEngine;

public class RotationColumn : MonoBehaviour
{
    [Header("Rotation Column Crane")]
    public Transform columnCrane;
    public KeyCode leftRotationColumn;
    public KeyCode rightRotationColumn;
    public float speedColumn = 0f;
    public float smoothRotationColumn = 1.2f;

    private float floatRotColumn = 0;
    private bool rotationColumn_Bool = true;

    void Update()
    {
        // Rotate left
        if (Input.GetKey(rightRotationColumn))
        {
            floatRotColumn += Time.deltaTime * speedColumn;
            rotationColumn_Bool = true;
        }
        // Rotate right
        else if (Input.GetKey(leftRotationColumn))
        {
            floatRotColumn -= Time.deltaTime * speedColumn;
            rotationColumn_Bool = false;
        }

        // smooth rotation
        Quaternion targetRotation = Quaternion.AngleAxis(floatRotColumn, Vector3.forward);
        columnCrane.localRotation = Quaternion.Lerp(columnCrane.localRotation, targetRotation, Time.deltaTime * speedColumn / smoothRotationColumn);

        
    }
}
