using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class EndingSequenceLevel1 : NetworkBehaviour
{
    public Animation truck;
    public Transform[] spawnPoints;
    private Director director;
    public int spawnAmount = 16;
    public int spawnInterval = 5;
    public GameObject preperationCanvas;

    public void DisableDirector()
    {
        director = FindObjectOfType<Director>();
        director.enabled = false;
    }

    public void TriggerEvent()
    {
        MoveTruckRpc();
        director = FindObjectOfType<Director>();
        AudioManager.instance.PlayMusicRpc(0);

        StartCoroutine(SpawnBehaviour());
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void MoveTruckRpc()
    {
        truck.Play();
        preperationCanvas.SetActive(false);
    }

    IEnumerator SpawnBehaviour()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            for(int i = 0; i < spawnAmount; i++)
            {
                int randomIndex = Random.Range(0, spawnPoints.Length);
                director.SpawnEnemyRpc(0, spawnPoints[randomIndex].position, true);
            }

        }
    }

    public void StopEvent()
    {
        StopCoroutine(SpawnBehaviour());
    }
}
