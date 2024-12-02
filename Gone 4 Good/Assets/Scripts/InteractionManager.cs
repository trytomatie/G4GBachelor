using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public List<Interactable> interactablesInRange = new List<Interactable>();
    public Transform interactionToolTip;
    private void OnTriggerEnter(Collider other)
    {
        Interactable interactable = other.GetComponent<Interactable>();
        if (interactable != null)
        {
            interactablesInRange.Add(interactable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Interactable interactable = other.GetComponent<Interactable>();
        if (interactable != null)
        {
            interactablesInRange.Remove(interactable);
        }
    }

    private void Update()
    {
        if (interactablesInRange.Count > 0)
        {
            GameUI.instance.interactionToolTip.gameObject.SetActive(true);
            //GameUI.instance.interactionToolTip.objectToFollow = interactablesInRange.OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).First().gameObject;
        }
        else
        {
            GameUI.instance.interactionToolTip.gameObject.SetActive(false);
        }
    }

    public void Interact(GameObject source)
    {
        if (interactablesInRange.Count > 0)
        {
            Interactable correctInteractable = null;
            float distance = float.MaxValue;
            foreach(Interactable interactable in interactablesInRange)
            {
                if(interactable != null)
                {
                    if(Vector3.Distance(transform.position,interactable.transform.position) < distance)
                    {
                        correctInteractable = interactable;
                    }
                }
            }
            // Interactable interactable = interactablesInRange.OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).First();
            correctInteractable.Interact(source);
            interactablesInRange.Remove(correctInteractable);
        }
    }
}
