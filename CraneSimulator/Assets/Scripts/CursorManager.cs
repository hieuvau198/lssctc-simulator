using UnityEngine;

public class CursorManager : MonoBehaviour
{
    void Start()
    {
        // Show the cursor
        Cursor.visible = false;
        // Unlock the cursor so it can move freely
        Cursor.lockState = CursorLockMode.Locked;
    }
}
