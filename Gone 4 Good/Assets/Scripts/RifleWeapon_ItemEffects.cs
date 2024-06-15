using MoreMountains.Tools;
using UnityEngine;
using static Unity.VisualScripting.Member;

[CreateAssetMenu(fileName = "RifleWeapon_ItemEffects", menuName = "ScriptableObjects/ItemInteractionEffects/RifleWeapon_ItemEffects", order = 1)]
public class RifleWeapon_ItemEffects : ItemInteractionEffects
{
    public int attackDamage = 1;
    public GameObject weaponPrefab;
    public float fireRate = 0.1f;
    public Skill skill;
    public Skill skill2;

    private float timeLastFired = 0;
    public override void OnUse(GameObject source, Item item)
    {
        if (isUsing)
        {
            if(timeLastFired + fireRate < Time.time)
            {
                PlayerController pc = source.GetComponent<PlayerController>();

                timeLastFired = Time.time;
                NetworkSpellManager.Instance.FireBulletServerRpc(NetworkGameManager.GetLocalPlayerId, pc.StatusManager.AttackDamage, pc.aimFollowTarget.transform.position);
            }
        }
    }

    public override void OnUseEnd(GameObject source,Item item)
    {
        source.GetComponent<PlayerController>().HandleAttack(false);
    }

    public override string EffectDescription(Item item)
    {
        string result = $"+{attackDamage} Attackdamage\n";
        result += "Shoot bullets in Rapidly";
        return result;
    }


    public override void OnEquip(GameObject source, Item item)
    {
        NetworkItemEffectsManager.Instance.EquipItemServerRpc(NetworkGameManager.GetLocalPlayerId, weaponPrefab.name);
        source.GetComponent<StatusManager>().weaponAttackDamage = attackDamage;
        if(skill != null)
        {
            skill.AssignSkill(source, 0);
        }
        timeLastFired = 0;

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
        Debug.Log("Dropping " + item.id);
    }
}