using System.Collections;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct NetworkItemData : INetworkSerializable
{
    public int currentAmmo;
    public int currentClip;
    public Affinity affinity;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref currentAmmo);
        serializer.SerializeValue(ref currentClip);
        serializer.SerializeValue(ref affinity);
    }

    
}
