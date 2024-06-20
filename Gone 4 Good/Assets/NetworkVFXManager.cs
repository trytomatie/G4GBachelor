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
        switch(vfxIndex)
        {
            case 0:
                vfx[vfxIndex].GetFeedbackOfType<MMF_ParticlesInstantiation>().TargetWorldPosition = position;
                vfx[vfxIndex].PlayFeedbacks();
                break;
            case 1:
                vfx[vfxIndex].transform.position = position;
                vfx[vfxIndex].transform.rotation = rotation;
                vfx[vfxIndex].PlayFeedbacks();
                break;
        }

    }

    public static NetworkVFXManager Instance { get => instance;}
}
