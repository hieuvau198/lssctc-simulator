using UnityEngine;

public class BoomForward2 : MonoBehaviour
{
    [Header("Boom 2 (X Axis)")]
    public Transform boom2;
    public float min_Boom2X = 0f;
    public float max_Boom2X = 0f;
    private Vector3 minBoom_2;
    private Vector3 maxBoom_2;

    [Header("Boom 3 (Z Axis)")]
    public Transform boom3;
    public float min_Boom3Z = 0f;
    public float max_Boom3Z = 0f;
    private Vector3 minBoom_3;
    private Vector3 maxBoom_3;

    [Header("Boom 4 (Z Axis)")]
    public Transform boom4;
    public float min_Boom4Z = 0f;
    public float max_Boom4Z = 0f;
    private Vector3 minBoom_4;
    private Vector3 maxBoom_4;

    [Header("Settings")]
    public KeyCode extendKey = KeyCode.UpArrow;
    public KeyCode retractKey = KeyCode.DownArrow;
    public float extendSpeed = 2f;

    //[Header("Audio")]
    //public AudioSource moveAudio;

    private bool extending;

    void Start()
    {
        // Boom 2 (X axis)
        minBoom_2 = new Vector3(min_Boom2X, boom2.localPosition.y, boom2.localPosition.z);
        maxBoom_2 = new Vector3(max_Boom2X, boom2.localPosition.y, boom2.localPosition.z);

        // Boom 3 (Z axis)
        minBoom_3 = new Vector3(boom3.localPosition.x, boom3.localPosition.y, min_Boom3Z);
        maxBoom_3 = new Vector3(boom3.localPosition.x, boom3.localPosition.y, max_Boom3Z);

        // Boom 4 (Z axis)
        minBoom_4 = new Vector3(boom4.localPosition.x, boom4.localPosition.y, min_Boom4Z);
        maxBoom_4 = new Vector3(boom4.localPosition.x, boom4.localPosition.y, max_Boom4Z);
    }

    void Update()
    {
        bool isMoving = false;

        if (Input.GetKey(extendKey))
        {
            extending = true;
            MoveBooms();
            isMoving = true;
        }
        else if (Input.GetKey(retractKey))
        {
            extending = false;
            MoveBooms();
            isMoving = true;
        }

        //HandleMovementSound(isMoving);
    }

    private void MoveBooms()
    {
        if (extending)
        {
            
            boom4.localPosition = Vector3.MoveTowards(
                boom4.localPosition, maxBoom_4, extendSpeed * Time.deltaTime);

            
            if (Reached(boom4.localPosition, maxBoom_4))
            {
                boom3.localPosition = Vector3.MoveTowards(
                    boom3.localPosition, maxBoom_3, extendSpeed * Time.deltaTime);
            }

            
            if (Reached(boom3.localPosition, maxBoom_3))
            {
                boom2.localPosition = Vector3.MoveTowards(
                    boom2.localPosition, maxBoom_2, extendSpeed * Time.deltaTime);
            }
        }
        else
        {
           
            boom2.localPosition = Vector3.MoveTowards(
                boom2.localPosition, minBoom_2, extendSpeed * Time.deltaTime);

            
            if (Reached(boom2.localPosition, minBoom_2))
            {
                boom3.localPosition = Vector3.MoveTowards(
                    boom3.localPosition, minBoom_3, extendSpeed * Time.deltaTime);
            }

            
            if (Reached(boom3.localPosition, minBoom_3))
            {
                boom4.localPosition = Vector3.MoveTowards(
                    boom4.localPosition, minBoom_4, extendSpeed * Time.deltaTime);
            }
        }
    }

    private bool Reached(Vector3 a, Vector3 b)
    {
        return Vector3.Distance(a, b) < 0.001f;
    }

    //private void HandleMovementSound(bool isMoving)
    //{
    //    if (isMoving)
    //    {
    //        if (!moveAudio.isPlaying)
    //            moveAudio.Play();
    //    }
    //    else
    //    {
    //        if (moveAudio.isPlaying)
    //            moveAudio.Stop();
    //    }
    //}
}
