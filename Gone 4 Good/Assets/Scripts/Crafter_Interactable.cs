using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Crafter_Interactable : Interactable
{
    public string crafterName;
    public List<CraftingRecipe> recipes;
    public int selectedRecipeIndex;
    private int craftingRecipieIndex;

    public Item[] craftingMaterials = new Item[2];



    private int amount = 0;

    [Header("Interface")]
    public Image craftingImage;
    public TextMeshProUGUI amountToCraft;
    public MMProgressBar progressBar;

    private void Start()
    {
        ClearInterface();
    }

    public override void Interact()
    {
        GameUI.instance.crafterUI.InitizalizeCrafterUI(this);
        GameUI.instance.SetInterfaceState(2);
    }

    public void StartCrafting(int amount)
    {
        craftingRecipieIndex = selectedRecipeIndex;
        CancelCraft();
        for (int i = 0; i < recipes[craftingRecipieIndex].materials.Length; i++)
        {
            ItemBlueprint item = recipes[craftingRecipieIndex].materials[i];
            if(Stash.instance.inventoryItems.ContainsKey(item.id))
            {
                int craftingCost = recipes[craftingRecipieIndex].amount[i] * amount;
                if (Stash.instance.inventoryItems[item.id] < craftingCost)
                {
                    Debug.Log("Not enough items");
                    return;
                }
            }
            else
            {
                Debug.Log("Not enough items");
                return;
            }
        }
        this.amount = amount;
        craftingMaterials = new Item[2];
        for (int i = 0; i < recipes[craftingRecipieIndex].materials.Length; i++)
        {
            craftingMaterials[i] = new Item(recipes[craftingRecipieIndex].materials[i].id, recipes[craftingRecipieIndex].amount[i] * amount);
            Stash.instance.inventory.RemoveItem(craftingMaterials[i].id, craftingMaterials[i].amount);
        }
        progressBar.gameObject.SetActive(true);
        Invoke("PerformCrafting", 0);
    }

    public void PerformCrafting()
    {
        craftingImage.sprite = recipes[craftingRecipieIndex].result.itemIcon;
        amountToCraft.text = "x" + amount.ToString();


        progressBar.UpdateBar01(0);
        progressBar.LerpForegroundBarDurationIncreasing = recipes[craftingRecipieIndex].craftingTime;
        progressBar.BarFillMode = MMProgressBar.BarFillModes.FixedDuration;
        progressBar.UpdateBar01(1);
        Invoke("CompleteCraft", recipes[craftingRecipieIndex].craftingTime);

    }

    private void CompleteCraft()
    {
        // Spawn the item
        GameManager.Instance.SpawnItem(recipes[craftingRecipieIndex].result,1 * Mathf.RoundToInt(GameManager.Instance.generalItemProductionMultiplier), transform.position);
        for(int i = 0; i < recipes[craftingRecipieIndex].amount.Length; i++)
        {
            craftingMaterials[i].amount -= recipes[craftingRecipieIndex].amount[i];
        }
        amount--;
        if(amount > 0)
        {
            Invoke("PerformCrafting", 0);
        }
        else
        {
            ClearInterface();
            Debug.Log("Crafting complete");
        }
    }

    private void ClearInterface()
    {
        progressBar.gameObject.SetActive(false);
    }

    private void CancelCraft()
    {
        for (int i = 0; i < craftingMaterials.Length; i++)
        {
            if (craftingMaterials[i] != null && craftingMaterials[i].amount > 0)
            {
                GameManager.Instance.SpawnItem(ItemDatabase.GetItem(craftingMaterials[i].id), craftingMaterials[i].amount, transform.position);
            }
        }
        CancelInvoke();
        ClearInterface();
    }
}
