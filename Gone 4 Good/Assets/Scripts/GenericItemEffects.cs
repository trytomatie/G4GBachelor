using UnityEngine;

[CreateAssetMenu(fileName = "GenericItemEffects", menuName = "ScriptableObjects/ItemInteractionEffects/GenericItemEffects", order = 1)]
public class GenericItemEffects : ItemInteractionEffects
{
    public override void OnUse(GameObject source, Item item)
    {
        Debug.Log("Using " + item.id);
    }

    public override void OnEquip(GameObject source, Item item)
    {
        //Transform weaponPivot = source.GetComponent<PlayerController>().weaponPivot.transform;
        Debug.Log("Equipping " + item.id);
    }

    public override void OnUnequip(GameObject source, Item item)
    {
        Debug.Log("Unequipping " + item.id);
    }

    public override void OnDrop(GameObject source, Item item)
    {
        Debug.Log("Dropping " + item.id);
    }
}