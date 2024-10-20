using System.Collections;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static Unity.VisualScripting.Member;


public class NetworkItemEffectsManager : NetworkBehaviour
{
    public GameObject[] weaponPrefabs;
    // Singleton
    private static NetworkItemEffectsManager instance;


    // Use this for initialization
    void Awake()
    {
        if (Instance == null)
        {
            instance = this;
        }
        else
        {
                Destroy(this);
        }
    }


    [Rpc(SendTo.Server)]
    public void EquipItemServerRpc(ulong playerId,string weaponPrefabName,int upperBodyState)
    {
        EquipItemClientRPC(playerId,weaponPrefabName, upperBodyState);
    }


    [Rpc(SendTo.Everyone)]
    public void EquipItemClientRPC(ulong id,string weaponPrefabName,int upperBodyState)
    {
        GameObject source = NetworkGameManager.GetPlayerById(id);
        source.GetComponent<FPSController>().anim.SetInteger("UpperBody", upperBodyState);
        if (source.GetComponent<FPSController>().WeaponPivot.childCount > 0)
        {
            Destroy(source.GetComponent<FPSController>().WeaponPivot.GetChild(0).gameObject);
        }

        GameObject weaponPrefab = weaponPrefabs.FirstOrDefault(weapon => weapon.name == weaponPrefabName);
        if(weaponPrefab == null)
        {
            Debug.LogError("WeaponPrefab not found");
            return;
        }
        Transform weaponPivot = source.GetComponent<FPSController>().WeaponPivot;
        Transform fpsWeaponPivot = source.GetComponent<FPSController>().FPSWeaponPivot;
        if(source.GetComponent<NetworkObject>().OwnerClientId == NetworkManager.LocalClientId)
        {
            InstantiateWeaponToPivot(source, weaponPrefab, fpsWeaponPivot, true);
        }
        else
        {
            InstantiateWeaponToPivot(source, weaponPrefab, weaponPivot, false);
        }

    }

    private void InstantiateWeaponToPivot(GameObject source,GameObject weaponPrefab, Transform weaponPivot,bool fps)
    {
        GameObject instaniatedWeapon = Instantiate(weaponPrefab, weaponPivot);
        instaniatedWeapon.transform.localScale = weaponPrefab.transform.localScale;
        Transform gunBarrelEnd = instaniatedWeapon.transform.Find("GunBarrelEnd").transform;
        if (gunBarrelEnd != null)
        {
            if(fps)
            {
                source.GetComponent<FPSController>().fpsgunbarrelEnd = gunBarrelEnd;
                instaniatedWeapon.layer = LayerMask.NameToLayer("FPSPlayer");
                // also children
                foreach (Transform child in instaniatedWeapon.GetComponentsInChildren<Transform>())
                {
                    child.gameObject.layer = LayerMask.NameToLayer("FPSPlayer");
                }
            }
            else
            {
                source.GetComponent<FPSController>().gunBarrelEnd = gunBarrelEnd;
                instaniatedWeapon.layer = LayerMask.NameToLayer("Player");
                // also children
                foreach (Transform child in instaniatedWeapon.GetComponentsInChildren<Transform>())
                {
                    child.gameObject.layer = LayerMask.NameToLayer("Player");
                }
            }

        }
    }

    [Rpc(SendTo.Server)]
    public void UnequipItemServerRpc(ulong playerId)
    {
        UnequipItemClientRPC(playerId);
    }

    [Rpc(SendTo.Everyone)]
    public void UnequipItemClientRPC(ulong id)
    {
        GameObject source = NetworkGameManager.GetPlayerById(id);
        source.GetComponent<FPSController>().anim.SetInteger("UpperBody", 0);
        if (source.GetComponent<FPSController>().WeaponPivot.childCount >0)
        {
            Destroy(source.GetComponent<FPSController>().WeaponPivot.GetChild(0).gameObject);
        }
    }


    public static NetworkItemEffectsManager Instance { get => instance; }
}
