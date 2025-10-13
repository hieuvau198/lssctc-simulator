using UnityEngine;

public class BoomController : MonoBehaviour
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

    void Update()
    {
        // Boom up/down input
        if (Input.GetKey(upBoom))
        {
            floatBoom += speedUpBoom * Time.deltaTime;
        }
        else if (Input.GetKey(downBoom))
        {
            floatBoom -= speedUpBoom * Time.deltaTime;
        }

        // Clamp rotation
        floatBoom = Mathf.Clamp(floatBoom, minValueBoom, maxValueBoom);

        // Smoothly rotate the boom around X axis (for up/down)
        Quaternion targetRotation = Quaternion.Euler(floatBoom, 0f, 0f);
        boom_1.localRotation = Quaternion.Lerp(boom_1.localRotation, targetRotation, Time.deltaTime / smoothBoomUp);
    }
    
}
