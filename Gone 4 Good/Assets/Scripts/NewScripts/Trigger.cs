using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class Trigger : NetworkBehaviour
{
    public UnityEvent serverEvent;
    public NetworkVariable<bool> triggered = new NetworkVariable<bool>(false);

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<FPSController>() != null)
        {
            TriggerOnServerRpc();
        }
    }

    [Rpc(SendTo.Server)]
    public void TriggerOnServerRpc()
    {
        if (triggered.Value) 
        {
            return;
        }
        triggered.Value = true;
        serverEvent.Invoke();
    }
}
