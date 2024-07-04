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

    public int currentAmmo = 160;
    public int maxAmmo = 240;
    public int currentClip = 30;
    public int maxClip = 30;

    public bool SubstractAmmo()
    {
        if (currentClip > 0)
        {
            currentClip--;
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool Reload()
    {
        int amountNeeded = maxClip - currentClip;
        if (currentAmmo >= amountNeeded)
        {
            currentAmmo -= amountNeeded;
            currentClip = maxClip;
            return true;
        }
        else if (currentAmmo > 0)
        {
            currentClip += currentAmmo;
            currentAmmo = 0;
            return true;
        }
        return false;

    }
}
