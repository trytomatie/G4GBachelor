using System.Collections;
using UnityEngine;


public class BulletFirePoolable : MonoBehaviour
{
    private bool inUse = false;

    public bool InUse 
    { 
        get => inUse; 
        set
        {
            if(value == false)
            {
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
                StartCoroutine(DissableBullet());
            }
            inUse = value; 
        }
    }

    private IEnumerator DissableBullet()
    {
        yield return new WaitForSeconds(0.05f);
        InUse = false;
    }
    
}
