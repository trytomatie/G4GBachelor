using UnityEngine;
using UnityEngine.Events;

public class TutorialTrigger : MonoBehaviour
{
    public UnityEvent onTriggerEnter;
    public bool triggered = false;
    public void OnTriggerEnter(Collider other)
    {
        if(triggered) return;
        if(other.GetComponent<FPSController>() == null) return;
        onTriggerEnter.Invoke();
        triggered = true;
    }
}
