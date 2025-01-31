using Cinemachine;
using UnityEngine;

public class FPSCameraController : MonoBehaviour
{
    private float moseSensitivityModifier = 0.25f;
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
        Vector2 mouseMovement = InputSystem.GetInputActionMapPlayer().Camera.MouseMovement.ReadValue<Vector2>();
        // Mouse input
        float mouseX = mouseMovement.x * moseSensitivityModifier * Time.deltaTime * Options.mouseSensitivity;
        float mouseY = mouseMovement.y * moseSensitivityModifier * Time.deltaTime * Options.mouseSensitivity;

        // Rotate player body (left and right)
        playerBody.Rotate(Vector3.up * mouseX);

        // Rotate camera (up and down)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);  // Prevent over-rotation
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }


}
