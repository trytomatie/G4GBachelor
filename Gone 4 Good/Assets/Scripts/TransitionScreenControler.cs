using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class TransitionScreenControler : MonoBehaviour
{
    public Material transitionMaterial;
    public float tranistionValue;
    public AnimationCurve transitionCurve;
    public bool isTransitioning = false;

    // singleton
    public static TransitionScreenControler instance;
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if(tranistionValue != 0 || tranistionValue != 1)
        {
            transitionMaterial.SetFloat("_Opacity", transitionCurve.Evaluate(tranistionValue));
        }

    }


    public void CallTransition()
    { 
        StartCoroutine(TransitionPositive());
    }

    public void DismissTransition()
    {
        StartCoroutine(TransitionNegative());
    }

    IEnumerator TransitionPositive()
    {
        isTransitioning = true;
        float t = 0;
        while(t < 0.5f)
        {
            t += Time.unscaledDeltaTime;
            tranistionValue = t;
            yield return null;
        }
    }

    IEnumerator TransitionNegative()
    {
        float t = 1;
        while(t > 0)
        {
            t -= Time.unscaledDeltaTime;
            tranistionValue = t;
            yield return null;
        }
        isTransitioning = false;
    }
}

#if UNITY_EDITOR
// Editor Script
public class TransitionScreenControlerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        TransitionScreenControler tsc = (TransitionScreenControler)target;
        if(GUILayout.Button("Call Transition"))
        {
            tsc.CallTransition();
        }
        if(GUILayout.Button("Dismiss Transition"))
        {
            tsc.DismissTransition();
        }
    }
}
#endif