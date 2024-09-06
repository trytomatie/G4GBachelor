using UnityEditor;
using UnityEngine;


public class StatusEffect 
{
    public string name;
    public float duration;

    public void ApplyStatusEffect(StatusManager sm)
    {
        sm.statusEffects.Add(this);
        OnApply(sm);
    }
    public virtual void OnApply(StatusManager sm)
    {

    }

    public virtual void OnUpdate(StatusManager sm)
    {

    }

    public virtual void OnRemove(StatusManager sm)
    {

    }
}
