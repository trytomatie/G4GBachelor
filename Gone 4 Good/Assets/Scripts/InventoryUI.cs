using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public List<ItemSlotUI> hotbarSlots;
    public List<ItemSlotUI> inventorySlots;
    public List<ItemSlotUI> equipmentSlots;
    public Inventory syncedInventory;
    public Container equipmentContainer;
    public RectTransform selectedInventorySlot;
    public Image dragImage;

    // Singelton
    public static InventoryUI Instance { get; private set; }

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        //equipmentContainer.onInventoryUpdate += UpdateUI;
        UpdateUI(0);
        AssignIndciesToInventorySlots();
    }

    private void AssignIndciesToInventorySlots()
    {
        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            hotbarSlots[i].assignedIndex = i;
            hotbarSlots[i].syncedContainer = syncedInventory;
        }
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            inventorySlots[i].assignedIndex = i;
            inventorySlots[i].syncedContainer = syncedInventory;
        }
        for(int i = 0; i < equipmentSlots.Count; i++)
        {
            equipmentSlots[i].assignedIndex = i;
            equipmentSlots[i].syncedContainer = equipmentContainer;
        }
    }

    public void UpdateUI(int updateIndex)
    {
        print("Update Inventory");
        for (int i = 0; i < syncedInventory.items.Count; i++)
        {
            if (i < hotbarSlots.Count)
            {   
                hotbarSlots[i].SetItem(syncedInventory.items[i]);
            }
            if(i < inventorySlots.Count)
            {
                inventorySlots[i].SetItem(syncedInventory.items[i]);
            }
        }
    }

    public void SelectSlot(int index)
    {
        selectedInventorySlot.position = hotbarSlots[index].transform.position;
        // adjust size to hotbarslot
        selectedInventorySlot.sizeDelta = hotbarSlots[index].GetComponent<RectTransform>().sizeDelta;
    }
}
