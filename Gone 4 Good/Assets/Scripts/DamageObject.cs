using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DamageObject : MonoBehaviour
{
    public float lifeTime = 0.5f;
    public GameObject hitVFXPrefab;
    public UnityEvent HitEvent;
    public StatusManager source;

    private List<GameObject> hitObjects = new List<GameObject>();
    private void OnTriggerEnter(Collider other)
    {
        if (hitObjects.Contains(other.gameObject) || other.gameObject == source.gameObject ) return;

        ResourceStatusManager resourceStatusManager = other.GetComponent<ResourceStatusManager>() ?? null;
        StatusManager statusManager = other.GetComponent<StatusManager>() ?? null;
        if(resourceStatusManager != null)
        {
            HitEvent.Invoke();  
            hitObjects.Add(other.gameObject);
            resourceStatusManager.ApplyDamage(source.AttackDamage);
            Vector3 hitPosition = other.ClosestPointOnBounds(transform.position+ new Vector3(0,0.5f,0));
            GameObject hitVFX = Instantiate(hitVFXPrefab, hitPosition, Quaternion.identity);
            Destroy(hitVFX, 2f);
        }
        else if (statusManager != null&& statusManager.faction != source.faction)
        {
            HitEvent.Invoke();  
            hitObjects.Add(other.gameObject);
            statusManager.ApplyDamage(source.AttackDamage);
            Vector3 hitPosition = other.ClosestPointOnBounds(transform.position+ new Vector3(0,0.5f,0));
            GameObject hitVFX = Instantiate(hitVFXPrefab, hitPosition, Quaternion.identity);
            Destroy(hitVFX, 2f);
        }

    }

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

}
