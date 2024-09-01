using UnityEngine;

public class DebugBoxCollider : MonoBehaviour
{
    void OnDrawGizmos()
    {
        Gizmos.color = new Color32(255,0,0,100);
        Gizmos.DrawCube(transform.position, transform.localScale);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}

