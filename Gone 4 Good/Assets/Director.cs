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

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            enabled = false;
            return;
        }
        InvokeRepeating("SpawnEnemies", 15, 15);
        flowLine = GameObject.FindFirstObjectByType<SplineContainer>();
        InvokeRepeating("LevelProgression", 1, 1);
    }

    public void SpawnEnemies()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 spawnPos = GetElegibleSpawnPos();
            SpawnEnemyRpc(0,spawnPos);
        }
    }

    public void LevelProgression()
    {
        // Get all players
        GameObject players = NetworkGameManager.GetRandomPlayer();
        // Find the player that is closesed to the flowline
        float test = SplineUtility.GetNearestPoint(flowLine.Spline, players.transform.position,out float3 newPos,out float t,4,2);
        print($"t: {t} splinePos: {newPos}");

    }

    private Vector3 GetElegibleSpawnPos()
    {   
        // Get random player 
        GameObject player = NetworkGameManager.GetRandomPlayer();
        NavMeshHit hit;
        Vector3 spawnPos;
        int samples = 0;
        while (samples < 15)
        {
            Vector2 rndCircle = UnityEngine.Random.insideUnitCircle * 30;
            spawnPos = player.transform.position + new Vector3(rndCircle.x, 0, rndCircle.y);
            if(NavMesh.SamplePosition(spawnPos, out hit, 1.0f, NavMesh.AllAreas))
            {
                if (!NavMesh.CalculatePath(hit.position,player.transform.position , NavMesh.AllAreas, new NavMeshPath())) continue;
                if(Vector3.Distance(player.transform.position, hit.position) > 15) return hit.position;
            }
            samples++;
        }


        return Vector3.zero;
    }
    [Rpc(SendTo.Server)]
    private void SpawnEnemyRpc(int index,Vector3 position)
    {
        if(position == Vector3.zero) return;
        GameObject enemyInstance = Instantiate(zombie, position, Quaternion.identity);
        enemyInstance.GetComponent<NetworkObject>().Spawn();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
