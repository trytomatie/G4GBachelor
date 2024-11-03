using MoreMountains.Tools;
using UnityEngine;
using static Unity.VisualScripting.Member;

[CreateAssetMenu(fileName = "RifleWeapon_ItemEffects", menuName = "ScriptableObjects/ItemInteractionEffects/RifleWeapon_ItemEffects", order = 1)]
public class RifleWeapon_ItemEffects : GunInteractionEffects
{
    public Skill skill;
    public Skill skill2;

    private float timeLastFired = 0;
    private float perfectShotCounter = 0;
    private float currentSpread = 0;
    public override void OnUse(GameObject source, Item item)
    {
        if (isUsing)
        {
            if(timeLastFired + fireRate < Time.time)
            {
                if (SubstractAmmo(source, item) == false) return;
                source.GetComponent<FPSController>().TriggerAttack();
                FPSController pc = source.GetComponent<FPSController>();
                if(perfectShotCounter < perfectShots)
                {
                    currentSpread = 0;
                    perfectShotCounter++;
                }
                else
                {
                    currentSpread = Mathf.Clamp(currentSpread + spreadAccumulation, 0, spreadLimit);
                }
                timeLastFired = Time.time;
                NetworkSpellManager.Instance.FireRaycastBullet(NetworkGameManager.GetLocalPlayerId, source.transform.eulerAngles.y, Random.Range(40,60), 3);
            }
        }
    }

    public override void OnUseEnd(GameObject source,Item item)
    {

    }

    public override string EffectDescription(Item item)
    {
        string result = $"+{attackDamage} Attackdamage\n";
        result += "Shoot bullets in Rapidly";
        return result;
    }


    public override void OnEquip(GameObject source, Item item)
    {
        NetworkItemEffectsManager.Instance.EquipItemServerRpc(NetworkGameManager.GetLocalPlayerId, weaponPrefab.name,1);
        source.GetComponent<StatusManager>().weaponAttackDamage = attackDamage;
        if(skill != null)
        {
            skill.AssignSkill(source, 0);
        }
        timeLastFired = 0;
        currentSpread = 0;

    }

    public override void OnUnequip(GameObject source, Item item)
    {
        NetworkItemEffectsManager.Instance.UnequipItemServerRpc(NetworkGameManager.GetLocalPlayerId);
        source.GetComponent<StatusManager>().weaponAttackDamage = 0;
        if (skill != null)
        {
            skill.RemoveSkill(source, 0);
        }
    }

    public override void OnDrop(GameObject source, Item item)
    {
        NetworkItemEffectsManager.Instance.UnequipItemServerRpc(NetworkGameManager.GetLocalPlayerId);
        source.GetComponent<StatusManager>().weaponAttackDamage = 0;
        Debug.Log("Dropping " + item.id);
    }
}