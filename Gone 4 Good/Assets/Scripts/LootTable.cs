using System;
using UnityEngine;

[CreateAssetMenu(fileName = "LootTable", menuName = "ScriptableObjects/LootTable", order = 1)]
public class LootTable : ScriptableObject
{
    public ItemBlueprint[] lootTable;
    public int[] lootAmount;

    public void DropLoot(Vector3 position)
    {
        for(int i = 0; i < GameManager.Instance.lootDropAmount;i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, lootTable.Length);
            ItemBlueprint item = lootTable[randomIndex];
            int amount = lootAmount[randomIndex];
            GameManager.Instance.SpawnItem(item,amount,position);
        }
    }
}