using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Interactable_NetworkItem : Interactable
{
    private bool stale = false;
    public int itemID;
    public int amount = 1;
    public NetworkItemData networkItemData;
    public override void Interact(GameObject source)
    {
        NetworkObject networkObject = source.GetComponent<NetworkObject>();
        ulong id = networkObject.OwnerClientId;
        GivePickupServerRpc(id);
    }

    [Rpc(SendTo.Server)]
    public void GivePickupServerRpc(ulong id)
    {
        if(stale)
        {
            return;
        }
        NetworkObject player = NetworkGameManager.GetPlayerById(id).GetComponent<NetworkObject>();
        stale = true;
        // Despawn Item
        // Add Item to Player
        RpcParams rpcParams = new RpcParams
        {
            Send = new RpcSendParams
            {
                Target = RpcTarget.Single(id, RpcTargetUse.Temp)
            }
        };
        GivePickupClientRpc(id, networkItemData, rpcParams);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void GivePickupClientRpc(ulong id, NetworkItemData networkItemData,RpcParams rpcParams = default)
    {
        NetworkObject player = NetworkGameManager.GetPlayerById(id).GetComponent<NetworkObject>();
        
        // Add Item to Player
        Item item = new Item(itemID, amount);
        item.currentAmmo = networkItemData.currentAmmo;
        item.currentClip = networkItemData.currentClip;
        player.GetComponent<Inventory>().AddItem(item);
        player.GetComponent<Inventory>().SwitchHotbarItem(player.GetComponent<Inventory>().currentHotbarIndex);
        NetworkObject networkObject = GetComponent<NetworkObject>();
        DespawnRpc();
    }

    [Rpc(SendTo.Server)]
    public void DespawnRpc()
    {
        NetworkObject networkObject = GetComponent<NetworkObject>();
        networkObject.Despawn(true);
    }
}
