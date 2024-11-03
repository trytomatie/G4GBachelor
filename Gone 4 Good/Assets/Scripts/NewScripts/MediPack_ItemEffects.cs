
using UnityEngine;
[CreateAssetMenu(fileName = "MediPack_ItemEffects", menuName = "ScriptableObjects/ItemInteractionEffects/MediPack_ItemEffects", order = 1)]
public class MediPack_ItemEffects : ItemInteractionEffects
{
    public int healAmount = 50;
    public GameObject handModel;

    public override void OnUse(GameObject source, Item item)
    {
        if (isUsing)
        {
            Inventory inventory = source.GetComponent<FPSController>().inventory;
            source.GetComponent<StatusManager>().Hp.Value += healAmount + Random.Range(0, 10);
            inventory.items[inventory.currentHotbarIndex] = new Item(0, 0);
            OnUnequip(source, item);
        }
    }

    public override string EffectDescription(Item item)
    {
        return "Heals " + healAmount + " health";
    }

    public override void OnEquip(GameObject source, Item item)
    {
        NetworkItemEffectsManager.Instance.EquipItemServerRpc(NetworkGameManager.GetLocalPlayerId, handModel.name, 1);
        isUsing = false;
    }


    public override void OnUnequip(GameObject source, Item item)
    {
        NetworkItemEffectsManager.Instance.UnequipItemServerRpc(NetworkGameManager.GetLocalPlayerId);
    }

    public override void OnDrop(GameObject source, Item item)
    {
        NetworkItemEffectsManager.Instance.UnequipItemServerRpc(NetworkGameManager.GetLocalPlayerId);
    }
}
