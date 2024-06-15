using System.Collections;
using Unity.Netcode;
using UnityEngine;


public class NetworkGameManager : NetworkBehaviour
{
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public static ulong GetLocalPlayerId
    {
        get { return NetworkManager.Singleton.LocalClientId; }
    }

    public static GameObject GetPlayerById(ulong id)
    {         
        return NetworkManager.Singleton.ConnectedClients[id].PlayerObject.gameObject;
    }
}
