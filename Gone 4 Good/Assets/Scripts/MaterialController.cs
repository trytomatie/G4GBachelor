using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialController : MonoBehaviour
{
    public Material[] materials;
    public LayerMask layerMask;
    private bool hideMaterial;
    private bool finisheAnimation;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        SetMaterialOpacity();
    }

    public void SetMaterialOpacity()
    {
        // Raycast from camera to player
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 6, layerMask))
        {
            HideMaterial = true;
        }
        else
        {
            HideMaterial = false;
        }
        if(!HideMaterial && !finisheAnimation)
        {
            foreach (Material m in materials)
            {
                m.SetFloat("_Alpha",Mathf.Clamp01(m.GetFloat("_Alpha") + Time.deltaTime *2));
                if(m.GetFloat("_Alpha") == 0)
                {
                    finisheAnimation = true;
                }
            }
        }
        else if(!finisheAnimation && HideMaterial)
        {
            foreach (Material m in materials)
            {
                m.SetFloat("_Alpha", Mathf.Clamp01(m.GetFloat("_Alpha") - Time.deltaTime*2));
                if(m.GetFloat("_Alpha") == 1)
                {
                    finisheAnimation = true;
                }
            }
        }
    }

    public bool HideMaterial 
    { 
        get => hideMaterial;
        set
        {
            if(hideMaterial != value) 
            { 
                finisheAnimation = false;
                
            }
            hideMaterial = value;
        } 
    }
}
