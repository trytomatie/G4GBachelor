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
    public void EquipItemServerRpc(ulong playerId,string weaponPrefabName)
    {
        EquipItemClientRPC(playerId,weaponPrefabName);
    }


    [Rpc(SendTo.Everyone)]
    public void EquipItemClientRPC(ulong id,string weaponPrefabName)
    {
        GameObject source = NetworkGameManager.GetPlayerById(id);
        if(source.GetComponent<PlayerController>().weaponPivot.transform.childCount > 0)
        {
            Destroy(source.GetComponent<PlayerController>().weaponPivot.transform.GetChild(0).gameObject);
        }
        GameObject weaponPrefab = weaponPrefabs.FirstOrDefault(weapon => weapon.name == weaponPrefabName);
        if(weaponPrefab == null)
        {
            Debug.LogError("WeaponPrefab not found");
            return;
        }
        Transform weaponPivot = source.GetComponent<PlayerController>().weaponPivot.transform;
        GameObject instaniatedWeapon = Instantiate(weaponPrefab, weaponPivot);
        instaniatedWeapon.transform.localScale = weaponPrefab.transform.localScale;
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
        if(source.GetComponent<PlayerController>().weaponPivot.transform.childCount >0)
        {
            Destroy(source.GetComponent<PlayerController>().weaponPivot.transform.GetChild(0).gameObject);
        }
    }


    public static NetworkItemEffectsManager Instance { get => instance; }
}
