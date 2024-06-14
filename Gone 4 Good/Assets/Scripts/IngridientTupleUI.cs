using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngridientTupleUI : MonoBehaviour
{
    public TextMeshProUGUI ingridientName;
    public TextMeshProUGUI ingridientAmount;
    public Image ingridientIcon;

    public void SetIngridientInfo(ItemBlueprint item, int amount)
    {

        ingridientName.text = item.itemName;
        ingridientAmount.text = $"{amount} / {Stash.instance.GetTotalAmountOfItem(item)}";
        ingridientIcon.enabled = true;
        ingridientIcon.sprite = item.itemIcon;
    }

    public void SetIngridientInfoZero()
    {
        ingridientName.text = " - ";
        ingridientAmount.text = $"- / -";
        ingridientIcon.enabled = false;
    }
}