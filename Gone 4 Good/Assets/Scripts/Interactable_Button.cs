using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Interactable_Button : Interactable
{
    public UnityEvent localEvent;
    public UnityEvent serverEvent;
    public bool oneTimeUse = false;
    public NetworkVariable<bool> used = new NetworkVariable<bool>(false);
    public override void Interact(GameObject source)
    {
        if (used.Value && oneTimeUse)
        {
            return;
        }
        localEvent.Invoke();
        ServerInteractRpc();

    }

    [Rpc(SendTo.Server)]
    public void ServerInteractRpc()
    {
        serverEvent.Invoke();
        if (oneTimeUse)
        {
            used.Value = true;
        }
    }
}
