using System.Collections;
using UnityEngine;


[CreateAssetMenu(fileName = "StatsScaling", menuName = "ScriptableObjects/StatsScaling", order = 1)]
public class StatsScaling : ScriptableObject
{
    public int hpGrowth;
    public int attackGrowth;
    public int expGrowth;
}
