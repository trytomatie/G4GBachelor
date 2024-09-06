using UnityEditor;
using UnityEngine;


public class Slowness : StatusEffect
{
    public float slowAmount = 0.5f;

    public override void OnApply(StatusManager sm)
    {
        sm.movementSpeedMultiplier -= slowAmount;
    }

    public override void OnRemove(StatusManager sm)
    {
        sm.movementSpeedMultiplier += slowAmount;
    }
}
