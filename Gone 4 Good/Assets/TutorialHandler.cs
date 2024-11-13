using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class TutorialHandler : NetworkBehaviour
{
    G4GNetworkManager networkManager;
    public GameObject zombiePrefab;

    [Header("Hostage")]
    public Transform[] spawnPoints;
    public StatusManager hostage;
    private bool hostageSituationInProgress = false;
    public Animation doorAnimation;

    [Header("Platforming")]
    public Transform[] platformingSpawnPoints;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        networkManager = FindObjectOfType<G4GNetworkManager>();
        networkManager.StartLanGame();
    }

    public void StartHostageSituation()
    {
        if(hostageSituationInProgress) return;
        StartCoroutine(HostageSituation());
    }

    public void StartPlatforming()
    {
        StartCoroutine(Platforming());
    }

    IEnumerator Platforming()
    {
        float startTime = Time.time;
        while(startTime + 30 > Time.time)
        {
            SpawnEnemyRpc(0, platformingSpawnPoints[Random.Range(0, platformingSpawnPoints.Length)].position, 1);
            yield return new WaitForSeconds(5);
        }
    }

    IEnumerator HostageSituation()
    {
        hostageSituationInProgress = true;
        PerformanceTracker.StartNewStack("HostageSituation", FindObjectOfType<FPSController>().playerName.Value.ToString());
        float startTime = Time.time;
        while(startTime + 30 > Time.time)
        {
            SpawnEnemyRpc(0, spawnPoints[Random.Range(0, spawnPoints.Length)].position, 2);
            SpawnEnemyRpc(0, spawnPoints[Random.Range(0, spawnPoints.Length)].position, 2);
            yield return new WaitForSeconds(Random.Range(2,3f));
        }
        yield return new WaitForSeconds(4);
        hostageSituationInProgress = false;
        doorAnimation.Play();
        PerformanceTracker.EndCurrentStack();
    }

    [Rpc(SendTo.Server)]
    private void SpawnEnemyRpc(int index, Vector3 position, int aggroed)
    {
        if (position == Vector3.zero || ZombieAI.zombies.Count >= 120) return;
        GameObject enemyInstance = Instantiate(zombiePrefab, position, Quaternion.identity);
        if (aggroed == 1)
        {
            enemyInstance.GetComponent<ZombieAI>().target = NetworkGameManager.GetRandomPlayer();
        }
        if(aggroed == 2)
        {
            enemyInstance.GetComponent<ZombieAI>().target = hostage.gameObject;
        }
        enemyInstance.GetComponent<NetworkObject>().Spawn();
    }
}
