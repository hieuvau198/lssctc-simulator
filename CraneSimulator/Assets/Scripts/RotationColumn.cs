using UnityEngine;

public class RotationColumn : MonoBehaviour
{
    [Header("Rotation Column Crane")]
    public Transform columnCrane;
    public KeyCode leftRotationColumn = KeyCode.LeftArrow;
    public KeyCode rightRotationColumn = KeyCode.RightArrow;
    public float speedColumn = 0f;
    public float smoothRotationColumn = 1.2f;

    [Header("Audio")]
    public AudioSource moveAudio;

    private float floatRotColumn = 0;
    //private bool rotationColumn_Bool = true;

    void Update()
    {
        bool isMoving = false;
        // Rotate left
        if (Input.GetKey(rightRotationColumn))
        {
            floatRotColumn += Time.deltaTime * speedColumn;
            //rotationColumn_Bool = true;
            isMoving = true;
        }
        // Rotate right
        else if (Input.GetKey(leftRotationColumn))
        {
            floatRotColumn -= Time.deltaTime * speedColumn;
            //rotationColumn_Bool = false;
            isMoving = true;
        }

        // smooth rotation
        Quaternion targetRotation = Quaternion.AngleAxis(floatRotColumn, Vector3.forward);
        columnCrane.localRotation = Quaternion.Lerp(columnCrane.localRotation, targetRotation, Time.deltaTime * speedColumn / smoothRotationColumn);

        HandleMovementSound(isMoving);
    }
    private void HandleMovementSound(bool isMoving)
    {
        if (isMoving)
        {
            if (!moveAudio.isPlaying)
                moveAudio.Play();
        }
        else
        {
            if (moveAudio.isPlaying)
                moveAudio.Stop();
        }
    }

}
