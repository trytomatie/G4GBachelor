using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Stash : MonoBehaviour
{
    public Dictionary<int, int> inventoryItems = new Dictionary<int, int>();
    public Dictionary<int, int> totalItems = new Dictionary<int, int>();
    public Inventory inventory;



    // Singleton
    public static Stash instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one instance of Stash found!");
            return;
        }
        instance = this;

    }
    private void Start()
    {
        InputSystem.GetInputActionMapPlayer().Debug.PrintStackToConsole.performed += ctx => PrintStack();
    }

    private void PrintStack()
    {
        foreach (KeyValuePair<int, int> item in totalItems)
        {
            Debug.Log("Key: " + item.Key + " Value: " + item.Value);
        }
    }

    private void OnDisable()
    {
        InputSystem.GetInputActionMapPlayer().Debug.PrintStackToConsole.performed -= ctx => PrintStack();
    }
    public bool AddItem(Item itemRef, int amount, Container source)
    {
        bool fromInventory = false;
        if(source == inventory)
        {
            fromInventory = true;
        }
        ItemBlueprint item = ItemDatabase.GetItem(itemRef.id);
        if (totalItems.ContainsKey(itemRef.id))
        {
            totalItems[itemRef.id] += amount;
            if (fromInventory)
            {
                if(inventoryItems.ContainsKey(itemRef.id))
                {
                    inventoryItems[itemRef.id] += amount;
                }
                else
                {
                    inventoryItems.Add(itemRef.id, amount);
                }

            }
        }
        else
        {
            totalItems.Add(itemRef.id, amount);
            if (fromInventory)
            {
                inventoryItems.Add(itemRef.id, amount);
            }
        }
        return true;
    }

    public bool RemoveItem(int id, int amount,Container source)
    {
        bool fromInventory = false;
        bool hasremoved = false;
        if(source == inventory)
        {
            fromInventory = true;
        }
        if(fromInventory)
        {
            if (inventoryItems.ContainsKey(id))
            {
                    inventoryItems[id] -= amount;
                    if (inventoryItems[id] == 0)
                    {
                        inventoryItems[id] = 0;
                    }
                    hasremoved= true;
            }
        }
        if (totalItems.ContainsKey(id))
        {
                totalItems[id] -= amount;
                if (totalItems[id] == 0)
                {
                    totalItems[id] = 0;
                }
                hasremoved= true;
        }
        return hasremoved;
    }

    public int GetTotalAmountOfItem(ItemBlueprint result)
    {
        return GetTotalAmountOfItem(result.id);
    }

    public int GetTotalAmountOfItem(int id)
    {
        if(GameManager.Instance.globalStashUnlocked)
        {
            if (inventoryItems.ContainsKey(id))
            {
                return inventoryItems[id];
            }
        }
        else
        {
            if (totalItems.ContainsKey(id))
            {
                return totalItems[id];
            }
        }
        return 0;
    }

    public bool HasEnoughItems(Item item1)
    {
        if (item1 == null)
        {
            return true;
        }
        return HasEnoughItems(item1.id, item1.amount);
    }

    public bool HasEnoughItems(int id,int amount)
    {
        if (totalItems.ContainsKey(id))
        {
            if (totalItems[id] >= amount)
            {
                return true;
            }
        }
        return false;
    }

    public Dictionary<int,int> GetItemsInStash()
    {
        if(GameManager.Instance.globalStashUnlocked)
        {
            return inventoryItems;
        }
        else
        {
            return totalItems;
        }
    }
}
