using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public ItemType slotRestriction = ItemType.None;
    public Image sprite;
    public TextMeshProUGUI amountText;
    public MMF_Player feedback;
    public Container syncedContainer;
    public int currentAmount;
    public int assignedIndex = 0;
    public int descriptionLocationIndex = 0;

    private void Awake()
    {
        ClearSlot();
    }
    public void ClearSlot()
    {
        sprite.enabled = false;
        sprite.sprite = null;
        amountText.text = "";
        currentAmount = 0;
    }
    public void SetItem(Item item)
    {
        if (item == null || item.id == 0)
        {
            ClearSlot();
            return;
        }

        sprite.enabled = true;
        sprite.sprite = ItemDatabase.GetItem(item.id).itemIcon;
        if (item.amount != currentAmount)
        {
            feedback.PlayFeedbacks();
            currentAmount = item.amount;
        }
        if(item.amount == 1)
        {
            amountText.text = "";
        }
        else
        {
            amountText.text = $"{item.amount}";
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Vector2 mousePos = eventData.position;
        InventoryUI.Instance.dragImage.sprite = sprite.sprite;
        InventoryUI.Instance.dragImage.enabled = true;
        InventoryUI.Instance.dragImage.transform.position = mousePos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 mousePos = eventData.position;
        InventoryUI.Instance.dragImage.transform.position = mousePos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ItemSlotUI dragedOverItemSlot = eventData.pointerCurrentRaycast.gameObject.GetComponent<ItemSlotUI>();
        if(dragedOverItemSlot != null)
        {
            ItemType itemType = ItemDatabase.GetItem(syncedContainer.items[assignedIndex].id).itemType;
            ItemType dragedOverItemType = ItemDatabase.GetItem(dragedOverItemSlot.syncedContainer.items[dragedOverItemSlot.assignedIndex].id).itemType;
            if (dragedOverItemSlot.slotRestriction != ItemType.None && dragedOverItemSlot.slotRestriction != itemType)
            {
                if(itemType != ItemType.None)
                {
                    return;
                }
            }
            if(slotRestriction != ItemType.None && slotRestriction != dragedOverItemType)
            {
                if (dragedOverItemType != ItemType.None)
                {
                    return;
                }
            }
            Container.SwapItems(dragedOverItemSlot.syncedContainer, syncedContainer, dragedOverItemSlot.assignedIndex, assignedIndex);
        }
        InventoryUI.Instance.dragImage.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (syncedContainer.items[assignedIndex].id != 0)
        {
            GameUI.instance.ShowItemDescription(syncedContainer.items[assignedIndex], descriptionLocationIndex);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (syncedContainer.items[assignedIndex].id != 0)
        {
            GameUI.instance.HideItemDescription();
        }
    }
}
