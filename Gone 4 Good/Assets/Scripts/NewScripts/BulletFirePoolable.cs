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
    public bool InUse 
    { 
        get => inUse; 
        set
        {
            if(value == false)
            {
                distanceUntilImpact = 999;
                gameObject.SetActive(false);
            }
            else
            {
                impactCount = 0;
                trailRenderer.Clear();
                gameObject.SetActive(true);
                StartCoroutine(DissableBullet());
            }
            inUse = value; 
        }
    }

    private void Start()
    {
        trailRenderer = GetComponent<TrailRenderer>();
        trailRenderer.Clear();

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
