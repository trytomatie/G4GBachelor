using UnityEngine;

public class TalentFunctionality : ScriptableObject
{
    public virtual void ApplyTalentEffect(StatusManager sm)
    {
        Debug.Log("Talent effect applied");
    }

    public virtual void RemoveTalentEffect(StatusManager sm)
    {
        Debug.Log("Talent effect removed");
    }
}

