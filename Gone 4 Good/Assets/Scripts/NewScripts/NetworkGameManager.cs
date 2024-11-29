using MoreMountains.Feedbacks;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;


public class NetworkGameManager : NetworkBehaviour
{
    public Dictionary<ulong, NetworkObject> connectedClients = new Dictionary<ulong, NetworkObject>();
    public MMF_Player floatingTextSpawner;

    // Singleton
    private static NetworkGameManager instance;
    public static bool enableDDA = false;
    public static string connectedPlayerName;

    public GameObject playerPrefab;

    public Dictionary<GameObject, NavMeshPath> calculatedPaths = new Dictionary<GameObject, NavMeshPath>();

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

    [Rpc(SendTo.SpecifiedInParams)]
    public void RequestSpawnOnServerRpc(ulong clientId,RpcParams rpcParams = default)
    {
        SpawnPlayerOnServerRpc(PlayerPrefs.GetString("PlayerName", "Unknown"),clientId);
    }

    [Rpc(SendTo.Server)]
    public void SpawnPlayerOnServerRpc(FixedString128Bytes value,ulong clientId)
    {
        print(value.ToString());
        string playerName = value.ToString();
        if (playerName == "Spectator1337") return;
        GameObject go = Instantiate(playerPrefab);
        go.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.J))
        {
            NetworkManager.Singleton.StartHost();
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            NetworkManager.Singleton.StartClient();
        }
        return;
        if(IsServer)
        {
            CalculateNavmeshPath();
        }
    }

    public void CalculateNavmeshPath()
    {
        foreach (var player in connectedClients.Values)
        {
            NavMeshPath path = new NavMeshPath();
            NavMesh.CalculatePath(player.gameObject.transform.position, GetRandomPlayer().transform.position, NavMesh.AllAreas, path);
            // Override if already added
            if (calculatedPaths.ContainsKey(player.gameObject))
            {
                calculatedPaths[player.gameObject] = path;
            }
            else
            {
                calculatedPaths.Add(player.gameObject, path);
            }
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

    public static GameObject[] GetAllConnectedPlayers()
    {
        return Instance.connectedClients.Values.Select(x => x.gameObject).ToArray();
    }

    public static GameObject GetRandomPlayer()
    {
        if (Instance.connectedClients.Count == 0) return null;
        return Instance.connectedClients.Values.ElementAt(Random.Range(0, Instance.connectedClients.Count)).gameObject;
    }
    public static NetworkGameManager Instance { get => instance;}
}
