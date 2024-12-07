using Cinemachine;
using System.Collections;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class FPSController : NetworkBehaviour, IActor
{
    public LayerMask groundLayer;

    public NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>("string.Empty",NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [Header("References")]  
    public StatusManager sm;
    public Animator anim;
    public Animator fpsAnimator;
    public InteractionManager interactionManager;
    public Transform weaponPivot;
    public Transform fpsWeaponPivot;
    public Transform fpsgunbarrelEnd;
    public Transform gunBarrelEnd;
    public GameObject equipedObject;
    public GameObject playerCamera;
    public GameObject fpsOverlayCamera;
    public Transform playerModel;
    public GameObject fpsPlayerModel;
    public GameObject playerRemnant;
    public Animator CameraAnimator;
    public GameObject flashLightRef;
    public NetworkVariable<bool> flashLight = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);

    [Header("Item Usage")]
    public bool isReloading;

    public CinemachineVirtualCamera cinemachineCam;
    private float cameraFOV = 60;
    private Vector3 movementDirection;
    private float currentSpeed;
    public float acceleration = 0.5f;
    private float currentAcceleration = 0;
    private float ySpeed;
    private float gravity = 2.5f;
    public float groundedRaycastDistance = 0.5f;
    private CharacterController cc;
    private CinemachineBasicMultiChannelPerlin viewBobing;
    private Vector3 lastSolidGround;
    public Inventory inventory;
    // PlayerSetup
    public GameObject playerSetup;
    private GameObject playerSetupInstance;
    private float horizontalInput;
    private float verticalInput;
    public TextMeshProUGUI playerNameCard;

    public override void OnNetworkSpawn()
    {
        playerName.OnValueChanged += (oldValue, newValue) =>
        {
            UpdateNameCardRpc();
        };
        UpdateNameCardRpc();
        if (IsLocalPlayer)
        {
            playerName.Value = PlayerPrefs.GetString("PlayerName", "Unknown");
            PlayerSetupSetup();
            GameUI.instance.SyncHpBar(GetComponent<StatusManager>());
            playerNameCard.gameObject.SetActive(false);
        }
        else
        {
            cinemachineCam.enabled = false;
            StartCoroutine(SyncAfterDelay());
        }
        base.OnNetworkSpawn();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void UpdateNameCardRpc()
    {
        playerNameCard.text = playerName.Value.ToString();
    }

    public void Start()
    {
        NetworkGameManager.Instance.AddClient(NetworkObject.OwnerClientId, GetComponent<NetworkObject>());
        if (!IsLocalPlayer)
        {
            CameraAnimator.gameObject.SetActive(false);
            playerCamera.GetComponent<Camera>().enabled = false; // Only turning camera off so the aimtarget can still sync
            playerCamera.GetComponent<AudioListener>().enabled = false;
            fpsOverlayCamera.SetActive(false);
            fpsPlayerModel.SetActive(false);
            interactionManager.gameObject.SetActive(false);
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
        viewBobing = cinemachineCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        sm.OnStaminaUpdate.AddListener(UpdateStamina);
        InputSystem.GetInputActionMapPlayer().Player.Hotkey1.performed += ctx => SwitchHotbarItem(0);
        InputSystem.GetInputActionMapPlayer().Player.Hotkey2.performed += ctx => SwitchHotbarItem(1);
        InputSystem.GetInputActionMapPlayer().Player.UseSelectedItem.performed += ctx => HandleItemUsage(true);
        InputSystem.GetInputActionMapPlayer().Player.UseSelectedItem.canceled += ctx => HandleItemUsage(false);
        InputSystem.GetInputActionMapPlayer().Player.Jump.performed += ctx => HandleJump();
        InputSystem.GetInputActionMapPlayer().Player.FlashLight.performed += ctx => ToggleFlashLight();
        InputSystem.GetInputActionMapPlayer().Player.DropEquipedItem.performed += ctx => DropEqipedItem();
        InputSystem.GetInputActionMapPlayer().Player.Reload.performed += ctx => ReloadCurrentItem();

        PerformanceTracker.StartNewStack("Level", playerName.Value.ToString(), "Performance of the Player in the Main Scenairo Level.");
        sm.OnDamageOwner.AddListener(TrackDamageReceived);
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint");
        if(spawnPoint != null)
        {
            Teleport(spawnPoint.transform.position);
        }
        else
        {
            Debug.LogError("No spawnpoint found");
        }

    }

    public void SwitchHotbarItem(int index)
    {
        inventory.SwitchHotbarItem(index);
        InventoryUI.Instance.UpdateUI(0);
    }

    private void UpdateStamina()
    {
        GameUI.instance.playerStaminaBar.SetStamina((float)sm.Stamina / sm.maxStamina);
    }

    private void DropEqipedItem()
    {
        if (inventory.CurrentHotbarItem.id != 0)
        {
            print("Dropping item");
            Item item = inventory.CurrentHotbarItem;
            item.GetItemInteractionEffects.OnDrop(gameObject, inventory.CurrentHotbarItem);
            NetworkItemData data = new NetworkItemData()
            {
                affinity = 0,
                currentAmmo = item.currentAmmo,
                currentClip = item.currentClip
            };
            DropEquipedItemRpc(playerCamera.transform.position,playerCamera.transform.forward, inventory.CurrentHotbarItem.id, data);
            inventory.items[inventory.currentHotbarIndex] = new Item(0, 0);
            inventory.onInventoryUpdate?.Invoke(inventory.currentHotbarIndex);
        }
    }

    private void ToggleFlashLight()
    {
        flashLight.Value = !flashLight.Value;
        ToggleFlashLightRpc(flashLight.Value);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ToggleFlashLightRpc(bool value)
    {
        flashLightRef.SetActive(value);
    }

    [Rpc(SendTo.Server)]
    public void DropEquipedItemRpc(Vector3 pos, Vector3 direction, int itemId, NetworkItemData networkItemData)
    {
        print(ItemDatabase.instance.items[itemId].droppedPrefab);
        GameObject _droppedItem = Instantiate(ItemDatabase.instance.items[itemId].droppedPrefab, pos, Quaternion.identity);
        NetworkObject networkObject = _droppedItem.GetComponent<NetworkObject>();
        _droppedItem.GetComponent<Interactable_NetworkItem>().networkItemData = networkItemData;
        networkObject.Spawn();
        Rigidbody rb = _droppedItem.GetComponent<Rigidbody>();
        rb.angularVelocity = new Vector3(UnityEngine.Random.Range(-10, 10), UnityEngine.Random.Range(-10, 10), UnityEngine.Random.Range(-10, 10));
        rb.linearVelocity = UnityEngine.Random.Range(4, 7) * direction;

    }

    public void PlayerSetupSetup()
    {
        playerSetupInstance = Instantiate(playerSetup, transform.position, transform.rotation);
        GameUI gameUI = playerSetupInstance.GetComponentInChildren<GameUI>();
        gameUI.inventoryUI.syncedInventory = inventory;
        gameUI.inventoryUI.syncedInventory.onInventoryUpdate += gameUI.inventoryUI.UpdateUI;
        flashLightRef.transform.parent = playerCamera.transform;
    }

    private IEnumerator SyncAfterDelay()
    {
        while (GameUI.instance == null)
        {
            yield return new WaitForSeconds(0.25f);
        }
        GameUI.instance.SyncHpAllyBar(GetComponent<StatusManager>());

    }

    private void Update()
    {
        HandleGravity();
        Animation();
        if (inventory.CurrentHotbarItem.id != 0)
        {
            inventory.CurrentHotbarItem.GetItemInteractionEffects.ConstantUpdate(gameObject, inventory.CurrentHotbarItem);
        }
        if (sm.Hp.Value <= 0)
        {
            return;
        }
        Movement();
        HandleInteraction();
        CheckForItemUsage();

    }

    private void Animation()
    {
        anim.SetFloat("XDir", horizontalInput);
        anim.SetFloat("YDir", verticalInput);
    }

    [Rpc(SendTo.Owner)]
    public void TriggerDamageIndicatorsRpc(Vector3 originPosition)
    {
        Direction dir = Direction.Front;
        Vector3 direction = (transform.position - originPosition).normalized;
        float angle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
        if (angle > 45 && angle < 135)
        {
            dir = Direction.Right;
        }
        else if (angle < -45 && angle > -135)
        {
            dir = Direction.Left;
        }
        else if (angle > 135 || angle < -135)
        {
            dir = Direction.Front;
        }
        else if (angle < 45 && angle > -45)
        {
            dir = Direction.Back;
        }
        GameUI.instance.TriggerDamageIndicator(dir);
    }

    public void Teleport(Vector3 position)
    {
        cc.enabled = false;
        transform.position = position;
        cc.enabled = true;
    }
    public void Movement()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        if(horizontalInput != 0 || verticalInput != 0)
        {
            currentAcceleration = Mathf.Clamp01(currentAcceleration + acceleration * Time.deltaTime);
            movementDirection = transform.right * horizontalInput + transform.forward * verticalInput;
        }
        else
        {
            currentAcceleration = Mathf.Clamp01(currentAcceleration - acceleration * Time.deltaTime);
        }


        float inputMagnitude = Mathf.Clamp01(movementDirection.magnitude);

        bool sprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (sprinting)
        {
            sm.staminaConsumptionPerSecond = 20;
            anim.SetFloat("AnimationSpeed", 1.3f);
            if(sm.Stamina > 0)
            {
                CameraFOV = 70;
                currentSpeed = inputMagnitude * 1.25f;
            }
            else
            {
                CameraFOV = 60;
                currentSpeed = inputMagnitude;
            }
        }
        else
        {
            sm.staminaConsumptionPerSecond = 0;
            anim.SetFloat("AnimationSpeed", 1);
            currentSpeed = inputMagnitude;
            CameraFOV = 60;
        }
        float calculatedSpeed = currentSpeed * currentAcceleration * sm.MovementSpeedMultiplier * Time.deltaTime * 3.55f;
        PerformanceTracker.instance.currentStack.distanceMoved += calculatedSpeed;
        if(sprinting && sm.Stamina > 0)
        {
            PerformanceTracker.instance.currentStack.distanceSprinted += calculatedSpeed;
        }
        
        cc.Move(movementDirection * calculatedSpeed);

        // Handle Viewbob Intensity
        viewBobing.m_FrequencyGain = Mathf.Clamp(currentSpeed * currentAcceleration * 1.5f, 0,3);
    }

    public void HandleJump()
    {
        if (sm.Hp.Value <= 0) return;
        if (IsGrounded())
        {
            ySpeed = 0;
            ySpeed += 7;
        }
    }

    [Rpc(SendTo.Owner)]
    public void TrackEnemiesKilledRpc()
    {
        PerformanceTracker.instance.currentStack.enemiesKilled++;
    }

    [Rpc(SendTo.Owner)]
    public void TrackDamageDealtRpc(int damage)
    {
        PerformanceTracker.instance.currentStack.damageDealt += damage;
    }

    public void TrackDamageReceived(int damage)
    {
        PerformanceTracker.instance.currentStack.damageReceived += damage;
        PerformanceTracker.instance.currentStack.timesHit++;
    }

    public void TriggerRemnantTransformation()
    {
        if (!IsServer) return;
        BecomeRemnantRpc();
        BecomeRemnantLocalRpc();
    }

    public void RecoverFromRemnantTransformation()
    {
        RecoverFromRemnantTransformationRpc();
    }

    [Rpc(SendTo.Owner)]
    public void RecoverFromRemnantTransformationRpc()
    {
        if(sm != null)
        {
            if (sm.Hp.Value > 0) return;
            sm.HealHpRpc(30 + UnityEngine.Random.Range(0, 10));
        }
        RecoverFromRemnantRemnantRpc();
        RecoverFromRemnantLocalRpc();
    }

    [Rpc(SendTo.NotOwner)]
    private void BecomeRemnantRpc()
    {
        playerRemnant.SetActive(true);
        playerModel.gameObject.SetActive(false);
    }
    [Rpc(SendTo.Owner)]
    private void BecomeRemnantLocalRpc()
    {
        CameraAnimator.SetInteger("CameraState", 1);
        playerRemnant.SetActive(true);
        playerModel.gameObject.SetActive(false);
        PerformanceTracker.instance.currentStack.timesDowned++;
    }
    [Rpc(SendTo.NotOwner)]
    private void RecoverFromRemnantRemnantRpc()
    {
        playerRemnant.SetActive(false);
        playerModel.gameObject.SetActive(true);
    }

    [Rpc(SendTo.Owner)]
    private void RecoverFromRemnantLocalRpc()
    {
        CameraAnimator.SetInteger("CameraState", 0);
        playerRemnant.SetActive(false);
        playerModel.gameObject.SetActive(true);
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
            fpsAnimator.SetTrigger("Reload");
            isReloading = true;
            StartCoroutine(ReloadRoutine());

        }
    }
    public void TriggerAttack(bool value)
    {
        anim.SetBool("Attack", value);
        fpsAnimator.SetBool("Attack", value);
    }

    public IEnumerator ReloadRoutine()
    {
        yield return new WaitForSeconds(((GunInteractionEffects)inventory.CurrentHotbarItem.GetItemInteractionEffects).reloadTime);
        isReloading = false;
        ((GunInteractionEffects)inventory.CurrentHotbarItem.GetItemInteractionEffects).Reload(inventory.CurrentHotbarItem);
    }

    [Rpc(SendTo.Owner)]
    public void UpdateRemnantRevivalBarUIRpc(ulong id, float fillAmount)
    {
        
        FPSController player = NetworkGameManager.GetPlayerById(id).GetComponent<FPSController>();
        RemnantRevivalBarUI bar = GameUI.instance.remnantRevivalBarUI;
        bar.SetBar(player.playerName.Value.ToString(), fillAmount);
        if(fillAmount == 0)
        {
            bar.HideBar();
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
    public float CameraFOV 
    { 
        get => cameraFOV; 
        set
        {
            // Lerp Camera FOV
            cameraFOV = Mathf.Lerp(cameraFOV, value, 0.25f);
            cinemachineCam.m_Lens.FieldOfView = cameraFOV;
        }
    }
}
