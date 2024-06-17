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
    public GameObject networkProjectile;
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
    public void FireRaycastBulletServerRpc(ulong sourcePlayer,float clientRotation, int damage,float spread)
    {
        RaycastHit hit;
        Vector2 randomSpread = UnityEngine.Random.insideUnitCircle * spread;
        PlayerController player = NetworkGameManager.GetPlayerById(sourcePlayer).GetComponent<PlayerController>();
        Vector3 gunBarrel = player.gunBarrelEnd.position;
        bulletAimer.transform.SetPositionAndRotation(player.transform.position + new Vector3(0, 1f, 0), Quaternion.Euler(0, clientRotation,0));
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
            NetworkGameManager.Instance.floatingTextSpawner.transform.position = impactPosition;
            NetworkGameManager.Instance.floatingTextSpawner.PlayFeedbacks();
        }
        FireRaycastBulletVisualRpc(sourcePlayer, impactPosition);
    }

    [Rpc(SendTo.ClientsAndHost)]
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

    [Rpc(SendTo.Server)]
    public void FireProjectileRpc(ulong sourcePlayer, float clientRotation, int damage, float spread,float size,float speed,int visual)
    {
        Vector2 randomSpread = UnityEngine.Random.insideUnitCircle * spread;

        PlayerController player = NetworkGameManager.GetPlayerById(sourcePlayer).GetComponent<PlayerController>();
        bulletAimer.transform.SetPositionAndRotation(player.transform.position + new Vector3(0, 1f, 0), Quaternion.Euler(0, clientRotation, 0));
        bulletAimer.transform.eulerAngles += new Vector3(0, randomSpread.y, 0);
        Vector3 gunBarrel = player.gunBarrelEnd.position;
        GameObject spawnedProjectile = Instantiate(networkProjectile, gunBarrel, bulletAimer.rotation);
        NetworkObject networkObject = spawnedProjectile.GetComponent<NetworkObject>();
        networkObject.Spawn();

        // SpawnLogic
        Rigidbody rb = spawnedProjectile.GetComponent<Rigidbody>();
        rb.linearVelocity = spawnedProjectile.transform.forward * speed;
        FireProjectileVisualRpc(sourcePlayer,networkObject,visual,speed);

    }

    [Rpc(SendTo.ClientsAndHost)]
    private void FireProjectileVisualRpc(ulong sourcePlayer,NetworkObjectReference networkObjectReference,int visual,float speed)
    {
        PlayerController player = NetworkGameManager.GetPlayerById(sourcePlayer).GetComponent<PlayerController>();
        Vector3 gunBarrel = player.gunBarrelEnd.position;
        networkObjectReference.TryGet(out NetworkObject networkObject);
        networkObject.transform.position = gunBarrel;
        Rigidbody rb = networkObject.GetComponent<Rigidbody>();
        rb.linearVelocity = networkObject.transform.forward * speed;
        GameObject projectile = networkObject.gameObject;
        GameObject vfx = Instantiate(NetworkVFXManager.Instance.projectileVFX[visual], projectile.transform.position, projectile.transform.rotation,projectile.transform);
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
