using System;
using System.Reflection;
using UnityEngine;

public class ItemInteractionEffects : ScriptableObject
{
    protected bool isUsing = false;
    public virtual void OnUse(GameObject source, Item item)
    {
        Debug.LogError("OnUse not implemented");
    }

    public virtual string EffectDescription(Item item)
    {
        return "";
    }

    public virtual void OnUseEnd(GameObject source, Item item)
    {
        Debug.LogError("OnUseEnd not implemented");
    }

    public void OnUsePerformed(GameObject source, Item item)
    {
        isUsing = true;
    }

    public void OnUseCancelled(GameObject source, Item item)
    {
        isUsing = false;
        OnUseEnd(source,item);
    }

    public virtual void OnEquip(GameObject source, Item item)
    {
        Debug.LogError("OnEquip not implemented");
    }

    public virtual void OnUnequip(GameObject source, Item item)
    {
        Debug.LogError("OnUnequip not implemented");
    }

    public virtual void OnDrop(GameObject source, Item item)
    {
        Debug.LogError("OnDrop not implemented");
    }
}

public enum ItemType
{
    None,
    Head,
    Chest,
    Legs,
    Feet,
    Weapon,
    Shield,
    Tool,
    Consumable,
    KeyItem,
    Material
}
