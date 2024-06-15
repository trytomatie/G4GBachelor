using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkSpellManager : NetworkBehaviour
{
    // Singleton
    private static NetworkSpellManager instance;

    public BulletFirePoolable[] bulletFirePool;


    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    [Rpc(SendTo.Server)]
    public void FireBulletServerRpc(ulong sourcePlayer, int damage,Vector3 endPoint)
    {
        RaycastHit hit;
        PlayerController player = NetworkGameManager.GetPlayerById(sourcePlayer).GetComponent<PlayerController>();
        Vector3 gunBarrel = player.gunBarrelEnd.position;
        if (Physics.Raycast(gunBarrel, endPoint, out hit))
        {
            print("Hit: " + hit.collider.gameObject.name);
        }
        FireBulletVisualRpc(sourcePlayer, endPoint);
    }

    [Rpc(SendTo.Everyone)]
    public void FireBulletVisualRpc(ulong sourcePlayer, Vector3 endPoint)
    {
        BulletFirePoolable bulletFire = GetBulletFire();
        if(bulletFire != null)
        {
            print("Uhh no more bullets left in the pool!");
        }
        bulletFire.InUse = true;
        PlayerController player = NetworkGameManager.GetPlayerById(sourcePlayer).GetComponent<PlayerController>();
        Vector3 gunBarrel = player.gunBarrelEnd.position;
        LineRenderer lr = bulletFire.GetComponent<LineRenderer>();
        lr.SetPosition(0, gunBarrel);
        lr.SetPosition(1, endPoint);
    }

    private BulletFirePoolable GetBulletFire()
    { 
        foreach (BulletFirePoolable bulletFire in bulletFirePool)
        {
            if (!bulletFire.InUse)
            {
                return bulletFire;
            }
        }
        return null;
    }

    public static NetworkSpellManager Instance { get => instance; }

}
