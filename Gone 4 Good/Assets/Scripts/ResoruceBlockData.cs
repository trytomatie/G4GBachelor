using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(fileName = "ResoruceBlockData", menuName = "ScriptableObjects/ResoruceBlockData", order = 1)]
public class ResoruceBlockData : ScriptableObject
{
    public int id = -1;
    public int spawnWeight;
    public int guarenteedSpawnPerMinute = 1;
    public GameObject resourceBlockPrefab;


}