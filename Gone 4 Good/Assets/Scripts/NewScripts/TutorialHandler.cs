using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class TutorialHandler : NetworkBehaviour
{
    G4GNetworkManager networkManager;
    public GameObject zombiePrefab;
    public GameObject realZombiePrefab;

    [Header("Hostage")]
    public Transform[] spawnPoints;
    public StatusManager hostage;
    private bool hostageSituationInProgress = false;
    public Animation doorAnimation;
    public List<float> enemyLifeTimes = new List<float>();

    [Header("Platforming")]
    public Transform[] platformingSpawnPoints;

    [Header("Reaction Test")]
    public Transform reactionTextSpawnPoint;

    [Header("Survival Fight")]
    public Transform[] survivalFightSpawnPoints;
    public Animation tutorialEndAnimation;
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
            SpawnEnemy(0, platformingSpawnPoints[Random.Range(0, platformingSpawnPoints.Length)].position, 1);
            yield return new WaitForSeconds(5);
        }
    }

    public void StartReactionTest()
    {
        GameObject enemy = SpawnEnemy(0, reactionTextSpawnPoint.position, 1);
        //enemy.GetComponent<StatusManager>().OnDeath.AddListener(StopReactionTest);
        StartCoroutine(TrackPlayerTurnaroundTime());
    }

    IEnumerator TrackPlayerTurnaroundTime()
    {
        FPSController player = FindObjectOfType<FPSController>();
        float startTime = Time.time;
        float endTime = 0;
        while (true)
        {
            // Check if rotation is between 45 and -45 degrees
            if (player.transform.eulerAngles.y < 45 && player.transform.eulerAngles.y > -45)
            {
                endTime = Time.time;
                StopReactionTest(endTime - startTime);
                print("Player turned around in " + (endTime - startTime) + " seconds");
                break;
            }
            yield return null;
        }
    }

    public void StopReactionTest(float turnaroundTime)
    {
        PerformanceTracker.StartNewStack("ReactionTest", FindObjectOfType<FPSController>().playerName.Value.ToString(),"Measure the the time it takes for the Player to turn around and face an incoming threat. Only Elapsed time is Relevant here.");
        PerformanceTracker.instance.currentStack.timeElapsed = turnaroundTime;
        PerformanceTracker.instance.performanceStacks.Add(PerformanceTracker.instance.currentStack);
        PerformanceTracker.instance.currentStack = new PerformanceStack();
        PerformanceTracker.instance.SaveStackToFile();
        print("ReactionTestConcluded");
    }

    IEnumerator HostageSituation()
    {
        hostageSituationInProgress = true;
        PerformanceTracker.StartNewStack("HostageSituation", FindObjectOfType<FPSController>().playerName.Value.ToString(), "The Player has to defend a hostage from Incoming Enemies. Measured is mainly the players accuary. The custom value is the averageEnemyLifetime in this PerformanceTracker instance");
        float startTime = Time.time;
        while(startTime + 30 > Time.time)
        {
            GameObject enemy1 = SpawnEnemy(0, spawnPoints[Random.Range(0, spawnPoints.Length)].position, 2);
            GameObject enemy2 = SpawnEnemy(0, spawnPoints[Random.Range(0, spawnPoints.Length)].position, 2);
            enemy1.GetComponent<StatusManager>().OnDeath.AddListener(() => AddEnemyLifeTime(enemy1.GetComponent<ZombieAI>()));
            enemy2.GetComponent<StatusManager>().OnDeath.AddListener(() => AddEnemyLifeTime(enemy2.GetComponent<ZombieAI>()));
            yield return new WaitForSeconds(Random.Range(2,3f));
        }
        yield return new WaitForSeconds(4);
        hostageSituationInProgress = false;
        doorAnimation.Play();
        float averageEnemyLifeTime;
        if(enemyLifeTimes.Count > 0)
        {
            // Calculate average enemy life time
            float sum = 0;
            foreach(float time in enemyLifeTimes)
            {
                sum += time;
            }
            averageEnemyLifeTime = sum / enemyLifeTimes.Count;
        }
        else
        {
            averageEnemyLifeTime = 0;
        }
        PerformanceTracker.instance.currentStack.custom = averageEnemyLifeTime;
        PerformanceTracker.EndCurrentStack();
    }

    public void AddEnemyLifeTime(ZombieAI zombie)
    {
        enemyLifeTimes.Add(Time.time - zombie.spawnTime);
    }

    public void StartSurvivalFight()
    {
        StartCoroutine(SurvivalFight());
        FPSController player = FindObjectOfType<FPSController>();
        player.GetComponent<StatusManager>().OnDeath.AddListener(EndSurvivalFight);
        AudioManager.instance.PlayMusicRpc(0);
    }

    public void EndSurvivalFight()
    {
        PerformanceTracker.EndCurrentStack();
        tutorialEndAnimation.Play();
        GameUI.instance.forceMouseVisible = true;
    }

    public void BackToMainMenu()
    {
        // Close Server
        networkManager.Shutdown();
        Destroy(networkManager.gameObject);
        SceneManager.LoadScene("MainMenu");
    }


    IEnumerator SurvivalFight()
    {
        PerformanceTracker.StartNewStack("SurvivalFight", FindObjectOfType<FPSController>().playerName.Value.ToString(),"The Player has to survive as long as possible in an arena.");
        float startTime = Time.time;
        float difficultyIncreaseTimer = Time.time + 10;
        int spawnAmount = 3;
        while(startTime + 3600 > Time.time)
        {
            for(int i = 0; i < spawnAmount; i++)
            {
                SpawnEnemy(1, survivalFightSpawnPoints[Random.Range(0, survivalFightSpawnPoints.Length)].position, 1);
            }
            yield return new WaitForSeconds(Random.Range(3,5));
            if(difficultyIncreaseTimer < Time.time)
            {
                spawnAmount++;
                difficultyIncreaseTimer = Time.time + 13;
            }

        }
        PerformanceTracker.EndCurrentStack();
    }

    private GameObject SpawnEnemy(int index, Vector3 position, int aggroed)
    {
        GameObject _zombiePrefab = zombiePrefab;
        if (index == 1)
        {
            _zombiePrefab = realZombiePrefab;
        }
        if (position == Vector3.zero || ZombieAI.zombies.Count >= 120) return null;
        GameObject enemyInstance = Instantiate(_zombiePrefab, position, Quaternion.identity);
        float scale = UnityEngine.Random.Range(0.8f, 1.2f);
        enemyInstance.transform.localScale = new Vector3(scale, scale, scale);
        if (aggroed == 1)
        {
            enemyInstance.GetComponent<ZombieAI>().target = NetworkGameManager.GetRandomPlayer();
        }
        if(aggroed == 2)
        {
            enemyInstance.GetComponent<ZombieAI>().target = hostage.gameObject;
        }
        enemyInstance.GetComponent<NetworkObject>().Spawn();
        return enemyInstance;
    }
}
