using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RemnantRevivalZone : MonoBehaviour
{
    public static List<RemnantRevivalZone> allRevivalZones = new List<RemnantRevivalZone>();
    public float range = 3;
    public float remnantRevivalTime = 8;
    public float remnantRevivalTimer = 0;

    public FPSController fpsController;

    private FPSController[] playersReviving = new FPSController[4];
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        allRevivalZones.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < playersReviving.Length; i++)
        {
            if (playersReviving[i] != null)
            {
                StatusManager statusManager = playersReviving[i].GetComponent<StatusManager>();
                if (statusManager.hp.Value > 0)
                {
                    playersReviving[i].UpdateRemnantRevivalBarUIRpc(fpsController.OwnerClientId, 1 - remnantRevivalTimer / remnantRevivalTime);
                    fpsController.UpdateRemnantRevivalBarUIRpc(fpsController.OwnerClientId, 1 - remnantRevivalTimer / remnantRevivalTime);
                    remnantRevivalTimer += Time.deltaTime;
                    if (remnantRevivalTimer >= remnantRevivalTime)
                    {
                        fpsController.RecoverFromRemnantTransformation();
                    }
                }
            }
        }
    }

    private void OnEnable()
    {
        remnantRevivalTimer = 0;
    }

    private void OnDisable()
    {
        remnantRevivalTimer = 0;
        fpsController.UpdateRemnantRevivalBarUIRpc(fpsController.OwnerClientId, 0);
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<FPSController>() != null) 
        {
            StatusManager sm = other.GetComponent<StatusManager>();
            if(sm.Hp.Value > 0)
            {
                for(int i = 0; i < playersReviving.Length; i++)
                {
                    if (playersReviving[i] == null)
                    {
                        playersReviving[i] = other.GetComponent<FPSController>();
                        break;
                    }
                }
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        FPSController player = other.GetComponent<FPSController>();
        if (player != null)
        {
            for (int i = 0; i < playersReviving.Length; i++)
            {
                if (playersReviving[i] == other.GetComponent<FPSController>())
                {
                    playersReviving[i] = null;
                    break;
                }
            }
            player.UpdateRemnantRevivalBarUIRpc(fpsController.OwnerClientId, 0);
        }

    }
}
