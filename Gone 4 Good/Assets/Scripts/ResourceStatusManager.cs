using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ResourceStatusManager : StatusManager
{
    public override void Start()
    {
        base.Start();
        OnDeath.AddListener(() => Destroy(gameObject));
    }
    public virtual void DamageVFX()
    { 
    
    }

}
