using System.Collections;
using Unity.Netcode;
using UnityEngine;


public struct NetworkItemData : INetworkSerializable
{
    public int currentAmmo;
    public int maxAmmo;
    public int maxClip;
    public int currentClip;
    public Affinity affinity;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref currentAmmo);
        serializer.SerializeValue(ref maxAmmo);
        serializer.SerializeValue(ref maxClip);
        serializer.SerializeValue(ref currentClip);
        serializer.SerializeValue(ref affinity);
    }

    
}
