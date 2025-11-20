using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCrane_L : MonoBehaviour
{
    public Transform targetCam;
    [HideInInspector]
    public float distance = 6f;
    private float horizontal = 0f;
    private float h = 0f;
    private float vertical = 0f;
    private float v = 0f;
    public float speed = 2;
    public float smooth = 0.5f;
    private float yMinLi = 0f;
    private float yMaxLi = 90f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        h += Input.GetAxis("Mouse X") * speed;
        horizontal = Mathf.Lerp(horizontal, h, Time.deltaTime * speed / smooth);
        v -= Input.GetAxis("Mouse Y") * speed;
        vertical = Mathf.Lerp(vertical, v, Time.deltaTime * speed / smooth);
        vertical = Mathf.Clamp(vertical, yMinLi, yMaxLi);

        Quaternion rotation = Quaternion.Euler(vertical, horizontal, 0);
        Vector3 position = rotation * new Vector3(0f, 0f, -distance) + targetCam.position;
        transform.rotation = rotation;
        transform.position = position;
    }
}
