using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class OnServerEnable : NetworkBehaviour
{
    public UnityEvent OnEnable;

    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            OnEnable.Invoke();
        }
    }
    
}
