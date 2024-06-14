using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXController : MonoBehaviour
{
    public GameObject[] attackVFX;
    public GameObject attackHitBox;
    public GameObject dashEffect;
    public void HandleVFX(int i)
    {
        GameObject go = Instantiate(attackVFX[i], attackVFX[i].transform.position, attackVFX[i].transform.rotation);
        go.SetActive(true);
        Destroy(go, 2f);

        GameObject go2 = Instantiate(attackHitBox, attackHitBox.transform.position, attackHitBox.transform.rotation);
        go2.SetActive(true);    
    }
}
