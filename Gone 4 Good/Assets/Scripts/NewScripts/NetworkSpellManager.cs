using MoreMountains.Feedbacks;
using System.Collections.Generic;
using System.Linq;
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

    List<StatusManager> hitlist = new List<StatusManager>();
    public void FireRaycastBullet(ulong sourcePlayer,float spread, int damage,int penetration)
    {
        hitlist.Clear();
        FPSController player = NetworkGameManager.GetPlayerById(sourcePlayer).GetComponent<FPSController>();
        Vector3 forward = player.playerCamera.transform.forward;
        // factor in spread
        Vector2 randomSpread = UnityEngine.Random.insideUnitCircle * spread * 0.01f;
        forward.x += randomSpread.x;
        forward.y += randomSpread.y;
        forward.Normalize();
        Ray ray = new Ray(player.playerCamera.transform.position, forward);
        RaycastHit[] hits = Physics.SphereCastAll(ray, player.GetComponent<DDAData>().weaponSpherecastRadius.Value, 100, hitLayer);
        // Bullet Fire
        if(sourcePlayer == NetworkManager.Singleton.LocalClientId)
        {
            NetworkVFXManager.Instance.SpawnVFXRpc(1,player.fpsgunbarrelEnd.position,Quaternion.identity);
            NetworkVFXManager.Instance.SpawnVFXRpc(3, player.fpsgunbarrelEnd.position, player.fpsgunbarrelEnd.rotation);
        }
        else
        {
            NetworkVFXManager.Instance.SpawnVFXRpc(1, player.gunBarrelEnd.position, Quaternion.identity);
            NetworkVFXManager.Instance.SpawnVFXRpc(3, player.gunBarrelEnd.position, player.gunBarrelEnd.rotation);
        }
        AudioManager.instance.PlaySoundFromAudiolistRpc(1, player.transform.position, 1);
        float distance = 100;
        Vector3 impactPosition = ray.GetPoint(100);
        hits = hits.OrderBy(hits => hits.distance).ToArray();
        bool hasHitEntity = false;
        bool hasHitHead = false;
        foreach (RaycastHit hit in hits)
        {
            if(hit.collider.gameObject.name.Contains("Head"))
            {
                hasHitHead = true;
            }
        }
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider != null)
            {
                distance = hit.distance;
                impactPosition = hit.point;
                StatusManager sm = hit.collider.transform.root.GetComponent<StatusManager>() ?? null;
                print(hit.collider.gameObject.name);
                if (sm != null)
                {
                    if(hitlist.Contains(sm))
                    {
                        continue;
                    }
                    if(sm.Hp.Value > 0)
                    {
                        hasHitEntity = true;
                    }

                    hitlist.Add(sm);
                    if(hasHitHead)
                    {
                        damage *= 3;
                    }
                    hit.collider.transform.root.GetComponent<StatusManager>().ApplyDamageRpc(damage, player.transform.position, 0);
                    NetworkVFXManager.Instance.SpawnVFXRpc(2, impactPosition, Quaternion.LookRotation(-hit.normal));

                }
                else
                {
                    PerformanceTracker.WriteToCurrentStack(hasHitEntity, hasHitHead);
                    NetworkVFXManager.Instance.SpawnVFXRpc(0, impactPosition, Quaternion.LookRotation(-hit.normal));
                    break;
                }
                penetration--;
            }
            if(penetration <= 0)
            {
                PerformanceTracker.WriteToCurrentStack(hasHitEntity, hasHitHead);
                break;
            }
        }
        NetworkVFXManager.Instance.SpawnVFXBulletLineRpc(sourcePlayer, impactPosition);
        //FireRaycastBulletVisualRpc(sourcePlayer, impactPosition);
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
        // Set to constant height
        gunBarrel.y = 1.325f;
        bulletFire.transform.position = gunBarrel;
        bulletFire.transform.LookAt(impactPosition);
        bulletFire.InUse = true;
    }

    [Rpc(SendTo.Server)]
    public void FireProjectileRpc(ulong sourcePlayer, float clientRotation, int damage, float spread,float size,float speed,int penetration,int visual)
    {
        Vector2 randomSpread = UnityEngine.Random.insideUnitCircle * spread;

        PlayerController player = NetworkGameManager.GetPlayerById(sourcePlayer).GetComponent<PlayerController>();
        bulletAimer.transform.SetPositionAndRotation(player.transform.position + new Vector3(0, 1f, 0), Quaternion.Euler(0, clientRotation, 0));
        bulletAimer.transform.eulerAngles += new Vector3(0, randomSpread.y, 0);
        Vector3 gunBarrel = player.gunBarrelEnd.position;
        gunBarrel.y = 1.325f;
        GameObject spawnedProjectile = Instantiate(networkProjectile, gunBarrel, bulletAimer.rotation);
        spawnedProjectile.GetComponent<NetworkProjectile>().damage = damage;
        spawnedProjectile.GetComponent<NetworkProjectile>().soruce = player.GetComponent<StatusManager>();
        NetworkObject networkObject = spawnedProjectile.GetComponent<NetworkObject>();
        networkObject.Spawn();

        // SpawnLogic
        Rigidbody rb = spawnedProjectile.GetComponent<Rigidbody>();
        rb.linearVelocity = spawnedProjectile.transform.forward * speed;
        FireProjectileVisualRpc(sourcePlayer,networkObject,visual,speed);
    }

    [Rpc(SendTo.Server)]
    public void FireNPCProjectileRpc(NetworkObjectReference npc,int visual,float speed,Quaternion direction,int damage)
    {
        NetworkObject npcNetworkObject;
        if(npc.TryGet(out npcNetworkObject))
        {
            Vector3 projectileSpawnPosition = npcNetworkObject.transform.position;
            GameObject spawnedProjectile = Instantiate(networkProjectile, projectileSpawnPosition + new Vector3(0, 1.325f,0), direction);
            spawnedProjectile.GetComponent<NetworkProjectile>().damage = damage;
            spawnedProjectile.GetComponent<NetworkProjectile>().soruce = npcNetworkObject.GetComponent<StatusManager>();
            NetworkObject networkObject = spawnedProjectile.GetComponent<NetworkObject>();
            networkObject.Spawn();

            // SpawnLogic
            Rigidbody rb = spawnedProjectile.GetComponent<Rigidbody>();
            rb.linearVelocity = spawnedProjectile.transform.forward * speed;
            FireNPCProjectileVisualRpc(networkObject, visual, speed);
        }

    }

    [Rpc(SendTo.ClientsAndHost)]
    private void FireNPCProjectileVisualRpc(NetworkObjectReference networkObjectReference, int visual, float speed)
    {
        NetworkObject networkObject;
        if(networkObjectReference.TryGet(out networkObject))
        {
            GameObject projectile = networkObject.gameObject;
            GameObject vfx = Instantiate(NetworkVFXManager.Instance.projectileVFX[visual], projectile.transform.position, projectile.transform.rotation, projectile.transform);
            projectile.GetComponent<NetworkProjectile>().attchedVFX = vfx;
        }
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
        projectile.GetComponent<NetworkProjectile>().attchedVFX = vfx;
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
