using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class Interactable_Button : Interactable
{
    public UnityEvent localEvent;
    public override void Interact(GameObject source)
    {
        localEvent.Invoke();
    }
}
