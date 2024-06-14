using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Toggle))]
public class ToggleSelection : MonoBehaviour
{
    public UnityEvent onToggleOn;
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
        GetComponent<Toggle>().onValueChanged.AddListener(ToggleOn);
    }

    private void OnEnable()
    {
        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }
        anim.SetBool("IsOn", GetComponent<Toggle>().isOn);
    }

    public virtual void ToggleOn(bool isOn)
    {
        if(isOn)
        {
            onToggleOn.Invoke();
        }
        anim.SetBool("IsOn", isOn);

    }
}
