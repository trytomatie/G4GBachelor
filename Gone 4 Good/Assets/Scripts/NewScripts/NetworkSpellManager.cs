using JetBrains.Annotations;
using MoreMountains.Feedbacks;
using NUnit.Framework.Constraints;
using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkSpellManager : NetworkBehaviour
{
    // Singleton
    private static NetworkSpellManager instance;
    public Transform bulletAimer;
    public BulletFirePoolable[] bulletFirePool;
    public MMF_Player bulletImpact;
    public LayerMask hitLayer;

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
    public void FireRaycastBulletServerRpc(ulong sourcePlayer, int damage,float spread)
    {
        RaycastHit hit;
        Vector2 randomSpread = UnityEngine.Random.insideUnitCircle * spread;
        PlayerController player = NetworkGameManager.GetPlayerById(sourcePlayer).GetComponent<PlayerController>();
        Vector3 gunBarrel = player.gunBarrelEnd.position;
        bulletAimer.transform.SetPositionAndRotation(player.transform.position + new Vector3(0, 1f, 0), player.transform.rotation);
        bulletAimer.transform.eulerAngles += new Vector3(0, randomSpread.y, 0);
        Ray ray = new Ray(bulletAimer.transform.position, bulletAimer.transform.forward);
        if (Physics.Raycast(ray, out hit,30, hitLayer))
        {
            Debug.Log("Hit Logic");
        }

        float distance = 30;
        Vector3 impactPosition = ray.GetPoint(30);
        if (hit.collider != null)
        {
            distance = hit.distance;
            impactPosition = hit.point;
        }
        FireRaycastBulletVisualRpc(sourcePlayer, impactPosition);
    }

    [Rpc(SendTo.Everyone)]
    public void FireRaycastBulletVisualRpc(ulong sourcePlayer,Vector3 impactPosition)
    {
        BulletFirePoolable bulletFire = GetBulletFire();
        if(bulletFire == null)
        {
            print("Uhh no more bullets left in the pool!");
        }
        bulletFire.impactPosition = impactPosition;

        PlayerController player = NetworkGameManager.GetPlayerById(sourcePlayer).GetComponent<PlayerController>();
        bulletFire.distanceUntilImpact = Vector3.Distance(player.transform.position, impactPosition);
        Vector3 gunBarrel = player.gunBarrelEnd.position;
        bulletFire.transform.position = gunBarrel;
        bulletFire.transform.LookAt(impactPosition);
        bulletFire.InUse = true;
    }

    public void ImpactBulletVisual(Vector3 impactPosition,Quaternion rotation)
    {
        bulletImpact.transform.position = impactPosition;
        bulletImpact.transform.rotation = rotation;
        bulletImpact.PlayFeedbacks();
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
