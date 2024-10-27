using Unity.Netcode;
using UnityEngine;

public class DDAData : NetworkBehaviour
{
    [Header("Defensive Parameters")]
    // Weights the randomness favoring lower or higher damage recevied [0...1]
    public NetworkVariable<float> damgeReceivedBias = new NetworkVariable<float>(0.5f);

    // Maximum number of Damage Instances per Second [1...10]
    public NetworkVariable<int> maxDamageInstancesPerSecond = new NetworkVariable<int>(5);

    // Probabilty of beeing Targeted by Enemies [0...1]
    public NetworkVariable<float> enemyTargetingProbability = new NetworkVariable<float>(0.5f);

    // Navmesh Avoidance Radius [0.25...1]
    public NetworkVariable<float> navmeshAvoidanceRadiusMultiplier = new NetworkVariable<float>(0.5f);

    [Header("Offensive Parameters")]
    // The number of hitpoints a Target has to be reduced to be defeated [0...20]
    public NetworkVariable<int> damageOutgoingExecuteTreshold = new NetworkVariable<int>(0);

    // Spherecast Radius for Weapons Calculation [0...1.5]
    public NetworkVariable<float> weaponSpherecastRadius = new NetworkVariable<float>(0.5f);

    
}