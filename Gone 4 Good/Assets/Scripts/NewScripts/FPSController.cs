using Cinemachine;
using System.Collections;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

public class FPSController : NetworkBehaviour, IActor
{
    public LayerMask groundLayer;
    [Header("References")]  
    public StatusManager sm;
    public Animator anim;
    public InteractionManager interactionManager;
    public Transform weaponPivot;
    public Transform fpsWeaponPivot;
    public Transform fpsgunbarrelEnd;
    public Transform gunBarrelEnd;

    public CinemachineVirtualCamera cinemachineCam;
    private Vector3 movementDirection;
    private float currentSpeed;
    private float ySpeed;
    private float gravity = 6.5f;
    private CharacterController cc;
    private CinemachineBasicMultiChannelPerlin viewBobing;
    private Vector3 lastSolidGround;
    private Inventory inventory;
    // PlayerSetup
    public GameObject playerSetup;
    private GameObject playerSetupInstance;


    public override void OnNetworkSpawn()
    {
        if (IsLocalPlayer)
        {
            PlayerSetupSetup();
            GameUI.instance.SyncHpBar(GetComponent<StatusManager>());
        }
        else
        {
            cinemachineCam.enabled = false;
            StartCoroutine(SyncAfterDelay());
        }
        base.OnNetworkSpawn();
    }
    public void Start()
    {
        NetworkGameManager.Instance.AddClient(NetworkObject.OwnerClientId, GetComponent<NetworkObject>());
        if (!IsLocalPlayer)
        {
            enabled = false;
            return;
        }

        sm = GetComponent<StatusManager>();
        cc = GetComponent<CharacterController>();
        inventory = GetComponent<Inventory>();
        viewBobing = cinemachineCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        //InputSystem.GetInputActionMapPlayer().Player.UseSelectedItem.performed += ctx => HandleItemUsage(true);
        //InputSystem.GetInputActionMapPlayer().Player.UseSelectedItem.canceled += ctx => HandleItemUsage(false);
        //InputSystem.GetInputActionMapPlayer().Player.FlashLight.performed += ctx => ToggleFlashLight();
        //InputSystem.GetInputActionMapPlayer().Player.DropEquipedItem.performed += ctx => DropEqipedItem();
        //InputSystem.GetInputActionMapPlayer().Player.Reload.performed += ctx => ReloadCurrentItem();

    }

    public void PlayerSetupSetup()
    {
        playerSetupInstance = Instantiate(playerSetup, transform.position, transform.rotation);
        GameUI gameUI = playerSetupInstance.GetComponentInChildren<GameUI>();
        gameUI.inventoryUI.syncedInventory = inventory;
    }

    private IEnumerator SyncAfterDelay()
    {
        while (GameUI.instance == null)
        {
            yield return new WaitForSeconds(0.1f);
        }
        GameUI.instance.SyncHpAllyBar(GetComponent<StatusManager>());
    }

    private void Update()
    {
        Movement();
        HandleGravity();
        HandleInteraction();

    }

    public void Movement()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        movementDirection = transform.right * horizontalInput + transform.forward * verticalInput;

        float inputMagnitude = Mathf.Clamp01(movementDirection.magnitude);

        bool shouldWalk = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (shouldWalk)
        {
            anim.SetFloat("AnimationSpeed", 1.3f);
        }
        else
        {
            anim.SetFloat("AnimationSpeed", 1);
        }

        currentSpeed = shouldWalk ? inputMagnitude * 1.333f : inputMagnitude;
        if (anim.GetBool("attack"))
        {
            currentSpeed *= 0.3f;
        }
        cc.Move(movementDirection * currentSpeed * 5 * Time.deltaTime);

        // Handle Viewbob Intensity
        viewBobing.m_FrequencyGain = Mathf.Clamp(currentSpeed*2,1,4);
    }

    public void HandleGravity()
    {
        bool isGrounded = IsGrounded();
        if (isGrounded && ySpeed <= 0.2f)
        {
            ySpeed = 0;
            //if (CurrentPlayerState != PlayerState.VoidOut)
            //{
            //    lastSolidGround = transform.position;
            //}
        }
        else
        {
            ySpeed += Physics.gravity.y * gravity * Time.deltaTime;
            cc.Move(new Vector3(0, ySpeed, 0) * Time.deltaTime);
        }
    }

    public bool IsGrounded()
    {
        // raycast to ground, ignoreing triggers
        return Physics.Raycast(transform.position + new Vector3(0, 0.15f, 0), Vector3.down, 0.3f, groundLayer);
    }

    public void HandleInteraction()
    {
        if (InputSystem.GetInputActionMapPlayer().Player.Interact.WasPressedThisFrame())
        {
            interactionManager.Interact(gameObject);
        }
    }
    public Transform WeaponPivot 
    { 
        get => weaponPivot; 
        set => weaponPivot = value; 
    }

    public Transform FPSWeaponPivot 
    {
        get => fpsWeaponPivot;
        set => fpsWeaponPivot = value;
    }
}
