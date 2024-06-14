using System.Collections.Generic;
using UnityEngine;
using static Container;

public class EquipmentContainer : Container
{
    public override void Init()
    {
        onInventoryUpdate += OnItemEquip;
        onItemCompletlyRemoved += OnItemUnequip;
    }
    public void OnItemEquip(int index)
    {
        foreach(Item item in items)
        {
            if (item != null)
            {
                item.GetItemInteractionEffects.OnEquip(GameManager.Instance.player, item);
            }
        }
    }

    public void OnItemUnequip(int index)
    {
        items[index].GetItemInteractionEffects.OnUnequip(GameManager.Instance.player, items[index]);
    }
}
