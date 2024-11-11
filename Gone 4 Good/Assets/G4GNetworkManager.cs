using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class G4GNetworkManager : NetworkManager
{
    public UnityTransport lanTransport;
    public UnityTransport relayTransport;

    public void StartLanGame()
    {
        NetworkConfig.NetworkTransport = lanTransport;
        StartHost();
    }

    public void StartRelayGame()
    {
        NetworkConfig.NetworkTransport = relayTransport;
        StartHost();
    }
}
