using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class Director : NetworkBehaviour
{
    public GameObject zombie;
    public Vector3[] navMeshVertices;
    public enum DirectorState
    {
        BuildUp,
        Peak,
        Relax
    }
    public DirectorState currentState = DirectorState.BuildUp;
    public float buildUpTime = 80;
    public float buildUpTimer = 0;
   
    public float peakTime = 10;
    public float peakTimer = 0;
    public float relaxTime = 20;
    public float relaxTimer = 0;
    public float enemySpawnTimer = 0;
    public float enemySpawnInterval = 6;

    public float zombieSpieedBaseMutliplier = 1;
    public int zombieBaseHealthBonus = 0;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            enabled = false;
            return;
        }
        // get all vertecies of the navmesh
        NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();
        navMeshVertices = navMeshData.vertices;
        InitialSpawn();
    }

    public void InitialSpawn()
    {
        for(int i = 0; i < 200;i++)
        {
            int areaMask = ~(1 << NavMesh.GetAreaFromName("TutorialArea"));
            int randomPosition = UnityEngine.Random.Range(0,navMeshVertices.Length);
            NavMesh.SamplePosition(navMeshVertices[randomPosition], out NavMeshHit hit, 10, areaMask);
            if (CollidesWithWall(ref hit))
            {
                i--;
                continue;
            }
            SpawnEnemyRpc(0, navMeshVertices[randomPosition], false);
        }
        ZombieAI.zombies.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        if(!IsServer) return;
        switch (currentState)
        {
            case DirectorState.BuildUp:
                enemySpawnInterval = 6;
                buildUpTimer += Time.deltaTime;
                if (buildUpTimer > buildUpTime)
                {
                    currentState = DirectorState.Peak;
                    buildUpTimer = 0;
                    AudioManager.instance.PlayMusicRpc(0);
                    enemySpawnInterval = 0;
                }
                EnemySpawning();
                break;
            case DirectorState.Peak:
                peakTimer += Time.deltaTime;
                if (peakTimer > peakTime)
                {
                    currentState = DirectorState.Relax;
                    AudioManager.instance.StopMusicRpc(35);
                    peakTimer = 0;
                }
                enemySpawnInterval = 2f;
                EnemySpawning(22);
                break;
            case DirectorState.Relax:
                enemySpawnInterval = 30;
                relaxTimer += Time.deltaTime;
                if(relaxTimer > relaxTime)
                {
                    currentState = DirectorState.BuildUp;
                    relaxTimer = 0;
                }
                break;
        }
    }

    private void EnemySpawning(int amount = 4)
    {
        enemySpawnTimer += Time.deltaTime;
        if (enemySpawnTimer > enemySpawnInterval)
        {
            List<Vector3> spawnPositions = GetRandomVertNearPlayer(amount);
            foreach (Vector3 spawnPos in spawnPositions)
            {
                if (spawnPos != Vector3.zero)
                {
                    SpawnEnemyRpc(UnityEngine.Random.Range(0, 2), spawnPos, true);
                }
            }
            enemySpawnTimer = 0;
        }
    }

    private List<Vector3> GetRandomVertNearPlayer(int amountOfPositions)
    {
        List<Vector3> positions = new List<Vector3>();
        // Get random player 
        GameObject player = NetworkGameManager.GetRandomPlayer();
        GameObject[] players = NetworkGameManager.GetAllConnectedPlayers();
        Vector3 averagePlayerPosition;
        if(players.Length == 1)
        {
            averagePlayerPosition = player.transform.position;
        }
        else
        {
            averagePlayerPosition = Vector3.zero;
            foreach(GameObject p in players)
            {
                averagePlayerPosition += p.transform.position;
            }
            averagePlayerPosition /= players.Length;
        }
        int areaMask = ~(1 << NavMesh.GetAreaFromName("TutorialArea"));
        int samples = 0;
        Camera[] playerCameras = new Camera[players.Length];
        for(int i = 0; i < players.Length; i++)
        {
            playerCameras[i] = players[i].GetComponent<FPSController>().playerCamera.GetComponent<Camera>();
        }
        // Sample a random position near the players
        while(samples < 5000)
        {
            Vector3 randomPos = averagePlayerPosition + UnityEngine.Random.onUnitSphere * 50 
                + new Vector3(UnityEngine.Random.Range(-10,10),0, UnityEngine.Random.Range(-10, 10));
            NavMeshHit hit;
            if(NavMesh.SamplePosition(randomPos,out hit,10,areaMask) && samples < 5000)
            {
                // check if position collides with wall
                if (CollidesWithWall(ref hit))
                {
                    samples++;
                    continue;
                }
                foreach (Camera c in playerCameras)
                {
                    Vector3 viewPoint = c.WorldToViewportPoint(hit.position + new Vector3(0, 1.5f, 0));
                    if (viewPoint.x > 0 && viewPoint.x < 1 && viewPoint.y > 0 && viewPoint.y < 1)
                    {

                    }
                    else
                    {
                        positions.Add(hit.position);
                        if (positions.Count == amountOfPositions)
                        {
                            print("Samples needed for Spawn " + samples);
                            return positions;
                        }
                    }
                }
            }
            samples++;
        }
        Debug.LogError("Could not find a suitable position");
        return positions;
    }

    private static bool CollidesWithWall(ref NavMeshHit hit)
    {
        return Physics.CheckBox(hit.position + new Vector3(0, 1.5f, 0), new Vector3(0.5f, 0.5f, 0.5f));
    }

    [Rpc(SendTo.Server)]
    public void SpawnEnemyRpc(int index,Vector3 position,bool aggroed)
    {
        if(position == Vector3.zero || ZombieAI.zombies.Count >= 140) return;
        GameObject enemyInstance = Instantiate(zombie, position, Quaternion.identity);
        float scale = UnityEngine.Random.Range(0.8f, 1.2f);
        enemyInstance.transform.localScale = new Vector3(scale, scale, scale);
        if (aggroed)
        {
            GameObject[] allConnectedPlayers = NetworkGameManager.GetAllConnectedPlayers();
            int[] targetingRolls = new int[allConnectedPlayers.Length];
            for(int i =0; i < allConnectedPlayers.Length; i++)
            {
                targetingRolls[i] = Mathf.RoundToInt(UnityEngine.Random.Range(0, 100) * allConnectedPlayers[i].GetComponent<DDAData>().enemyTargetingProbability.Value);
            }
            int targetIndex = 0;
            for(int i = 0; i < targetingRolls.Length; i++)
            {
                if (targetingRolls[i] > targetingRolls[targetIndex])
                {
                    targetIndex = i;
                }
            }
            enemyInstance.GetComponent<ZombieAI>().target = allConnectedPlayers[targetIndex];
        }
        enemyInstance.GetComponent<ZombieAI>().moveSpeed *= zombieSpieedBaseMutliplier;
        enemyInstance.GetComponent<StatusManager>().maxHp += zombieBaseHealthBonus;
        enemyInstance.GetComponent<StatusManager>().Hp.Value += zombieBaseHealthBonus;
        enemyInstance.GetComponent<NetworkObject>().Spawn();
    }


}
