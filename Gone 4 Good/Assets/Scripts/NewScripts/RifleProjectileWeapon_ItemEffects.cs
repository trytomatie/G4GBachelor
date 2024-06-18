using MoreMountains.Tools;
using UnityEngine;
using static Unity.VisualScripting.Member;

[CreateAssetMenu(fileName = "RifleProjectileWeapon_ItemEffects", menuName = "ScriptableObjects/ItemInteractionEffects/RifleProjectileWeapon_ItemEffects", order = 1)]
public class RifleProjectileWeapon_ItemEffects : ItemInteractionEffects
{
    public int attackDamage = 1;
    public GameObject weaponPrefab;
    public float fireRate = 0.1f;
    public float projectileSpeed = 30;
    public float projectileSize = 0.2f;
    public int penetration = 0;
    public float spreadAccumulation = 0.05f;
    public float perfectShots = 3;
    public float startSpread = 0f;
    public float spreadLimit = 1;
    public float spreadDecay = 0.25f;
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
                PlayerController pc = source.GetComponent<PlayerController>();
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
                NetworkSpellManager.Instance.FireProjectileRpc(NetworkGameManager.GetLocalPlayerId, source.transform.eulerAngles.y, pc.StatusManager.AttackDamage, currentSpread,projectileSize,projectileSpeed, penetration, 0);
                NetworkVFXManager.Instance.SpawnVFXRpc(1, source.gameObject.GetComponent<PlayerController>().gunBarrelEnd.transform.position, source.transform.rotation);
            }
        }
    }

    public override void ConstantUpdate(GameObject source,Item item)
    {
        if(!isUsing)
        {
            
            currentSpread = Mathf.Clamp(currentSpread - (spreadDecay * Time.deltaTime),0,spreadLimit);
            perfectShotCounter = 0;
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
        Debug.Log("Dropping " + item.id);
    }
}