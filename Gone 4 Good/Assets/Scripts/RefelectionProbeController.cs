using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ReflectionProbe))]
public class RefelectionProbeController : MonoBehaviour
{
    private ReflectionProbe reflectionProbe;
    // Start is called before the first frame update
    void Start()
    {
        reflectionProbe = GetComponent<ReflectionProbe>();
        reflectionProbe.RenderProbe();
        InvokeRepeating("UpdateReflectionProbe", 0,5f);
    }


    public void UpdateReflectionProbe()
    {
        reflectionProbe.RenderProbe();
    }
}
