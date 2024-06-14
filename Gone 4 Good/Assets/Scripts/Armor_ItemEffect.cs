using UnityEngine;
using static Unity.VisualScripting.Member;

[CreateAssetMenu(fileName = "Armor_ItemEffect", menuName = "ScriptableObjects/ItemInteractionEffects/Armor_ItemEffect", order = 1)]
public class Armor_ItemEffect : ItemInteractionEffects
{
    public int defenseRating = 1;

    public override void OnEquip(GameObject source, Item item)
    {
        source.GetComponent<StatusManager>().bonusDefense += defenseRating;
    }

    public override void OnUnequip(GameObject source, Item item)
    {
        source.GetComponent<StatusManager>().bonusDefense -= defenseRating;
    }

    public override void OnDrop(GameObject source, Item item)
    {
        Debug.Log("Dropping " + item.id);
    }
}