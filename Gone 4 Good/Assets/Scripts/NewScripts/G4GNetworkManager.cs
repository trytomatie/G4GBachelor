using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class G4GNetworkManager : NetworkManager
{
    public UnityTransport lanTransport;
    public UnityTransport relayTransport;
    public string relayCode;


    // Start is called before the first frame update
    private void Start()
    {


    }

    private async Task ConnectToRelayService()
    {

        NetworkManager.Singleton.NetworkConfig.NetworkTransport = relayTransport;
        InitializationOptions initializationOptions = new InitializationOptions();
        initializationOptions.SetProfile(UnityEngine.Random.Range(0, 10000000).ToString());
        await UnityServices.InitializeAsync(initializationOptions);
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            AuthenticationService.Instance.SignedIn += () => { Debug.Log("Has signed in" + AuthenticationService.Instance.PlayerId); };
            await AuthenticationService.Instance.SignInAnonymouslyAsync();


        }
    }
    public void StartLanGame()
    {
        NetworkConfig.NetworkTransport = lanTransport;
        StartHost();
    }

    public async void StartRelayGame()
    {
        await ConnectToRelayService();
        string relayCode = await CreateRelay(5);
        print($"RelayCode: {relayCode}");
    }

    public async void JoinRelayGame(string code)
    {
        relayCode = code;
        await ConnectToRelayService();
        JoinRelay(code);
    }

    /// <summary>
    /// In Summary, Start the Game for the Server and Host
    /// </summary>
    /// <param name="p_lobby"></param>
    /// <returns></returns>
    private async Task<string> CreateRelay(int maxPlayers)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            relayCode = joinCode;
            print(relayTransport);
            relayTransport.SetHostRelayData
            (
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );
            Singleton.OnClientConnectedCallback += (clientId) =>
            {

                NetworkGameManager.Instance.RequestSpawnOnServerRpc(clientId,RpcTarget.Single(clientId,RpcTargetUse.Temp));
            };
            StartHost();


            return joinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
        return "0";
    }

    /// <summary>
    /// In Sumary, Starts the Game for the Clients
    /// </summary>
    /// <param name="joinCode"></param>
    public async void JoinRelay(string joinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            print(relayTransport);
            relayTransport.SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
                );
            NetworkManager.Singleton.StartClient();

        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e);
        }
    }


}
