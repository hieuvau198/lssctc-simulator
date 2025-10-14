using UnityEngine;

public class BoomForward : MonoBehaviour
{
    [Header("Boom 2")]
    public Transform boom2;
    private Vector3 minBoom_2;
    private Vector3 maxBoom_2;
    public float min_Boom2F = 0;
    public float max_Boom2F = 0;
    [Header("Boom 3")]
    public Transform boom3;
    private Vector3 minBoom_3;
    private Vector3 maxBoom_3;
    public float min_Boom3F = 0;
    public float max_Boom3F = 0;
    [Header("Rope")]
    public Transform rope;

    [Header("Settings")]
    public KeyCode extendKey = KeyCode.Plus;   
    public KeyCode retractKey = KeyCode.Minus;   
    public float extendSpeed = 2f;
    private bool boomFor_Bool = true;

    void Start()
    {
        minBoom_2 = new Vector3(boom2.localPosition.x, min_Boom2F, boom2.localPosition.z);
        maxBoom_2 = new Vector3(boom2.localPosition.x, max_Boom2F, boom2.localPosition.z);

        minBoom_3 = new Vector3(boom3.localPosition.x, min_Boom3F, boom3.localPosition.z);
        maxBoom_3 = new Vector3(boom3.localPosition.x, max_Boom3F, boom3.localPosition.z);
    }

    void Update()
    {
        if (Input.GetKey(extendKey))
        {
            boomFor_Bool = false;
            BoomMoveForward();
        }
        else if (Input.GetKey(retractKey))
        {
            boomFor_Bool = true;
            BoomMoveForward();
        }
    }
    private void BoomMoveForward()
    {
        if(boomFor_Bool == true) 
        { 
            if(min_Boom2F == boom2.localPosition.y)
            {
                boom3.localPosition = Vector3.MoveTowards(boom3.localPosition, minBoom_3, extendSpeed * Time.deltaTime);
            }
        
            boom2.localPosition = Vector3.MoveTowards(boom2.localPosition, minBoom_2, extendSpeed * Time.deltaTime);
        }
        else if(boomFor_Bool == false)
        {
            
            boom3.localPosition = Vector3.MoveTowards(boom3.localPosition, maxBoom_3, extendSpeed * Time.deltaTime);

            if (max_Boom3F == boom3.localPosition.y)
            {
                boom2.localPosition = Vector3.MoveTowards(boom2.localPosition, maxBoom_2, extendSpeed * Time.deltaTime);
            }
        }

    }
    
}
