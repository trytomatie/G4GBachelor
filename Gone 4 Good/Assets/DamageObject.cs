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
        DDAData data = other.GetComponent<DDAData>() ?? null;
        if (sm != null && !hitList.Contains(sm))
        {
            int dmg = 3;
            if (data != null)
            {

                for (int i = 0; i < 3; i++)
                {
                    dmg += Random.Range(0f, 1f) < data.damgeReceivedBias.Value ? 1 : 0;
                }
            }
            else
            {
                dmg = Random.Range(1,6);
                Debug.LogWarning("No DDAData found on " + other.name);
            }

            if (sm.faction == faction) return;
            sm.ApplyDamageRpc(dmg,sm.transform.position,20);
            Slowness slowness = new Slowness();
            slowness.duration = 0.1f;
            slowness.slowAmount = 0.35f;
            slowness.ApplyStatusEffect(sm);
            hitList.Add(sm);
        }
    }

}
