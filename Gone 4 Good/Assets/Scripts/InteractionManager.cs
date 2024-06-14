using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    public List<Interactable> interactablesInRange = new List<Interactable>();
    public FollowGameObjectUI interactionToolTip;
    private void OnTriggerEnter(Collider other)
    {
        print("OnTriggerEnter" + other.name);
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
            interactionToolTip.gameObject.SetActive(true);
            interactionToolTip.objectToFollow = interactablesInRange.OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).First().gameObject;
        }
        else
        {
            interactionToolTip.gameObject.SetActive(false);
        }
    }

    public void Interact()
    {
        if (interactablesInRange.Count > 0)
        {
            Interactable interactable = interactablesInRange.OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).First();
            interactable.Interact();
        }
    }
}
