using UnityEngine;

public class DamageObject : MonoBehaviour
{
    public float lifeTime = 1f;
    public bool destoyDamageObject = true;
    public StatusManager.Faction faction = StatusManager.Faction.Neutral;
    private void OnEnable()
    {
        Invoke("DisableAfterTime", 1f);
    }

    private void DisableAfterTime()
    {
        gameObject.SetActive(false);
        if (destoyDamageObject)
        {
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        StatusManager sm = other.GetComponent<StatusManager>() ?? null;
        
        if (sm != null)
        {
            if (sm.faction == faction) return;
            sm.ApplyDamageRpc(10,sm.transform.position);
        }
    }

}
