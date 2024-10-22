using Cinemachine;
using System.Collections;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class FPSController : NetworkBehaviour, IActor
{
    public LayerMask groundLayer;
    [Header("References")]  
    public StatusManager sm;
    public Animator anim;
    public Animator fpsAnimator;
    public InteractionManager interactionManager;
    public Transform weaponPivot;
    public Transform fpsWeaponPivot;
    public Transform fpsgunbarrelEnd;
    public Transform gunBarrelEnd;
    public GameObject playerCamera;
    public GameObject fpsOverlayCamera;
    public Transform playerModel;
    public GameObject fpsPlayerModel;

    [Header("Item Usage")]
    public bool isReloading;

    public CinemachineVirtualCamera cinemachineCam;
    private Vector3 movementDirection;
    private float currentSpeed;
    private float ySpeed;
    private float gravity = 2.5f;
    public float groundedRaycastDistance = 0.5f;
    private CharacterController cc;
    private CinemachineBasicMultiChannelPerlin viewBobing;
    private Vector3 lastSolidGround;
    private Inventory inventory;
    // PlayerSetup
    public GameObject playerSetup;
    private GameObject playerSetupInstance;
    private float horizontalInput;
    private float verticalInput;

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
            playerCamera.GetComponent<Camera>().enabled = false; // Only turning camera off so the aimtarget can still sync
            fpsOverlayCamera.SetActive(false);
            fpsPlayerModel.SetActive(false);
            // set layer to PlayerInvisible
            enabled = false;
            return;
        }

        // Do all of the following only if local player

        // Setlayermask of playermodel and children to PlayerInvisible
        playerModel.gameObject.layer = LayerMask.NameToLayer("PlayerInvisible");
        foreach (Transform child in playerModel.GetComponentsInChildren<Transform>())
        {
            child.gameObject.layer = LayerMask.NameToLayer("PlayerInvisible");
        }

        sm = GetComponent<StatusManager>();
        cc = GetComponent<CharacterController>();
        inventory = GetComponent<Inventory>();
        viewBobing = cinemachineCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        InputSystem.GetInputActionMapPlayer().Player.UseSelectedItem.performed += ctx => HandleItemUsage(true);
        InputSystem.GetInputActionMapPlayer().Player.UseSelectedItem.canceled += ctx => HandleItemUsage(false);
        InputSystem.GetInputActionMapPlayer().Player.Jump.performed += ctx => HandleJump();
        //InputSystem.GetInputActionMapPlayer().Player.FlashLight.performed += ctx => ToggleFlashLight();
        //InputSystem.GetInputActionMapPlayer().Player.DropEquipedItem.performed += ctx => DropEqipedItem();
        InputSystem.GetInputActionMapPlayer().Player.Reload.performed += ctx => ReloadCurrentItem();

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
        Animation();
        CheckForItemUsage();
        if (inventory.CurrentHotbarItem.id != 0)
        {
            inventory.CurrentHotbarItem.GetItemInteractionEffects.ConstantUpdate(gameObject, inventory.CurrentHotbarItem);
        }
    }

    private void Animation()
    {
        anim.SetFloat("XDir", horizontalInput);
        anim.SetFloat("YDir", verticalInput);
    }

    public void Movement()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

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
        cc.Move(movementDirection * currentSpeed * 5 * Time.deltaTime);

        // Handle Viewbob Intensity
        viewBobing.m_FrequencyGain = Mathf.Clamp(currentSpeed*2,1,4);
    }

    public void HandleJump()
    {
        print("Jumping");
        if (IsGrounded())
        {
            ySpeed = 0;
            ySpeed += 7;
        }
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
        // spherecast to ground, ignoreing triggers
        return Physics.SphereCast(transform.position +new Vector3(0, 0.4f, 0), cc.radius, Vector3.down, out RaycastHit hit, groundedRaycastDistance, groundLayer);
    }

    public void HandleInteraction()
    {
        if (InputSystem.GetInputActionMapPlayer().Player.Interact.WasPressedThisFrame())
        {
            interactionManager.Interact(gameObject);
        }
    }

    public void HandleItemUsage(bool isUsing)
    {
        if (isUsing)
        {

            inventory.CurrentHotbarItem?.GetItemInteractionEffects.OnUsePerformed(gameObject, inventory.CurrentHotbarItem);
        }
        else
        {
            inventory.CurrentHotbarItem?.GetItemInteractionEffects.OnUseCancelled(gameObject, inventory.CurrentHotbarItem);
        }
    }
    public void CheckForItemUsage()
    {
        inventory.CurrentHotbarItem?.GetItemInteractionEffects?.OnUse(gameObject, inventory.CurrentHotbarItem);
    }

    public void ReloadCurrentItem()
    {

        if (((GunInteractionEffects)inventory.CurrentHotbarItem.GetItemInteractionEffects).CanReload(inventory.CurrentHotbarItem) && !isReloading)
        {
            anim.SetTrigger("Reload");
            isReloading = true;
            StartCoroutine(ReloadRoutine());

        }
    }
    public void TriggerAttack()
    {
        anim.SetTrigger("Attack");
        fpsAnimator.SetTrigger("Attack");
    }

    public IEnumerator ReloadRoutine()
    {
        yield return new WaitForSeconds(((GunInteractionEffects)inventory.CurrentHotbarItem.GetItemInteractionEffects).reloadTime);
        ((GunInteractionEffects)inventory.CurrentHotbarItem.GetItemInteractionEffects).Reload(inventory.CurrentHotbarItem);
        isReloading = false;
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
