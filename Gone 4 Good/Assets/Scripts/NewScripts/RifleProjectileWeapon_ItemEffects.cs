using MoreMountains.Tools;
using UnityEngine;
using static Unity.VisualScripting.Member;

[CreateAssetMenu(fileName = "RifleProjectileWeapon_ItemEffects", menuName = "ScriptableObjects/ItemInteractionEffects/RifleProjectileWeapon_ItemEffects", order = 1)]
public class RifleProjectileWeapon_ItemEffects : GunInteractionEffects
{

    private float timeLastFired = 0;
    private float perfectShotCounter = 0;
    private float currentSpread = 0;
    public override void OnUse(GameObject source, Item item)
    {
        if (isUsing)
        {
            if(timeLastFired + fireRate < Time.time)
            {
                if(SubstractAmmo(item) == false) return;
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
                source.GetComponent<PlayerController>().anim.SetTrigger("Attack");
                NetworkSpellManager.Instance.FireProjectileRpc(NetworkGameManager.GetLocalPlayerId, source.transform.eulerAngles.y, pc.StatusManager.AttackDamage, currentSpread,projectileSize,projectileSpeed, penetration, 1);
                NetworkVFXManager.Instance.SpawnVFXRpc(1, source.gameObject.GetComponent<PlayerController>().gunBarrelEnd.transform.position, source.transform.rotation);
                AudioManager.instance.PlaySoundFromAudiolistRpc(1, source.gameObject.GetComponent<PlayerController>().gunBarrelEnd.transform.position, 1);
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