using System.Collections;
using UnityEngine;


public class BulletFirePoolable : MonoBehaviour
{
    private bool inUse = false;
    private TrailRenderer trailRenderer;
    public float speed = 100;
    private bool isNetworkBullet = false;
    public float distanceUntilImpact = 999;
    public Vector3 impactPosition;
    private int impactCount = 0;
    private bool inPosition = false;
    public bool InUse 
    { 
        get => inUse; 
        set
        {
            if(value == false)
            {
                inUse = false;
                distanceUntilImpact = 999;

            }
            else
            {
                trailRenderer.Clear();
                this.enabled = true;
                impactCount = 0;
                StartCoroutine(DissableBullet());
            }
            inUse = value; 
        }
    }

    private void Start()
    {
        trailRenderer = GetComponent<TrailRenderer>();
        trailRenderer.Clear();
        this.enabled = false;

    }

    private void Update()
    {
        if (InUse)
        {
            distanceUntilImpact -= Time.deltaTime * speed;
            if (distanceUntilImpact > 0)
            {
                transform.position += transform.forward * Time.deltaTime * speed;

            }
            else if(impactCount <1)
            {
                transform.position = impactPosition;
                impactCount++;
                NetworkSpellManager.Instance.ImpactBulletVisual(impactPosition,transform.rotation);
            }


        }
    }

    private IEnumerator DissableBullet()
    {
        yield return new WaitForSeconds(0.4f);
        InUse = false;
    }
    
}
