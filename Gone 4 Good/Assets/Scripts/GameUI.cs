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

    [Header("Building Menu")]
    public GameObject buildingMenu;
    private bool isInBuildingMode = false;

    [Header("Crafting Menu")]
    public CrafterUI crafterUI;

    [Header("Hotbar")]
    public InventoryUI inventoryUI;

    [Header("Talent Menu")]
    public GameObject talentMenuEnviorment;
    public TalentDescriptionUI talentDescriptionUI;
    public TextMeshProUGUI skillPointsText;

    [Header("Level Up")]
    public GameObject levelUpCanvas;
    public GameObject levelUpMenu;

    [Header("ItemGetScreen")]
    public GameObject itemGetScene;
    public SpriteRenderer itemGetSprite;
    public TextMeshProUGUI itemGetTitle;
    public TextMeshProUGUI itemGetDescription;

    [Header("Player Stats")]
    public Image playerHealthBar;
    public Image playerManaBar;
    public TextMeshProUGUI playerHealthText;
    public TextMeshProUGUI playerManaText;
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
        InputSystem.GetInputActionMapPlayer().IngameUI.TalentMenu.performed -= ctx => ToggleTalentMenu();
    }

    /// <summary>
    /// Set up the input system for the inventory
    /// </summary>
    private void SetUpInputSystem()
    {
        InputSystem.GetInputActionMapPlayer().IngameUI.Inventory.performed += ToggleInventory;
        InputSystem.GetInputActionMapPlayer().IngameUI.BuildingMenu.performed += ToggleBuildingMenu;
        InputSystem.GetInputActionMapPlayer().IngameUI.Escape.performed += ctx => CloseAllWindows();  
        InputSystem.GetInputActionMapPlayer().IngameUI.TalentMenu.performed += ctx => ToggleTalentMenu();
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

    public void ToggleTalentMenu()
    {
        StartCoroutine(ToggleTalentMenuEnumerator());
    }

    private IEnumerator ToggleTalentMenuEnumerator()
    {
        if (!TransitionScreenControler.instance.isTransitioning)
        {
            if (interfaceAnimator.GetInteger("State") != 3)
            {
                Time.timeScale = 0.01f;
                float time = 0.5f;
                TransitionScreenControler.instance.CallTransition();
                yield return new WaitForSecondsRealtime(time);
                talentMenuEnviorment.SetActive(true);
                interfaceAnimator.SetInteger("State", 3);
                TransitionScreenControler.instance.DismissTransition();
            }
            else
            {
                float time = 0.5f;
                TransitionScreenControler.instance.CallTransition();
                yield return new WaitForSecondsRealtime(time);
                talentMenuEnviorment.SetActive(false);
                interfaceAnimator.SetInteger("State", 0);
                TransitionScreenControler.instance.DismissTransition();
                Time.timeScale = 1;
            }
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
        talentMenuEnviorment.SetActive(false);
    }

    public void ShowTalentDescription(Talent talent)
    {
        talentDescriptionUI.SetTalent(talent);
        talentDescriptionUI.gameObject.SetActive(true);
    }

    public void HideTalentDescription()
    {
        talentDescriptionUI.gameObject.SetActive(false);
    }

    #region LevelUp
    public void LevelUp()
    {
        StartCoroutine(LevelUpCoorutine());
    }

    private IEnumerator LevelUpCoorutine()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        //Time.timeScale = 0f;
        levelUpCanvas.SetActive(true);
        yield return new WaitForSecondsRealtime(2.2f);
        //SetInterfaceState(4);
        levelUpCanvas.SetActive(false);
    }

    public void EndLevelUp()
    {
        StartCoroutine(EndLevelUpCoorutine());
    }

    private IEnumerator EndLevelUpCoorutine()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        Time.timeScale = 1;
        levelUpMenu.SetActive(false);
        SetInterfaceState(0);
    }
    #endregion

    #region ItemGetScreen
    public void SetUpGetItemScreen(Item item)
    {

        itemGetScene.SetActive(true);
        itemGetSprite.sprite = ItemDatabase.GetItem(item.id).itemIcon;
        itemGetTitle.text = "Aquired " + ItemDatabase.GetItem(item.id).itemName +"!";
        itemGetDescription.text = ItemDatabase.GetItem(item.id).itemDescription;
        StartCoroutine(GetItemScreenCoorutine());
    }

    public void UpdatePlayerHealthBar()
    {
        int currentHealth = playerStatusManager.Hp;
        int maxHealth = playerStatusManager.maxHp;
        playerHealthBar.fillAmount = (float)currentHealth / maxHealth;
        playerHealthText.text = ""+currentHealth;
    }

    private IEnumerator GetItemScreenCoorutine()
    {
        Time.timeScale = 0.1f;
        yield return new WaitForSecondsRealtime(0.5f);
        SetInterfaceState(5);
        Time.timeScale = 0;
        InputSystem.GetInputActionMapPlayer().Player.UseSelectedItem.performed += SetGetItemScreenCloseDelegate;
    }

    private void SetGetItemScreenCloseDelegate(InputAction.CallbackContext ctx)
    {
        Time.timeScale = 1;
        SetInterfaceState(0);
        InputSystem.GetInputActionMapPlayer().Player.UseSelectedItem.performed -= SetGetItemScreenCloseDelegate;
    }
    #endregion

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
