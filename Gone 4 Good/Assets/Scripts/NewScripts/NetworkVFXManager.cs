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
                vfx[vfxIndex].transform.position = position;
                vfx[vfxIndex].transform.rotation = rotation;
                vfx[vfxIndex].PlayFeedbacks();
                break;
            case 1:
                vfx[vfxIndex].transform.position = position;
                vfx[vfxIndex].transform.rotation = rotation;
                vfx[vfxIndex].PlayFeedbacks();
                break;
            case 2:
                vfx[vfxIndex].transform.position = position;
                vfx[vfxIndex].transform.rotation = rotation;
                vfx[vfxIndex].PlayFeedbacks();
                break;
            case 3:
                vfx[vfxIndex].transform.position = position;
                vfx[vfxIndex].transform.rotation = rotation;
                vfx[vfxIndex].PlayFeedbacks();
                break;
            case 4:
                break;
        }

    }

    [Rpc(SendTo.ClientsAndHost)]
    public void SpawnVFXBulletLineRpc(ulong id, Vector3 end)
    {
        FPSController source = NetworkGameManager.GetPlayerById(id).GetComponent<FPSController>();
        Vector3 start = source.gunBarrelEnd.transform.position;
        if(id == NetworkManager.LocalClientId)
        {
            start = source.fpsgunbarrelEnd.transform.position;
        }
        GameObject bulletLine = Instantiate(projectileVFX[4], start, Quaternion.identity);
        bulletLine.GetComponent<BulletLineHandler>().enabled = true;
        bulletLine.GetComponent<BulletLineHandler>().start = start;
        bulletLine.GetComponent<BulletLineHandler>().end = end;
    }

    public static NetworkVFXManager Instance { get => instance;}
}

