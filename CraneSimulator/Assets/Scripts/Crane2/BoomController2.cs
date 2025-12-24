using UnityEngine;

public class BoomController2 : MonoBehaviour
{
    [Header("Up Down Boom Crane")]
    public Transform boom_1;
    public KeyCode upBoom;
    public KeyCode downBoom;
    public float speedUpBoom = 0f;
    public float smoothBoomUp = 0.5f;
    public float minValueBoom = 0f;
    public float maxValueBoom = 0f;

    

    private float floatBoom = 0f;

    private void Start()
    {
        floatBoom = boom_1.localEulerAngles.z;
        if (floatBoom > 180f)
            floatBoom -= 360f;
    }

    void Update()
    {
        bool isMoving = false;

        // Boom up/down input
        if (Input.GetKey(upBoom))
        {
            floatBoom += speedUpBoom * Time.deltaTime;
            isMoving = true;
        }
        else if (Input.GetKey(downBoom))
        {
            floatBoom -= speedUpBoom * Time.deltaTime;
            isMoving = true;
        }

        // Clamp rotation
        floatBoom = Mathf.Clamp(floatBoom, minValueBoom, maxValueBoom);

        // Smoothly rotate the boom around X axis (up/down)
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, floatBoom);
        boom_1.localRotation = Quaternion.Lerp(
            boom_1.localRotation,
            targetRotation,
            Time.deltaTime / smoothBoomUp
        );

        
    }

    public bool IsBoomMoving()
    {
        return Input.GetKey(upBoom) || Input.GetKey(downBoom);
    }

    
}
