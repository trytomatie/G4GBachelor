using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CheckForPlayersTrigger : NetworkBehaviour
{
    private List<FPSController> playersInTrigger = new List<FPSController>();
    private BoxCollider myCollider;
    private bool triggered = false;
    public CanvasGroup endCanvas;
    private void Start()
    {
        if(!IsServer)
        {
            enabled = false;
        }
        myCollider = GetComponent<BoxCollider>();
    }
    private void Update()
    {
        if (playersInTrigger.Count >= NetworkGameManager.Instance.connectedClients.Values.Count && !triggered)
        {
            EndTheGame();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<FPSController>() != null && !playersInTrigger.Contains(other.GetComponent<FPSController>()))
        {
            playersInTrigger.Add(other.GetComponent<FPSController>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<FPSController>() != null && playersInTrigger.Contains(other.GetComponent<FPSController>()))
        {
            playersInTrigger.Remove(other.GetComponent<FPSController>());
        }
    }

    private void EndTheGame()
    {
        triggered = true;
        StartCoroutine(ShowCanvaGroup());
    }

    private IEnumerator ShowCanvaGroup()
    {
        endCanvas.alpha = 0;
        endCanvas.gameObject.SetActive(true);
        while (endCanvas.alpha < 1)
        {
            endCanvas.alpha += Time.deltaTime;
            yield return null;
        }
    }


}