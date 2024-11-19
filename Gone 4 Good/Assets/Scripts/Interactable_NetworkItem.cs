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
        ItemBlueprint itemBlueprint = ItemDatabase.GetItem(itemID);
        Inventory inventory = source.GetComponent<Inventory>();
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
        Inventory inventory = player.GetComponent<Inventory>();
        // Add Item to Player
        Item item = new Item(itemID, amount);
        int assignedSlot = -1;
        switch (item.BluePrint.itemType)
        {
            case ItemType.Weapon:
                item.currentAmmo = networkItemData.currentAmmo;
                item.currentClip = networkItemData.currentClip;
                assignedSlot = 0;
                break;
            case ItemType.Consumable:
                assignedSlot = 1;
                break;
            default:
                break;
        }
        
        if (assignedSlot != -1)
        {
            if (inventory.items[assignedSlot] == new Item(0,0) || inventory.items[assignedSlot] == null)
            {
                inventory.items[assignedSlot] = item;
                inventory.SwitchHotbarItem(player.GetComponent<Inventory>().currentHotbarIndex);
            }
            else
            {
                inventory.items[assignedSlot].GetItemInteractionEffects.OnDrop(player.gameObject, inventory.items[assignedSlot]);
                NetworkItemData data = new NetworkItemData()
                {
                    affinity = 0,
                    currentAmmo = inventory.items[assignedSlot].currentAmmo,
                    currentClip = inventory.items[assignedSlot].currentClip
                };
                player.GetComponent<FPSController>().DropEquipedItemRpc(OwnerClientId, inventory.items[assignedSlot].id, data);
                inventory.items[assignedSlot] = item;
                inventory.SwitchHotbarItem(player.GetComponent<Inventory>().currentHotbarIndex);
            }
            inventory.onInventoryUpdate(assignedSlot);
            DespawnRpc();

        }

    }

    [Rpc(SendTo.Server)]
    public void DespawnRpc()
    {
        NetworkObject networkObject = GetComponent<NetworkObject>();
        networkObject.Despawn(true);
    }
}
