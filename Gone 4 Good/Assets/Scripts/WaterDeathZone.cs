using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterDeathZone : MonoBehaviour
{
    public Transform respawnPoint;
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            other.gameObject.GetComponent<PlayerController>().SwitchPlayerState(PlayerController.PlayerState.VoidOut);
        }
        if(other.GetComponent<FPSController>() != null) 
        {
            other.GetComponent<FPSController>().Teleport(respawnPoint.position);
        }
    }
}
