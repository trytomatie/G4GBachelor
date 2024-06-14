using UnityEngine;
using static Unity.VisualScripting.Member;

[CreateAssetMenu(fileName = "Boots_ItemEffect", menuName = "ScriptableObjects/ItemInteractionEffects/Boots_ItemEffect", order = 1)]
public class Boots_ItemEffect : ItemInteractionEffects
{
    public Skill airDash;

    public override void OnEquip(GameObject source, Item item)
    {
        airDash.AssignSkill(source,2);
    }

    public override void OnUnequip(GameObject source, Item item)
    {
        airDash.RemoveSkill(source,2);
    }

    public override void OnDrop(GameObject source, Item item)
    {
    }
}