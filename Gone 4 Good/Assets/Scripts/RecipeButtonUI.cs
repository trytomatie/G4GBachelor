using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RecipeButtonUI : MonoBehaviour
{
    public TextMeshProUGUI recipeName;
    public CraftingRecipe recipe;
    public Image recipieSprite;
    private int recipeIndex;
    public void InitializeRecipe(CraftingRecipe craftingRecipe,int index)
    {
        recipeIndex = index;
        recipe = craftingRecipe;
        recipeName.text = recipe.recipeName;
        recipieSprite.sprite = recipe.result.itemIcon;
        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(SetRecipe);

    }

    public void SetRecipe()
    {
        print("Setting recipe: " + recipeName.text);
        GameUI.instance.crafterUI.selectedCrafter.selectedRecipeIndex = recipeIndex;
        GameUI.instance.crafterUI.SetupCraftingInfoUI(recipe);
    }
}