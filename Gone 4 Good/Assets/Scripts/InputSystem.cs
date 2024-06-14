using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystem : MonoBehaviour
{
    public PlayerInput input;
    public InputActionMapPlayer inputActionMapPlayer;

    private static InputSystem instance;
    // Start is called before the first frame update
    void Awake()
    {
        if (instance == this || instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        instance.inputActionMapPlayer = new InputActionMapPlayer();


    }

    private void OnEnable()
    {
        GetInputActionMapPlayer().Enable();
        GetInputActionMapPlayer().IngameUI.Inventory.performed += PrintDebugMessage;
    }

    public static InputActionMapPlayer GetInputActionMapPlayer()
    {
        if(instance == null)
        {
            instance = GameObject.FindObjectOfType<InputSystem>();
            instance.inputActionMapPlayer = new InputActionMapPlayer();
        }
        return instance.inputActionMapPlayer;
    }
    

    private void PrintDebugMessage(InputAction.CallbackContext ctx)
    {
        Debug.Log(ctx.action.name);
    }

    private void OnDisable()
    {
        GetInputActionMapPlayer().Disable();
        GetInputActionMapPlayer().IngameUI.Inventory.performed -= PrintDebugMessage;
    }

}
