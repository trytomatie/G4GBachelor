using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CheckForPlayersTrigger : NetworkBehaviour
{
    private List<FPSController> playersInTrigger = new List<FPSController>();
    private bool triggered = false;
    public CanvasGroup endCanvas;
    private void Start()
    {
        
    }
    private void Update()
    {
        if (NetworkGameManager.Instance.connectedClients.Values.Count == 0) return;
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
        // force show cursor
        GameUI.instance.forceMouseVisible = true;
        PerformanceTracker.EndCurrentStack("SessionFiles");
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

    public void OpenFormAndExit()
    {
        NetworkManager.Singleton.Shutdown();
        Application.Quit();

        // Open File Location
        string path = Application.persistentDataPath + "/SessionFiles";
        path = path.Replace(@"/", @"\");
        if (System.IO.Directory.Exists(path)) // Check if the folder exists
        {
            System.Diagnostics.Process.Start("explorer.exe", "/select," + path);
        }
        else
        {
            print("The specified folder does not exist.");
        }
    }


}