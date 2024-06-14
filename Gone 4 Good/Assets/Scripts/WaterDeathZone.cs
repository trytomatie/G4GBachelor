using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterDeathZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            other.gameObject.GetComponent<PlayerController>().SwitchPlayerState(PlayerController.PlayerState.InWater);
        }
    }
}
