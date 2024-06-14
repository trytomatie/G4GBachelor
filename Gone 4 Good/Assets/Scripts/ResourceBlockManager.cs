using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceBlockManager : MonoBehaviour
{
    /*
    public List<ResourceBlock> resourceBlocksInScene = new List<ResourceBlock>();
    public List<ResoruceBlockData> resourceBlockData;
    public List<SpawnDirector> spawnDirectors = new List<SpawnDirector>();
    public DirectorShop directorShop;
    public int spawnCredits = 0;

    private int spawnIntervall = 10;

    // Singleton
    public static ResourceBlockManager Instance { get; private set; }
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    public void AddResourceBlockToSceneList(ResourceBlock resourceBlock)
    {
        resourceBlocksInScene.Add(resourceBlock);
    }

    private void OnEnable()
    {
        GameManager.OnGameTick += CheckDirectorShop;
    }

    private void OnDisable()
    {
        GameManager.OnGameTick -= CheckDirectorShop;
    }

    private void CheckDirectorShop()
    {
        // Get the spawn director the player is in
        if (GameManager.Instance.gameTicks % 1 == 0)
        {
            spawnCredits++;
        }
        if (GameManager.Instance.gameTicks % 20 != 0)
        {
            return;
        }
        SpawnDirector spawnDirector = GetSpawnDirector();
        int shopIndex = 0;
        directorShop.CheckForRestock();
        while (shopIndex != -1)
        {
            shopIndex = directorShop.GetRandomAffordableShopIndex(spawnCredits);
            if (shopIndex != -1)
            {
                ResourceBlock resourceBlock = spawnDirector.blocks[Random.Range(0, spawnDirector.blocks.Count)];
                ResoruceBlockData blockData = directorShop.shopItems[shopIndex].resourceBlockData;
                spawnDirector.SpawnResource(resourceBlock, blockData);
                directorShop.shopItems[shopIndex].amount--;
                spawnCredits -= directorShop.shopItems[shopIndex].cost;
                
            }
        } 
    }

    private SpawnDirector GetSpawnDirector()
    {
        SpawnDirector spawnDirector = null;
        foreach (SpawnDirector director in spawnDirectors)
        {
            if (director.boxCollider.bounds.Contains(GameManager.Instance.player.transform.position))
            {
                spawnDirector = director;
                break;
            }
        }
        if (spawnDirector == null)
        {
            // Get the closest spawn director
            float closestDistance = float.MaxValue;
            foreach (SpawnDirector director in spawnDirectors)
            {
                float distance = Vector3.Distance(GameManager.Instance.player.transform.position, director.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    spawnDirector = director;
                }
            }
        }

        return spawnDirector;
    }
    */
}
