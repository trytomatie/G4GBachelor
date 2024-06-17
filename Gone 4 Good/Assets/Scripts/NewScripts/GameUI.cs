using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("Inventory")]
    public Animator inventoryAnimator;



    [Header("Hotbar")]
    public InventoryUI inventoryUI;






    [Header("Player Stats")]
    public Image playerHealthBar;
    public TextMeshProUGUI playerHealthText;
    private StatusManager playerStatusManager;

    [Header("Item Description")]
    public GameObject itemDescription;
    public RectTransform[] itemDescriptionLocations;
    public TextMeshProUGUI itemDescriptionText;
    public TextMeshProUGUI itemDescriptionTitle;
    public TextMeshProUGUI itemType;
    public Image itemDescriptionIcon;

    [Header("Skills")]
    public SkillSlotUI[] skillslots;

    [HideInInspector] public Animator interfaceAnimator;

    // Singleton
    public static GameUI instance;

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
        playerStatusManager = GameManager.Instance.player.GetComponent<StatusManager>();
        playerStatusManager.OnDamage.AddListener(UpdatePlayerHealthBar);
        UpdatePlayerHealthBar();
        for(int i = 0; i < skillslots.Length; i++)
        {
            skillslots[i].index = i;
        }
    }

    private void OnDisable()
    {
        InputSystem.GetInputActionMapPlayer().IngameUI.Inventory.performed -= ToggleInventory;
        InputSystem.GetInputActionMapPlayer().IngameUI.BuildingMenu.performed -= ToggleBuildingMenu;
        InputSystem.GetInputActionMapPlayer().IngameUI.Escape.performed -= ctx => CloseAllWindows();
    }

    /// <summary>
    /// Set up the input system for the inventory
    /// </summary>
    private void SetUpInputSystem()
    {
        InputSystem.GetInputActionMapPlayer().IngameUI.Inventory.performed += ToggleInventory;
        InputSystem.GetInputActionMapPlayer().IngameUI.BuildingMenu.performed += ToggleBuildingMenu;
        InputSystem.GetInputActionMapPlayer().IngameUI.Escape.performed += ctx => CloseAllWindows();  
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


    public void UpdatePlayerHealthBar()
    {
        int currentHealth = playerStatusManager.Hp.Value;
        int maxHealth = playerStatusManager.maxHp;
        playerHealthBar.fillAmount = (float)currentHealth / maxHealth;
        playerHealthText.text = ""+currentHealth;
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
    #endregion
}
