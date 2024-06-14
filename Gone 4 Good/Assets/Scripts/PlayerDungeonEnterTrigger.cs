using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerDungeonEnterTrigger : MonoBehaviour
{
    Vector3 dungeonOffsetPosition = new Vector3(0, 50, 0);

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.GetComponent<PlayerController>() )
        {
            if(other.transform.position.z > transform.position.z && GameManager.Instance.overWorld.transform.position.y > -4)
            {
                other.GetComponent<PlayerController>().TeleportToDungeonLayer(true);
            }
            else if(other.transform.position.z < transform.position.z && GameManager.Instance.overWorld.transform.position.y < -4)
            {
                other.GetComponent<PlayerController>().TeleportToDungeonLayer(false);
            }
        }
    }
}
