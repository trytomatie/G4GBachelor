using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("Inventory")]
    public Animator inventoryAnimator;



    [Header("Hotbar")]
    public InventoryUI inventoryUI;

    [Header("Interaction")]
    public Transform interactionToolTip;




    [Header("Statusbars")]
    public HealthBar playerHealthBar;
    public StaminaBarUI playerStaminaBar;
    public GameObject healthBarPrefab;
    public Transform allyHealthbarPanel;
    public HealthBar[] allyHealthBars;

    [Header("Item Description")]
    public GameObject itemDescription;
    public RectTransform[] itemDescriptionLocations;
    public TextMeshProUGUI itemDescriptionText;
    public TextMeshProUGUI itemDescriptionTitle;
    public TextMeshProUGUI itemType;
    public Image itemDescriptionIcon;

    [Header("Skills")]
    public SkillSlotUI[] skillslots;

    [Header("Ammo")]
    public TextMeshProUGUI ammoText;

    [Header("Remnant Revival Bar")]
    public RemnantRevivalBarUI remnantRevivalBarUI;

    [Header("Damage Indicators")]
    public Animator damageIndicatorFront;
    public Animator damageIndicatorLeft;
    public Animator damageIndicatorRight;
    public Animator damageIndicatorBack;

    [Header("Pause Menu")]
    public GameObject pauseMenu;
    public GameObject generalUI;

    [Header("Relay Code")]
    public TextMeshProUGUI relayCodeText;
    public Animation relayCodeCopiedText;

    [HideInInspector] public Animator interfaceAnimator;

    [Header("Options")]
    public AudioMixerGroup musicAudioGroup;
    public AudioMixerGroup sfxAudioGroup;

    // Singleton
    public static GameUI instance;

    public bool forceMouseVisible = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    private void Start()
    {
        interfaceAnimator = GetComponent<Animator>();
        SetUpInputSystem();
        for(int i = 0; i < skillslots.Length; i++)
        {
            skillslots[i].index = i;
        }
        generalUI.SetActive(true);
        pauseMenu.SetActive(false);
        Options.Initilize(musicAudioGroup, sfxAudioGroup);
        Options.LoadOptions();
        // Map pause menu
        InputSystem.GetInputActionMapPlayer().IngameUI.Escape.performed += ctx => TogglePauseMenu(!pauseMenu.activeSelf);
        SetRelayCode();
    }

    public void SetRelayCode()
    {
        relayCodeText.text = "Lobby Code: " + ((G4GNetworkManager)NetworkManager.Singleton).relayCode;
    }

    public void CopyRelayCodeToClipboard()
    {
        GUIUtility.systemCopyBuffer = ((G4GNetworkManager)NetworkManager.Singleton).relayCode;
        relayCodeCopiedText.Play();
    }

    

    public void TogglePauseMenu(bool state)
    {
        pauseMenu.SetActive(state);
        generalUI.SetActive(!state);
        if(state)
        {
            InputSystem.GetInputActionMapPlayer().Camera.Disable();
            InputSystem.GetInputActionMapPlayer().Player.Disable();
        }
        else
        {
            InputSystem.GetInputActionMapPlayer().Camera.Enable();
            InputSystem.GetInputActionMapPlayer().Player.Enable();
        }
    }

    private void Update()
    {
        if(pauseMenu.activeSelf || forceMouseVisible)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }


    public void TriggerDamageIndicator(Direction direction)
    {
        switch (direction)
        {
            case Direction.Front:
                damageIndicatorFront.SetTrigger("Damage");
                break;
            case Direction.Left:
                damageIndicatorLeft.SetTrigger("Damage");
                break;
            case Direction.Right:
                damageIndicatorRight.SetTrigger("Damage");
                break;
            case Direction.Back:
                damageIndicatorBack.SetTrigger("Damage");
                break;
        }
    }

    public void SetAmmo(int currentClip,int currentAmmo)
    {
        ammoText.text = currentClip + " / " + currentAmmo;
    }
    private void OnDisable()
    {
        InputSystem.GetInputActionMapPlayer().IngameUI.Inventory.performed -= ToggleInventory;
        InputSystem.GetInputActionMapPlayer().IngameUI.BuildingMenu.performed -= ToggleBuildingMenu;
        InputSystem.GetInputActionMapPlayer().IngameUI.Escape.performed -= ctx => TogglePauseMenu(false);
    }

    /// <summary>
    /// Set up the input system for the inventory
    /// </summary>
    private void SetUpInputSystem()
    {
        InputSystem.GetInputActionMapPlayer().IngameUI.Inventory.performed += ToggleInventory;
        InputSystem.GetInputActionMapPlayer().IngameUI.BuildingMenu.performed += ToggleBuildingMenu;
    }


    private void ToggleBuildingMenu(InputAction.CallbackContext ctx)
    {
        ToggleBuildingMenu();
    }

    public void ToggleBuildingMenu()
    {
        if (interfaceAnimator.GetInteger("State") != 1)
        {
            interfaceAnimator.SetInteger("State", 1);
        }
        else
        {
            interfaceAnimator.SetInteger("State", 0);
            BuildingManager.instance.PlaceBuildingMode = false;
        }
    }

    public void SetInterfaceState(int state)
    {
        interfaceAnimator.SetInteger("State", state);
    }

    /// <summary>
    /// Toggle the inventory on and off
    /// </summary>
    /// <param name="ctx"></param>
    private void ToggleInventory(InputAction.CallbackContext ctx)
    {
        if (interfaceAnimator.GetInteger("State") != 6)
        {
            SetInterfaceState(6);
        }
        else
        {
            SetInterfaceState(0);
        }


        //bool isInventoryOpen = inventoryAnimator.GetBool("Opened");
        //inventoryAnimator.SetBool("Opened", !isInventoryOpen);
    }

    public void CloseAllWindows()
    {
        if (interfaceAnimator.GetInteger("State") == 4) return; // Cant close windows while leveling up
        interfaceAnimator.SetInteger("State", 0);
        inventoryAnimator.SetBool("Opened", false);
        BuildingManager.instance.PlaceBuildingMode = false;
    }


    #region ItemDescription
    public void ShowItemDescription(Item item, int location)
    {
        ItemBlueprint itemBlueprint = item.BluePrint;
        itemDescriptionText.text = item.GetItemInteractionEffects.EffectDescription(item)+ "\n\n<color=grey>" + itemBlueprint.itemDescription;
        itemDescriptionTitle.text = itemBlueprint.itemName;
        itemType.text = itemBlueprint.itemType.ToString();
        itemDescriptionIcon.sprite = itemBlueprint.itemIcon;
        itemDescription.transform.position = itemDescriptionLocations[location].position;
        itemDescription.SetActive(true);
    }

    public void HideItemDescription()
    {
        itemDescription.SetActive(false);
    }

    public void SyncHpBar(StatusManager statusManager)
    {
        statusManager.Hp.OnValueChanged += (oldValue, newValue) =>
        {
            playerHealthBar.SetHealth(newValue, statusManager.maxHp);
        };
        playerHealthBar.playerName.text = statusManager.GetComponent<FPSController>().playerName.Value.ToString();
    }

    public void SyncHpAllyBar(StatusManager statusManager)
    {
        GameObject healthBar = Instantiate(healthBarPrefab, allyHealthbarPanel);
        HealthBar healthBarScript = healthBar.GetComponent<HealthBar>();
        healthBarScript.SetHealth(statusManager.Hp.Value, statusManager.maxHp);
        healthBarScript.playerName.text = statusManager.GetComponent<FPSController>().playerName.Value.ToString();
        statusManager.GetComponent<FPSController>().playerName.OnValueChanged += (oldValue, newValue) =>
        {
            healthBarScript.playerName.text = newValue.ToString();
        };
        statusManager.Hp.OnValueChanged += (oldValue, newValue) =>
        {
            healthBarScript.SetHealth(newValue, statusManager.maxHp);
        };
        statusManager.NetworkDespawnEvent.AddListener(() =>
        {
            Destroy(healthBar);
        });
    }
    #endregion
}

public enum Direction { Front, Left, Right, Back }
