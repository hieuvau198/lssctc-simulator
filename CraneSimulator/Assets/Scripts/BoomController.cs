using UnityEngine;

public class BoomController : MonoBehaviour
{
    [Header("Up Down Boom Crane")]
    public Transform boom_1;
    public Transform topPiston;
    public Transform bottomPiston;
    public KeyCode upBoom;
    public KeyCode downBoom;
    public float speedUpBoom = 0f;
    public float smoothBoomUp = 0.5f;
    public float minValueBoom = 0f;
    public float maxValueBoom = 0f;
    [Header("Piston Boom Crane")]
    public Transform piston_A;
    public Transform piston_B;
    private Transform _piston_A;
    private Transform _piston_B;

   

    private float floatBoom = 0f;
    private float pistonOffset = 0f;
    private void Start()
    {
        //Start Position end Rotation Piston
        GameObject p_A = new GameObject("PistonEmpty_A");
        _piston_A = p_A.transform;
        _piston_A.transform.localPosition = piston_A.position;
        _piston_A.SetParent(bottomPiston);
        GameObject p_B = new GameObject("PistonEmpty_B");
        _piston_B = p_B.transform;
        _piston_B.transform.localPosition = piston_B.position;
        _piston_B.SetParent(topPiston);
        _piston_A.LookAt(_piston_B.position, _piston_A.up);
        _piston_B.LookAt(_piston_A.position, _piston_B.up);
        piston_A.SetParent(_piston_A);
        piston_B.SetParent(_piston_B);


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

        // Smoothly rotate the boom around X axis (for up/down)
        Quaternion targetRotation = Quaternion.Euler(floatBoom, 0f, 0f);
        boom_1.localRotation = Quaternion.Lerp(boom_1.localRotation, targetRotation, Time.deltaTime / smoothBoomUp);
        

    }
    private void LateUpdate()
    {
        if (_piston_A != null && _piston_B != null)
        {
            _piston_A.LookAt(_piston_B.position, _piston_A.up);
            _piston_B.LookAt(_piston_A.position, _piston_B.up);
        }
    }

    public bool IsBoomMoving()
    {
        return Input.GetKey(upBoom) || Input.GetKey(downBoom);
    }
    
}
