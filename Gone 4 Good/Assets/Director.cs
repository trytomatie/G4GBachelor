using Unity.AI.Navigation;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using UnityEngine.Splines;
using Unity.Mathematics;

public class Director : NetworkBehaviour
{
    public GameObject zombie;
    public SplineContainer flowLine;
    public Transform[] spawnPoints;
    public float hordeCounter = 0;
    public float hordeInterval = 50;
    public int spawnedHordes = 0;
    private Vector3 navMeshCenter;
    private float navmeshRadius = 0;

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            enabled = false;
            return;
        }
        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint").Select(x => x.transform).ToArray();
        flowLine = GameObject.FindFirstObjectByType<SplineContainer>();
        navMeshCenter = FindObjectOfType<Mapdata>().center;
        navmeshRadius = FindObjectOfType<Mapdata>().radius;
        InvokeRepeating("LevelProgression", 1, 1);
        SpawnGooners();
        // get all vertecies of the navmesh
        NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();
        Vector3[] navMeshVertices = navMeshData.vertices;
    }


public void SpawnGooners()
    {
        for (int i = 0; i < 100; i++)
        {
            Vector3 spawnPos = GetRandomPositionOnNavMesh();
            SpawnEnemyRpc(0,spawnPos,false);
        }
    }

    public void SpawnHorde()
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 spawnPos = GetHordeEventSpawnpoint();
            SpawnEnemyRpc(0, spawnPos,true);
        }
    }

    
    public void LevelProgression()
    {
        // Get all players
        GameObject players = NetworkGameManager.GetRandomPlayer();
        // Find the player that is closesed to the flowline
        float test = SplineUtility.GetNearestPoint(flowLine.Spline, players.transform.position,out float3 newPos,out float t,4,2);
        float progress = flowLine.CalculateLength(0) * t;
        print($"t: {t} splinePos: {newPos} progress: {progress}m");
        // spawn Horde every 50 progress
        hordeCounter = progress - spawnedHordes * hordeInterval;
        if(hordeCounter > hordeInterval)
        {
            SpawnHorde();
            spawnedHordes++;
        }
    }

    private Vector3 GetRandomPositionNearPlayerOnNavMesh()
    {   
        // Get random player 
        GameObject player = NetworkGameManager.GetRandomPlayer();
        NavMeshHit hit;
        NavMeshHit pHit;
        Vector3 spawnPos;
        int areaMask = ~(1 << NavMesh.GetAreaFromName("TutorialArea"));
        int samples = 0;
        while (samples < 15)
        {
            Vector2 rndCircle = UnityEngine.Random.insideUnitCircle * 30;
            spawnPos = player.transform.position + new Vector3(rndCircle.x, 0, rndCircle.y);
            if(NavMesh.SamplePosition(spawnPos, out hit, 1.0f, areaMask))
            {
                NavMesh.SamplePosition(new Vector3(0,0,0), out pHit, 1.0f, areaMask);
                if (!NavMesh.CalculatePath(hit.position, pHit.position, NavMesh.AllAreas, new NavMeshPath())) continue;
                if(Vector3.Distance(player.transform.position, hit.position) > 15) return hit.position;
            }
            samples++;
        }


        return Vector3.zero;
    }

    private Vector3 GetRandomPositionOnNavMesh()
    {
        NavMeshHit hit;
        Vector3 spawnPos;
        int areaMask = ~(1 << NavMesh.GetAreaFromName("TutorialArea"));
        int samples = 0;
        while (samples < 15)
        {
            Vector2 rndCircle = UnityEngine.Random.insideUnitCircle * navmeshRadius;
            spawnPos = new Vector3(rndCircle.x, 0, rndCircle.y) + navMeshCenter;
            if(NavMesh.SamplePosition(spawnPos, out hit, 21.0f, areaMask))
            {
                NavMesh.SamplePosition(new Vector3(0, 0, 0), out NavMeshHit pHit, 1.0f, areaMask);
                if (!NavMesh.CalculatePath(hit.position, pHit.position, areaMask, new NavMeshPath())) continue;
                return hit.position;
            }
            samples++;
        }
        return Vector3.zero;
    }

    private Vector3 GetHordeEventSpawnpoint()
    {
        // Find spawnpoint that is within 40 meters of the players, but not closer than 15 meters
        GameObject[] players = NetworkGameManager.Instance.connectedClients.Values.Select(x => x.gameObject).ToArray();
        NavMeshHit hit;
        bool validSpawn = true;
        // Shuffle spawnpointlist
        spawnPoints = spawnPoints.OrderBy(x => UnityEngine.Random.value).ToArray();
        foreach(Transform spawnPoint in spawnPoints)
        {
            validSpawn = true;
            foreach (GameObject player in players)
            {
                if(Vector3.Distance(player.transform.position,spawnPoint.position) < 15 || Vector3.Distance(player.transform.position,spawnPoint.position) > 40)
                {
                    validSpawn = false;
                    break;
                }
            }
            if(validSpawn)
            {
                if (NavMesh.SamplePosition(spawnPoint.position + UnityEngine.Random.insideUnitSphere * 2, out hit, 1.0f, NavMesh.AllAreas))
                {
                    return hit.position;
                }
            }
        }
        return Vector3.zero;
    }


    [Rpc(SendTo.Server)]
    private void SpawnEnemyRpc(int index,Vector3 position,bool aggroed)
    {
        if(position == Vector3.zero) return;
        GameObject enemyInstance = Instantiate(zombie, position, Quaternion.identity);
        if(aggroed)
        {
            enemyInstance.GetComponent<ZombieAI>().target = NetworkGameManager.GetRandomPlayer();
        }
        enemyInstance.GetComponent<NetworkObject>().Spawn();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
