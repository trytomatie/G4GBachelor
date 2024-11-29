using MoreMountains.Tools;
using UnityEngine;
using static Unity.VisualScripting.Member;

[CreateAssetMenu(fileName = "RifleProjectileWeapon_ItemEffects", menuName = "ScriptableObjects/ItemInteractionEffects/RifleProjectileWeapon_ItemEffects", order = 1)]
public class RifleProjectileWeapon_ItemEffects : GunInteractionEffects
{

    private float timeLastFired = 0;
    private float perfectShotCounter = 0;
    private float currentSpread = 0;
    private Slowness slownessDebuff = new Slowness();
    public override void OnUse(GameObject source, Item item)
    {
        if (isUsing)
        {
            FPSController pc = source.GetComponent<FPSController>();
            if (timeLastFired + fireRate < Time.time && !source.GetComponent<FPSController>().isReloading)
            {
                if(SubstractAmmo(source,item) == false) return;

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
                pc.anim.SetBool("Attack", true);
                slownessDebuff.duration = 0.3f;
                if(!pc.sm.statusEffects.Contains(slownessDebuff))
                {
                    slownessDebuff.ApplyStatusEffect(pc.sm);
                }
                pc.anim.speed = 0.5f;
                NetworkSpellManager.Instance.FireProjectileRpc(NetworkGameManager.GetLocalPlayerId, currentSpread, pc.sm.AttackDamage, currentSpread,projectileSize,projectileSpeed, penetration, 1);
                NetworkVFXManager.Instance.SpawnVFXRpc(1, pc.gunBarrelEnd.transform.position, source.transform.rotation);
                AudioManager.instance.PlaySoundFromAudiolistRpc(1, pc.gunBarrelEnd.transform.position, 1);
            }
            else
            {
                pc.anim.SetBool("Attack", false);
            }
        }
    }

    public override void ConstantUpdate(GameObject source,Item item)
    {
        base.ConstantUpdate(source,item);
        if(!isUsing)
        {
            
            currentSpread = Mathf.Clamp(currentSpread - (spreadDecay * Time.deltaTime),0,spreadLimit);
            perfectShotCounter = 0;
        }

    }

    public override void OnUseEnd(GameObject source,Item item)
    {
        base.OnUseEnd(source,item);
        FPSController pc = source.GetComponent<FPSController>();
        pc.anim.SetBool("Attack", false);
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
        timeLastFired = 0;
        currentSpread = 0;

    }

    public override void OnUnequip(GameObject source, Item item)
    {
        NetworkItemEffectsManager.Instance.UnequipItemServerRpc(NetworkGameManager.GetLocalPlayerId);
        source.GetComponent<StatusManager>().weaponAttackDamage = 0;
    }

    public override void OnDrop(GameObject source, Item item)
    {
        NetworkItemEffectsManager.Instance.UnequipItemServerRpc(NetworkGameManager.GetLocalPlayerId);
    }
}