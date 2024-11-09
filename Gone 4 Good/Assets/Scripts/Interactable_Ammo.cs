using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Interactable_Ammo : Interactable
{
    public override void Interact(GameObject source)
    {
        source.GetComponent<Inventory>().items[0].currentAmmo = source.GetComponent<Inventory>().items[0].maxAmmo;
    }
}
