using System;
using System.Reflection;
using UnityEngine;

public class GunInteractionEffects : ItemInteractionEffects
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


    public int maxAmmo = 240;
    public int maxClip = 30;

    public float reloadTime = 2.5f;

    public bool SubstractAmmo(GameObject source,Item item)
    {
        if (item.currentClip > 0)
        {
            item.currentClip--;
            return true;
        }
        else
        {
            source.GetComponent<PlayerController>().ReloadCurrentItem();
            return false;
        }
    }

    public bool Reload(Item item)
    {
        int amountNeeded = maxClip - item.currentClip;
        if (item.currentAmmo >= amountNeeded)
        {
            item.currentAmmo -= amountNeeded;
            item.currentClip = maxClip;
            return true;
        }
        else if (item.currentAmmo > 0)
        {
            item.currentClip += item.currentAmmo;
            item.currentAmmo = 0;
            return true;
        }
        return false;

    }

    public bool CanReload(Item item)
    {
        return item.currentAmmo > 0 && item.currentClip < maxClip;
    }

    public override void ConstantUpdate(GameObject source, Item item)
    {
        base.ConstantUpdate(source, item);
        GameUI.instance.SetAmmo(item.currentClip, item.currentAmmo);
    }

    public override void OnUseEnd(GameObject source, Item item)
    {
        source.GetComponent<PlayerController>().anim.speed = 1;
        base.OnUseEnd(source, item);
    }
}
