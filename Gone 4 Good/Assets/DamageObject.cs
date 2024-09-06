using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class DamageObject : MonoBehaviour
{
    public float lifeTime = 1f;
    public bool destoyDamageObject = true;
    public StatusManager.Faction faction = StatusManager.Faction.Neutral;
    private List<StatusManager> hitList = new List<StatusManager>();

    private void OnEnable()
    {
        Invoke("DisableAfterTime", 0.1f);
    }

    private void OnDisable()
    {
        hitList.Clear();
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
        
        if (sm != null && !hitList.Contains(sm))
        {
            if (sm.faction == faction) return;
            sm.ApplyDamageRpc(10,sm.transform.position,20);
            Slowness slowness = new Slowness();
            slowness.duration = 1;
            slowness.slowAmount = 0.15f;
            slowness.ApplyStatusEffect(sm);
            hitList.Add(sm);
        }
    }

}
