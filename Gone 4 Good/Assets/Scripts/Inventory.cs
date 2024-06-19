using System;
using System.Collections;

public class Inventory : Container
{
    public override void Init()
    {
        onItemCompletlyRemoved += RemoveHotbarItemIfRemoved;
        onInventoryUpdate += EquipNewHotBarItem;
    }
    public new void Start()
    {
        base.Start();
        AddItem(new Item(19, 1));
    }
    public int currentHotbarIndex = 0;
    public Item CurrentHotbarItem
    {
        get
        {
            if (items.Count - 1 < currentHotbarIndex) return null;
            return items[currentHotbarIndex];
        }
    }
    public void SwitchHotbarItem(int index)
    {
        // GameUI.instance.inventoryUI.SelectSlot(index); TODO: FIX THIS
        if (items.Count-1 >= currentHotbarIndex)
        {
            items[currentHotbarIndex]?.GetItemInteractionEffects.OnUnequip(gameObject, items[currentHotbarIndex]);
        }
        currentHotbarIndex = index;
        if(items.Count-1 >= currentHotbarIndex)
        {
            items[currentHotbarIndex]?.GetItemInteractionEffects.OnEquip(gameObject, items[currentHotbarIndex]);
        }
    }

    public void RemoveHotbarItemIfRemoved(int i)
    {
        print(i + " " + currentHotbarIndex);
        if(i == currentHotbarIndex)
        {
            items[currentHotbarIndex].GetItemInteractionEffects.OnUnequip(gameObject, items[currentHotbarIndex]);
        }
    }

    public void EquipNewHotBarItem(int i)
    {
        if(i == currentHotbarIndex)
        {
            items[currentHotbarIndex].GetItemInteractionEffects.OnEquip(gameObject, items[currentHotbarIndex]);
        }
    }
}
