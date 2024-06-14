using System.Collections.Generic;
using UnityEngine;
using static Container;

public class Container : MonoBehaviour
{
    public List<Item> items = new List<Item>();
    public int space = 20;
    public delegate void ContainerUpdated(int index);
    public delegate void ItemCompletlyRemoved(int index);
    public ItemCompletlyRemoved onItemCompletlyRemoved;
    public ContainerUpdated onInventoryUpdate;
    public Stash stash;

    private void Start()
    {
        items.Clear();
        for(int i = 0; i < space; i++)
        {
            items.Add(new Item(0, 0));
        }
        Init();
    }

    public virtual void Init()
    {

    }

    public bool AddItem(Item item)
    {
        int pos = ContainsItemAndHasSpace(item);
        if (pos != -1)
        {
            int maxStack = items[pos].maxStackSize;
            if (items[pos].amount + item.amount <= maxStack) // If the item can fit in the stack
            {
                items[pos].amount += item.amount;
                stash.AddItem(item, item.amount, this);
                onInventoryUpdate?.Invoke(pos);
                return true;
            }
            else // If the item can't fit in the stack
            {
                item.amount -= maxStack - items[pos].amount;
                items[pos].amount = maxStack;
                if (items.Count < space)
                {
                    print(item.amount);
                    AddItem(item);
                    onInventoryUpdate?.Invoke(pos);
                    return true;
                }
                else
                {
                    // TODO: Drop item on the ground
                }
            }
        }
        else if (ItemsCount < space)
        {

            AddItemToEmptySpot(item);
            stash.AddItem(item, item.amount, this);
            onInventoryUpdate?.Invoke(pos);
            return true;
        }
        return false;
    }

    private int ItemsCount
    {
        get
        {
            int count = 0;
            foreach(Item item in items)
            {
                if (item.id != 0) count++;
            }
            return count;
        }
    }

    private void AddItemToEmptySpot(Item itemToAdd)
    {
        int index = -1;
        int i = 0;
        foreach (Item item in items)
        {
            if (item.id == 0)
            {
                index = i;
                break;
            }
            i++;
        }
        if(index != -1)
        {
            items[index] = itemToAdd;
        }
        else
        {
            // Do Nothing i Guess
        }
    }


    public void RemoveItem(int id, int amount)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].id == id)
            {
                if (items[i].amount > amount) // If the item has more than the amount we want to remove
                {
                    items[i].amount -= amount;
                    stash.RemoveItem(id, amount, this);
                    onInventoryUpdate.Invoke(i);
                    return;
                }
                else if (items[i].amount == amount) // If the item has the same amount as the amount we want to remove
                {
                    print("Remove Same");
                    onItemCompletlyRemoved?.Invoke(i);
                    items[i] = new Item(0, 0);
                    stash.RemoveItem(id, amount, this);
                    onInventoryUpdate.Invoke(i);
                    return;
                }
                else // If the item has less than the amount we want to remove
                {
                    onItemCompletlyRemoved?.Invoke(i);
                    amount -= items[i].amount;
                    stash.RemoveItem(id, items[i].amount, this);
                    items[i] = new Item(0, 0);
                }
            }
        }
    }

    public int ContainsItemAndHasSpace(Item item)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].id == item.id)
            {
                if (items[i].maxStackSize != items[i].amount)
                {
                    return i;
                }
            }
        }
        return -1;
    }

    public static void SwapItems(Container container1, Container container2, int index1, int index2)
    {
        Item temp = container1.items[index1];
        container1.onItemCompletlyRemoved?.Invoke(index1);
        container2.onItemCompletlyRemoved?.Invoke(index2);
        container1.items[index1] = container2.items[index2];
        container2.items[index2] = temp;
        container1.onInventoryUpdate?.Invoke(index1);
        container2.onInventoryUpdate?.Invoke(index2);
    }
}
