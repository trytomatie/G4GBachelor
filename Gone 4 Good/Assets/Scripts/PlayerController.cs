using Cinemachine;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using static PlayerController;

public partial class PlayerController : NetworkBehaviour, IEntityControlls
{
    // Character Movement Properties
    [SerializeField] private float maxMovementSpeed = 6;
    [SerializeField] private float rotationSpeed = 40;
    private float deceleration = 0.15f;
    public float ySpeed = 0;
    private float gravity = 1f;
    private float currentSpeed;
    public Vector3 movementDirection;
    private Vector2 rotationRelativeDirection;
    public Vector3 rootMotionMotion;
    public LayerMask groundLayer;
    // References
    public CharacterController characterController;
    public CinemachineVirtualCamera vCam;
    public ParticleSystem walkDust;
    public InteractionManager interactionManager;
    public Transform weaponPivot;
    public Transform gunBarrelEnd;
    public Transform cameraFollowTarget;
    public Transform aimFollowTarget;
    private StatusManager sm;
    private Inventory inventory;
    public GameObject flashLightRef;

    // Flashlight
    private NetworkVariable<bool> flashLight = new NetworkVariable<bool>(false,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);


    [Header("HitBoxes")]
    public GameObject[] hitBoxes;
    public Transform vfxTransform;
    public Animator anim;
    private Transform cameraTransform;

    public Vector3 lastSolidGround = Vector3.zero;

    [Header("Skills")]
    public int skillIndex = -1;
    public Skill[] skills;
    private float[] skillSlotCooldowns = new float[3] { -999, -999, -999 };

    // PlayerSetup
    public GameObject playerSetup;
    private GameObject playerSetupInstance;


    // States
    public enum PlayerState
    {
        Controlling,
        InWater,
        Running,
        Attacking,
        PlayerUsingSkill
    }
    public PlayerState currentPlayerState = PlayerState.Controlling;

    private State[] states = new State[5];

    // Start is called before the first frame update
    void Start()
    {
        NetworkGameManager.Instance.AddClient(NetworkObject.OwnerClientId, GetComponent<NetworkObject>());
        ToggleFlashLightRpc(flashLight.Value);
        if (!IsLocalPlayer)
        {
            enabled = false;
            return;
        }
        PlayerSetupSetup();
        states[(int)PlayerState.Controlling] = new PlayerStateControlling();
        states[(int)PlayerState.InWater] = new PlayerStateInWater();
        states[(int)PlayerState.PlayerUsingSkill] = new PlayerUsingSkill();
        states[(int)currentPlayerState].OnEnter(this);
        cameraTransform = Camera.main.transform;
        inventory = GetComponent<Inventory>();
        sm = GetComponent<StatusManager>();
        foreach(SkillSlotUI skillSlotUI in GameUI.instance.skillslots)
        {
            skillSlotUI.BindEntity(this);
        }

        InputSystem.GetInputActionMapPlayer().Player.Hotkey1.performed += ctx => SwitchHotbarItem(0);
        InputSystem.GetInputActionMapPlayer().Player.Hotkey2.performed += ctx => SwitchHotbarItem(1);
        InputSystem.GetInputActionMapPlayer().Player.Hotkey3.performed += ctx => SwitchHotbarItem(2);
        InputSystem.GetInputActionMapPlayer().Player.Hotkey4.performed += ctx => SwitchHotbarItem(3);
        InputSystem.GetInputActionMapPlayer().Player.Hotkey5.performed += ctx => SwitchHotbarItem(4);
        InputSystem.GetInputActionMapPlayer().Player.Hotkey6.performed += ctx => SwitchHotbarItem(5);
        InputSystem.GetInputActionMapPlayer().Player.Hotkey7.performed += ctx => SwitchHotbarItem(6);
        InputSystem.GetInputActionMapPlayer().Player.UseSelectedItem.performed += ctx => HandleItemUsage(true);
        InputSystem.GetInputActionMapPlayer().Player.UseSelectedItem.canceled += ctx => HandleItemUsage(false);
        InputSystem.GetInputActionMapPlayer().Player.FlashLight.performed += ctx => ToggleFlashLight();

        for (int i = 0; i < skills.Length; i++)
        {
            if (skills[i] != null)
            {
                skills[i] = Instantiate(skills[i]);
                GameUI.instance.skillslots[i].SetupSkill(skills[i]);
            }

        }

        if (inventory.items[0].id != 0) SwitchHotbarItem(0);
    }

    public void PlayerSetupSetup()
    {
        playerSetupInstance = Instantiate(playerSetup, transform.position,transform.rotation);
        vCam = playerSetupInstance.GetComponentInChildren<CinemachineVirtualCamera>();
        vCam.LookAt = cameraFollowTarget;
        vCam.Follow = cameraFollowTarget;
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

    public void SwitchHotbarItem(int index)
    {
        if(currentPlayerState == PlayerState.PlayerUsingSkill)
        {
            return;
        }
        inventory.SwitchHotbarItem(index);
    }

    public void SwitchPlayerState(PlayerState newState)
    {
        SwitchPlayerState(newState, false);
    }

    public void SwitchPlayerState(PlayerState newState, bool force)
    {
        if (!force)
        {
            CurrentPlayerState = newState;
        }
        else
        {

            if (CurrentPlayerState == newState)
            {
                states[(int)currentPlayerState].OnExit(this);
                states[(int)currentPlayerState].OnEnter(this);
            }
            CurrentPlayerState = newState;
        }

    }

    public void SwitchPlayerState()
    {
        CurrentPlayerState = PlayerState.Controlling;
    }

    // Update is called once per frame
    void Update()
    {
        states[(int)currentPlayerState].OnUpdate(this);
        CheckForPlayerVoidOut();
        if(inventory.CurrentHotbarItem.id != 0)
        {
            inventory.CurrentHotbarItem.GetItemInteractionEffects.ConstantUpdate(gameObject,inventory.CurrentHotbarItem);
        }
    }

    public void HandleInteraction()
    {
        if (InputSystem.GetInputActionMapPlayer().Player.Interact.WasPressedThisFrame())
        {
            print("Interacting");
            interactionManager.Interact();

        }
    }

    public void HandleGravity()
    {
        bool isGrounded = IsGrounded();
        if (isGrounded && ySpeed <= 0.2f)
        {
            ySpeed = 0;
            lastSolidGround = transform.position;
        }
        else
        {
            ySpeed += Physics.gravity.y * gravity * Time.deltaTime;
            characterController.Move(new Vector3(0, ySpeed, 0) * Time.deltaTime);
        }
    }

    public bool IsGrounded()
    {
        return Physics.Raycast(transform.position + new Vector3(0, 0.05f, 0), Vector3.down, 0.1f, groundLayer);
    }

    public void Rotation()
    {
        // Rotate the character to movement direction
        if (movementDirection != Vector3.zero)
        {

            Quaternion targetCharacterRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetCharacterRotation, rotationSpeed * Time.deltaTime);



        }
        return;
        if (!anim.GetBool("attack"))
        {

        }
        else
        {
            Vector3 mousePosition = Input.mousePosition;
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            float hitDistance;

            if (groundPlane.Raycast(ray, out hitDistance))
            {
                Vector3 cursorPosition = ray.GetPoint(hitDistance);

                Vector3 direction = cursorPosition - transform.position;
                direction.Normalize();
                Quaternion targetCharacterRotation = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetCharacterRotation, 10000 * Time.deltaTime);
            }
        }
    }

    public void Death()
    {
        anim.SetBool("Death", true);
        AudioManager.PlayRandomSoundFromList(AudioManager.instance.playerDeath, transform.position);
        StartCoroutine(DeathRoutine());

    }

    IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(3);
        TransitionScreenControler.instance.CallTransition();
        yield return new WaitForSeconds(1f);
        sm.Hp.Value = sm.maxHp;
        characterController.enabled = false;
        TeleportToDungeonLayer(false);
        transform.position = new Vector3(0,0,0);
        characterController.enabled = true;
        anim.SetBool("Death", false);
        yield return new WaitForSeconds(0.5f);
        TransitionScreenControler.instance.DismissTransition();
    }

    public void Animations()
    {
        anim.SetFloat("XDir", Mathf.RoundToInt(rotationRelativeDirection.x),0.1f,Time.deltaTime);
        anim.SetFloat("YDir", Mathf.RoundToInt(rotationRelativeDirection.y), 0.1f, Time.deltaTime);
    }

    public void Movement()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        movementDirection = new Vector3(horizontalInput, 0, verticalInput);
        movementDirection.Normalize();

        float inputMagnitude = Mathf.Clamp01(movementDirection.magnitude);

        bool shouldWalk = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        currentSpeed = shouldWalk ? inputMagnitude * 0.333f : inputMagnitude;
        if (anim.GetBool("attack"))
        {
            currentSpeed *= 0.3f;
        }
        if (currentSpeed > 0.5f)
        {
            walkDust.Play();
        }
        else
        {
            walkDust.Stop();
        }

        movementDirection = Quaternion.AngleAxis(cameraTransform.rotation.eulerAngles.y, Vector3.up)
    * movementDirection;

        Vector3 finalMovementDirection = movementDirection * currentSpeed;
        finalMovementDirection.y = 0;
        characterController.Move((finalMovementDirection * maxMovementSpeed * Time.deltaTime) + rootMotionMotion);

        rootMotionMotion = Vector3.zero;

        // Calculation for Animation Parameters
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        Vector3 movement = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0) * new Vector3(horizontalInput, 0, verticalInput);
        float dot = Vector3.Dot(forward, movement.normalized);
        float dotRight = Vector3.Dot(right, movement.normalized);

        float maxDot = Mathf.Max(Mathf.Abs(dotRight), Mathf.Abs(dot));
        if (maxDot == Mathf.Abs(dot))
        {
            dotRight = 0;
        }
        else
        {
            dot = 0;
        }
        rotationRelativeDirection = new Vector2(dotRight, dot);
    }
    public void HandleAttack(bool handleAttack)
    {
        if (!EventSystem.current.IsPointerOverGameObject()  && handleAttack)
        {
            anim.SetBool("attack", true);
        }
        else
        {
            anim.SetBool("attack", false);
        }
    }

    private GameObject staffVFXRef = null;
    private float attackChargeTime = 1;
    private float attackChargeTimer = 0;
    public void HandleStaffCharge(bool isChargeing)
    {
        if (!BuildingManager.instance.PlaceBuildingMode && isChargeing)
        {
            if (anim.GetBool("chrageStaff") == false)
            {
                staffVFXRef = VFXManager.Instance.PlayFeedback(4, gunBarrelEnd);
            }
            attackChargeTimer += Time.deltaTime;
            CastRotation();
            anim.SetBool("chrageStaff", true);
        }
        else
        {
            if (attackChargeTimer >= attackChargeTime)
            {
                FireMagicAttack();
                attackChargeTimer = 0;
            }
            if (staffVFXRef != null)
            {
                staffVFXRef.GetComponent<ParticleSystem>().Stop();
                Destroy(staffVFXRef, 3);
            }
            anim.SetBool("chrageStaff", false);
        }
    }

    private void FireMagicAttack()
    {
        Instantiate(hitBoxes[1], hitBoxes[1].transform.position, hitBoxes[1].transform.rotation).SetActive(true);
        GameObject vfx = VFXManager.Instance.PlayFeedback(5, gunBarrelEnd);
        AudioManager.PlayGeneralSound(transform.position, 2);
        Destroy(vfx, 11);
    }

    public void CastRotation()
    {
        Vector3 mousePosition = Input.mousePosition;
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        groundPlane.SetNormalAndPosition(Vector3.up, transform.position);
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        float hitDistance;

        if (groundPlane.Raycast(ray, out hitDistance))
        {
            Vector3 cursorPosition = ray.GetPoint(hitDistance);

            Vector3 direction = cursorPosition - transform.position;
            direction.Normalize();
            Quaternion targetCharacterRotation = Quaternion.LookRotation(direction, Vector3.up);
            // Translate to euler angles
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetCharacterRotation, 10000 * Time.deltaTime);
            aimFollowTarget.transform.position = ray.GetPoint(hitDistance);
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

    public void RespawnAtLastSolidGround()
    {
        characterController.enabled = false;
        transform.position = lastSolidGround;
        characterController.enabled = true;
    }

    public void TeleportToDungeonLayer(bool enter)
    {
        print(enter);
        //vCam.GetCinemachineComponent<CinemachineTransposer>().m_XDamping = 0;
        //vCam.GetCinemachineComponent<CinemachineTransposer>().m_YDamping = 0;
        //vCam.GetCinemachineComponent<CinemachineTransposer>().m_ZDamping = 0;
        if (enter)
        {
            GameManager.Instance.overWorld.transform.position = new Vector3(0, -50, 0);
            GameManager.Instance.dungeonWorld.transform.position = new Vector3(0, 0, 0);
        }
        else
        {
            GameManager.Instance.overWorld.transform.position = new Vector3(0, 0, 0);
            GameManager.Instance.dungeonWorld.transform.position = new Vector3(0, -50, 0);
        }
        foreach (EnemyAI enemy in EnemyAI.enemyAIList)
        {
            if (enemy.transform.position.y < -20)
            {
                enemy.gameObject.SetActive(false);
            }
            else
            {
                enemy.gameObject.SetActive(true);
            }
            enemy.agent.enabled = false;
            enemy.agent.enabled = true;
        }
        // Invoke("ResetCamera", 0.1f);
    }

    private void ResetCamera()
    {
        vCam.GetCinemachineComponent<CinemachineTransposer>().m_XDamping = 1;
        vCam.GetCinemachineComponent<CinemachineTransposer>().m_YDamping = 1;
        vCam.GetCinemachineComponent<CinemachineTransposer>().m_ZDamping = 1;
    }

    #region Skill
    public void UseSkill(InputAction.CallbackContext ctx)
    {
        switch(ctx.action.name)
        {
            case "Skill1":
                skillIndex = 0;
                break;
            case "Skill2":
                skillIndex = 1;
                break;
            case "Air Step":
                skillIndex = 2;
                break;
            default: return;
        }
        if (skills[skillIndex] != null && skills[skillIndex].CheckSkillConditions(gameObject))
        {
            //GameUI.instance.skillslots[skillIndex].skillCooldown = skills[skillIndex].skillCooldown;
            SwitchPlayerState(PlayerState.PlayerUsingSkill);
        }
    }
    #endregion

    public bool CheckSkillUsage()
    {
        switch(skillIndex)
        {
            case 0:
                return InputSystem.GetInputActionMapPlayer().Player.Skill1.inProgress;
                case 1:
                return InputSystem.GetInputActionMapPlayer().Player.Skill2.inProgress;
                case 2:
                return InputSystem.GetInputActionMapPlayer().Player.AirStep.inProgress;
        }
        return false;
    }

    //#region AirStepping
    //public void AirStep(Vector3 airStepDirection, float delta)
    //{
    //    if (airStepDirection == Vector3.zero)
    //    {
    //        return;
    //    }
    //    float airStepDistance = this.airStepDistance;
    //    float airStepCurveValue = airStepCurve.Evaluate(delta / airStepDuration);
    //    Vector3 airStepVector = airStepDirection * airStepDistance * airStepCurveValue;
    //    Movement(airStepVector * Time.deltaTime);
    //}

    //public void CheckAirStepConditions(InputAction.CallbackContext ctx)
    //{
    //    if (lastDashTime + dashCooldown < Time.time)
    //    {
    //        if (movementDirection != Vector3.zero && dashCount < dashLimit)
    //        {
    //            SwitchPlayerState(PlayerState.AirStepping, true);
    //        }
    //    }
    //}
    //#endregion
    public void CheckForPlayerVoidOut()
    {
        if (transform.position.y < -15)
        {
            CurrentPlayerState = PlayerState.InWater;
        }
    }

    public PlayerState CurrentPlayerState
    {
        get => currentPlayerState;
        set
        {
            if (currentPlayerState != value)
            {
                states[(int)currentPlayerState].OnExit(this);
                states[(int)value].OnEnter(this);
                currentPlayerState = value;
            }

        }
    }



    public void ItemUsage()
    {
        inventory.CurrentHotbarItem?.GetItemInteractionEffects?.OnUse(gameObject, inventory.CurrentHotbarItem);
    }
    #region Interface
    public void Movement(Vector3 movement)
    {
        characterController.Move(movement);
    }

    public Animator GetAnimator()
    {
        return anim;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public Vector3 GetMovmentDirection()
    {
        return movementDirection;
    }

    public void SwitchState(PlayerState controlling)
    {
        SwitchPlayerState(controlling);
    }

    public void ManualMovement()
    {
        Movement();
    }

    public float[] SkillColldowns { get => skillSlotCooldowns; set => skillSlotCooldowns = value; }
    public int SkillIndex { get => skillIndex; set => skillIndex = value; }
    public StatusManager StatusManager { get => sm; set => sm = value; }
    public bool HoldingSkill { get => CheckSkillUsage();}

    public Transform VfxTransform { get => vfxTransform; set => vfxTransform = value; }

    public Transform CastingPivot => gunBarrelEnd;
    #endregion
}
public interface State
{
    void OnEnter(PlayerController pc);
    void OnExit(PlayerController pc);
    void OnUpdate(PlayerController pc);
}

public class PlayerStateControlling : State
{
    public void OnEnter(PlayerController pc)
    {
        InputSystem.GetInputActionMapPlayer().Player.AirStep.performed += pc.UseSkill;
        InputSystem.GetInputActionMapPlayer().Player.Skill1.performed += pc.UseSkill;
        InputSystem.GetInputActionMapPlayer().Player.Skill2.performed += pc.UseSkill;
    }

    public void OnExit(PlayerController pc)
    {
        pc.walkDust.Stop();
        InputSystem.GetInputActionMapPlayer().Player.AirStep.performed -= pc.UseSkill;
        InputSystem.GetInputActionMapPlayer().Player.Skill1.performed -= pc.UseSkill;
        InputSystem.GetInputActionMapPlayer().Player.Skill2.performed -= pc.UseSkill;
    }

    public void OnUpdate(PlayerController pc)
    {
        pc.Movement();
        pc.CastRotation();
        pc.Animations();
        pc.ItemUsage();
        pc.HandleGravity();
        pc.HandleInteraction();
    }
}

public class PlayerStateInWater : State
{
    private Transform initialFollowTarget;
    public void OnEnter(PlayerController pc)
    {
        pc.Invoke("RespawnAtLastSolidGround", 1.5f);
        pc.Invoke("SwitchPlayerState", 1.8f);
        initialFollowTarget = pc.vCam.Follow;
        pc.vCam.Follow = null;
    }

    public void OnExit(PlayerController pc)
    {
        pc.vCam.Follow = initialFollowTarget;
    }

    public void OnUpdate(PlayerController pc)
    {
        pc.Animations();
        pc.HandleGravity();
    }
}

public class PlayerUsingSkill : State
{
    public void OnEnter(PlayerController pc)
    {
        pc.skills[pc.skillIndex].OnEnter(pc.gameObject);
    }

    public void OnExit(PlayerController pc)
    {
        pc.skills[pc.skillIndex].OnExit(pc.gameObject);
    }

    public void OnUpdate(PlayerController pc)
    {
        pc.skills[pc.skillIndex].OnUpdate(pc.gameObject);
        pc.ItemUsage();
    }
}

/*
public class PlayerStateAirStepping : State
{
    private float onEnterTime;
    private Vector3 facingDirection;
    private float airStepInputLockoutTime = 0.9f; // Percentage of the airStepDuration
    public void OnEnter(PlayerController pc)
    {
        pc.dashCount++;
        AudioManager.PlaySound(pc.transform.position, SoundType.Player_Dash);
        VFXManager.Instance.PlayFeedback(0, pc.transform);
        onEnterTime = Time.time + pc.airStepDuration;
        facingDirection = pc.movementDirection;
        if(facingDirection == Vector3.zero)
        {
            facingDirection = pc.transform.forward;
        }
        pc.anim.SetFloat("speed",0);
        pc.StartCoroutine(InputLockout(pc));
    }

    IEnumerator InputLockout(PlayerController pc)
    {
        yield return new WaitForSeconds(airStepInputLockoutTime * pc.airStepDuration);
        InputSystem.GetInputActionMapPlayer().Player.AirStep.performed += pc.CheckAirStepConditions;
    }

    public void OnExit(PlayerController pc)
    {
        InputSystem.GetInputActionMapPlayer().Player.AirStep.performed -= pc.CheckAirStepConditions;
        pc.Movement();
    }

    public void OnUpdate(PlayerController pc)
    {
        pc.AirStep(facingDirection,pc.airStepDuration - (onEnterTime - Time.time));
        pc.ItemUsage();
        if (Time.time > onEnterTime)
        {
            pc.SwitchPlayerState(PlayerState.Controlling);
            pc.dashCount = 0;
            pc.lastDashTime = Time.time;
        }
    }
}

*/