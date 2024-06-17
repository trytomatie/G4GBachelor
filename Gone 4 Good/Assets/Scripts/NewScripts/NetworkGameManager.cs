using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class NetworkGameManager : NetworkBehaviour
{
    public Dictionary<ulong, NetworkObject> connectedClients = new Dictionary<ulong, NetworkObject>();
    public MMF_Player floatingTextSpawner;

    // Singleton
    private static NetworkGameManager instance;



    public void Awake()
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
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            NetworkManager.Singleton.StartHost();
        }
    }

    public void AddClient(ulong id, NetworkObject client)
    {
        connectedClients.Add(id, client);
    }

    public static ulong GetLocalPlayerId
    {
        get { return NetworkManager.Singleton.LocalClientId; }
    }

    public static GameObject GetPlayerById(ulong id)
    {         
        return Instance.connectedClients[id].gameObject;
    }
    public static NetworkGameManager Instance { get => instance;}
}
