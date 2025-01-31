using UnityEngine;

public class VoidOutBox : MonoBehaviour
{


    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.voidOut = true;
        }
    }
    void OnDrawGizmos()
    {
        Gizmos.color = new Color32(255,0,0,100);
        Gizmos.DrawCube(transform.position, transform.localScale);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}

