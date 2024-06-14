using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CrafterUI : MonoBehaviour
{
    public TextMeshProUGUI crafterName;
    public RectTransform recipeContainer;
    public GameObject recipiePrefab;

    [Header("Crafting Info")]
    public TextMeshProUGUI recipeName;
    public TextMeshProUGUI recipeAmountInStash;
    public Image craftingResultImage;
    public IngridientTupleUI[] ingridientTuple;
    public TextMeshProUGUI amountToCraftText;

    public Crafter_Interactable selectedCrafter;

    private int amountToCraft = 1;
    public void InitizalizeCrafterUI(Crafter_Interactable crafter)
    {
        amountToCraft = 1;
        selectedCrafter = crafter;
        crafterName.text = crafter.crafterName;
        foreach (Transform child in recipeContainer)
        {
            Destroy(child.gameObject);
        }
        int i = 0;
        foreach (CraftingRecipe recipe in crafter.recipes)
        {
            GameObject go = Instantiate(recipiePrefab, recipeContainer);
            go.GetComponent<RecipeButtonUI>().InitializeRecipe(recipe,i);
            i++;
        }
    }

    private void FixedUpdate()
    {
        if(selectedCrafter == null) return;
        SetupCraftingInfoUI(selectedCrafter.recipes[selectedCrafter.selectedRecipeIndex]);
    }

    public void IncrementAmount()
    {
        SetAmountToCraft(amountToCraft + 1);
    }

    public void DecrementAmount()
    {
        SetAmountToCraft(amountToCraft - 1);
    }

    public void SetAmountToCraft(int amount)
    {
        if (amount < 1) return;
        if (CheckIfAmountCanBeCrafted(selectedCrafter.recipes[selectedCrafter.selectedRecipeIndex],amount))
        {
            amountToCraft = amount;
            amountToCraftText.text = "Craft " + amountToCraft;
        }
    }

    public void SetupCraftingInfoUI(CraftingRecipe recipe)
    {
        recipeName.text = recipe.recipeName;
        recipeAmountInStash.text = "Owned: " + Stash.instance.GetTotalAmountOfItem(recipe.result);
        amountToCraftText.text = "Craft " + amountToCraft;
        craftingResultImage.sprite = recipe.result.itemIcon;
        if (recipe.materials.Length == 1)
        {
            ingridientTuple[0].SetIngridientInfo(recipe.materials[0], recipe.amount[0] * amountToCraft);
            ingridientTuple[1].SetIngridientInfoZero();
            return;
        }
        else
        {
            ingridientTuple[0].SetIngridientInfo(recipe.materials[0], recipe.amount[0] * amountToCraft);
            ingridientTuple[1].SetIngridientInfo(recipe.materials[1], recipe.amount[1] * amountToCraft);
        }
    }

    private bool CheckIfAmountCanBeCrafted(CraftingRecipe recipe,int amount)
    {
        for (int i = 0; i < recipe.materials.Length; i++)
        {
            ItemBlueprint item = recipe.materials[i];
            if (Stash.instance.GetItemsInStash().ContainsKey(item.id))
            {
                int craftingCost = recipe.amount[i] * amount;
                if (Stash.instance.GetItemsInStash()[item.id] < craftingCost)
                {
                    Debug.Log("Not enough items");
                    return false;
                }
            }
            else
            {
                Debug.Log("Not enough items");
                return false;
            }
        }
        return true;
    }

    public void Craft()
    {
        selectedCrafter.StartCrafting(amountToCraft);
        GameUI.instance.CloseAllWindows();
    }
}