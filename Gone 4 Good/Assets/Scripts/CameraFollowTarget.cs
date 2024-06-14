using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollowTarget : MonoBehaviour
{
    public Transform target;
    private bool isRotating = false;
    // Start is called before the first frame update

    void Start()
    {
        transform.parent = null;
        InputSystem.GetInputActionMapPlayer().Camera.RotateCamera.started += HandleCameraRotationInput;
        InputSystem.GetInputActionMapPlayer().Camera.RotateCamera.canceled += HandleCameraRotationInput;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = target.position;
        if(isRotating)
        {
            transform.Rotate(Vector3.up, InputSystem.GetInputActionMapPlayer().Camera.MouseMovement.ReadValue<Vector2>().x, Space.World);
        }

    }



    public void HandleCameraRotationInput(InputAction.CallbackContext ctx)
    {
        if(ctx.started)
        {
            isRotating = true;
            Cursor.lockState = CursorLockMode.Locked;
        }
        if(ctx.canceled)
        {
            isRotating = false;
            Cursor.lockState = CursorLockMode.None;
        }
    }
    

    private void OnDisable()
    {
        InputSystem.GetInputActionMapPlayer().Camera.RotateCamera.started -= HandleCameraRotationInput;
        InputSystem.GetInputActionMapPlayer().Camera.RotateCamera.canceled -= HandleCameraRotationInput;
    }
}
