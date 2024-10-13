using Cinemachine;
using UnityEngine;

public class FPSCameraController : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;


    private float xRotation = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;  // Lock cursor to center
        
    }

    // Update is called once per frame
    void Update()
    {
        // Mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate player body (left and right)
        playerBody.Rotate(Vector3.up * mouseX);

        // Rotate camera (up and down)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);  // Prevent over-rotation
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }


}
