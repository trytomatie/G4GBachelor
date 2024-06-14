using System.Collections;
using UnityEngine;


[CreateAssetMenu(fileName = "AttackIncrease_TalentFunctionality", menuName = "Talents/AttackIncrease")]
public class AttackIncrease_TalentFunctionality : TalentFunctionality
{
    public int attackIncrease = 1;
    public float attackIncreaseMultiplier = 0.1f;

    public override void ApplyTalentEffect(StatusManager sm)
    {
        sm.bonusAttackDamage += attackIncrease;
        sm.bonusAttackDamageMultiplier += attackIncreaseMultiplier;
    }

    public override void RemoveTalentEffect(StatusManager sm)
    {
        sm.bonusAttackDamage -= attackIncrease;
        sm.bonusAttackDamageMultiplier -= attackIncreaseMultiplier;
    }
}
