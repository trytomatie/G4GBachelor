using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Crafting Recipe", menuName = "Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    public string recipeName;
    public ItemBlueprint[] materials;
    public int[] amount;
    public ItemBlueprint result;
    public float craftingTime = 10;

}
