using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int gameTicks;
    public float gameTickRate = 1;
    public delegate void GameTick();
    public static event GameTick OnGameTick;
    public GameObject player;

    [Header("Prefabs")]
    public GameObject droppedItem;
    public GameObject[] grassPrefabs;
    public GameObject resourceBlock;

    [Header("Grid")]
    public Dictionary<Vector2Int, GameObject> grid = new Dictionary<Vector2Int, GameObject>();
    public LayerMask groundLayer;
    public Material groundMaterial;

    [Header("GameWorld")]
    public GameObject overWorld;
    public GameObject dungeonWorld;

    [Header("Hitboxes")]
    public GameObject[] hitboxes;

    [Header("Modifiers")]
    public float generalItemProductionMultiplier = 1;
    public bool globalStashUnlocked = false;
    public int lootDropAmount = 1;
    // Singleton
    private static GameManager instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
        Instance.player = GameObject.FindObjectOfType<PlayerController>().gameObject;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GameClock());
        // CheckGridInARadius(Vector3.zero, 360); Removed for the Raiding Game
    }

    private IEnumerator GameClock()
    {
        while (true)
        {
            yield return new WaitForSeconds(gameTickRate);
            try
            {
                if(OnGameTick != null)
                {
                    OnGameTick.Invoke();
                }

            }
            catch(Exception e)
            {
                Debug.LogException(e, this);
            }
            gameTicks++;
        }
    }

    public void CheckGridInARadius(Vector3 pos, float radius)
    {
        for (int x = Mathf.RoundToInt(pos.x - radius); x < Mathf.RoundToInt(pos.x + radius); x++)
        {
            for (int z = Mathf.RoundToInt(pos.z - radius); z < Mathf.RoundToInt(pos.z + radius); z++)
            {
                CheckGridAt(new Vector3(x, 0, z));
            }
        }
    }

    public void CheckGridAt(Vector3 pos)
    {
        Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
        bool assigned = false;
        if(grid.ContainsKey(gridPos))
        {
            assigned = true;
        }
        else
        {
            grid.Add(gridPos, null);
        }
        pos.y = 10;
        Vector3 posRounded = new Vector3(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
        RaycastHit hit;
        if (Physics.Raycast(pos + new Vector3(0.5f, 0, -0.5f), Vector3.down, out hit, 100, groundLayer))
        {

            if (hit.collider.gameObject.GetComponent<Renderer>().materials.Any(e => e.name.Contains(groundMaterial.name)))
            {
                print(assigned);
                if (!assigned)
                {
                    grid[gridPos] = Instantiate(resourceBlock, hit.point, Quaternion.identity);
                    grid[gridPos].transform.parent = hit.collider.transform.root;
                }
            }
            else
            {
                if(assigned)
                {
                    Destroy(grid[gridPos]);
                    grid.Remove(gridPos);
                }
            }
        }
    }

    public void SpawnItem(ItemBlueprint item,int amount, Vector3 pos)
    {
        GameObject itemObj = Instantiate(droppedItem, pos, Quaternion.identity);
        itemObj.GetComponent<DroppedItem>().SetUpDroppedItem(item, amount);
    }

    public GameObject SpawnHitbox(Transform character, int i)
    {
        GameObject hitbox = Instantiate(hitboxes[i], character.position + hitboxes[i].transform.localPosition, character.rotation);
        hitbox.SetActive(true);
        return hitbox;
    }

    public void SpawnFlowersDelayed(List<GameObject> flowers,int amountPerFrame,float delay)
    {
        StartCoroutine(SpawnFlowersDelayedCoroutine(flowers, amountPerFrame,delay));
    }

    private IEnumerator SpawnFlowersDelayedCoroutine(List<GameObject> flowers, int amountPerFrame,float delay)
    {
        yield return new WaitForSeconds(delay);
        int spawned = 0;
        foreach (var flower in flowers)
        {
            if(spawned == amountPerFrame)
            {
                spawned = 0;
                yield return new WaitForFixedUpdate();
            }
            flower.SetActive(true);
            StartCoroutine(RegulateFlowerSpawnWind(flower.GetComponentInChildren<Renderer>()));
            spawned++;
        }
    }
    public AnimationCurve flowerSpawnWindCurve;
    private IEnumerator RegulateFlowerSpawnWind(Renderer flower)
    {
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        float time = 0;
        propertyBlock.SetFloat("_SpeedScale", 1);
        while (time < 5)
        {
            time += Time.deltaTime;
            propertyBlock.SetFloat("_SpeedScale", flowerSpawnWindCurve.Evaluate(time/5) * 30);
            flower.SetPropertyBlock(propertyBlock);
            yield return new WaitForEndOfFrame();
        }
    }

    public void StopParticleSystemAfterTime(ParticleSystem particleSystem, float time)
    {
        StartCoroutine(StopParticleSystemAfterTimeCoroutine(particleSystem, time));
    }

    private IEnumerator StopParticleSystemAfterTimeCoroutine(ParticleSystem particleSystem, float time)
    {
        yield return new WaitForSeconds(time);
        particleSystem.Stop();
    }


    public static GameManager Instance { get 
        { 
            return instance; 
        } 
        set => instance = value; }
}
