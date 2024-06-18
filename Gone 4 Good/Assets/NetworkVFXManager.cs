using MoreMountains.Feedbacks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class NetworkVFXManager : NetworkBehaviour
{
    private static NetworkVFXManager instance;
    public GameObject[] projectileVFX;
    public MMF_Player[] vfx;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnVFXRpc(int vfxIndex, Vector3 position, Quaternion rotation)
    {
        vfx[vfxIndex].transform.position = position;
        vfx[vfxIndex].transform.rotation = rotation;
        vfx[vfxIndex].PlayFeedbacks();
    }

    public static NetworkVFXManager Instance { get => instance;}
}
