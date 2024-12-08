using Unity.Netcode;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class DDAData : NetworkBehaviour
{
    [Header("DDA Parameters")]
    // The Level of Adjustment the DDA System is currently in [1...5]
    public NetworkVariable<int> adjustmentLevel = new NetworkVariable<int>(-1,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);

    [Header("Defensive Parameters")]
    // Weights the randomness favoring lower or higher damage recevied [0...1]
    public NetworkVariable<float> damgeReceivedBias = new NetworkVariable<float>(0.5f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner); // Works

    // Maximum number of Damage Instances per Second [1...10]
    public NetworkVariable<int> maxDamageInstancesPerSecond = new NetworkVariable<int>(5, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner); // Workds

    // Probabilty of beeing Targeted by Enemies [0...1]
    public NetworkVariable<float> enemyTargetingProbability = new NetworkVariable<float>(0.5f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner); // works

    // Navmesh Avoidance Radius [0.5...1]
    public NetworkVariable<float> navmeshAvoidanceRadiusMultiplier = new NetworkVariable<float>(0.5f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner); // works

    // Navmesh Avoidance Radius [0.9...1.2]
    public NetworkVariable<float> zombieAggroedSpeedMultiplier = new NetworkVariable<float>(1f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner); // works

    [Header("Offensive Parameters")]
    // The number of hitpoints a Target has to be reduced to be defeated [0...20]
    public NetworkVariable<int> damageOutgoingExecuteTreshold = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner); // Works

    // Spherecast Radius for Weapons Calculation [0...1.5]
    public NetworkVariable<float> weaponSpherecastRadius = new NetworkVariable<float>(0.5f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner); // Works

    private void OnConnectedToServer()
    {
        if (NetworkManager.Singleton.IsServer && NetworkGameManager.enableDDA)
        {
            SetDDAParametersRpc();
        }
    }

    [Rpc(SendTo.Owner)]
    public void SetDDAParametersRpc()
    {
        adjustmentLevel.Value = PerformanceEvaluationHandler.ddaRating;
        SetDDAParameters(adjustmentLevel.Value);
    }

    public void SetDDAParameters(int i)
    {
        switch (i)
        {
            case 1: // Easiest
                damgeReceivedBias.Value = 0f;
                maxDamageInstancesPerSecond.Value = 2;
                enemyTargetingProbability.Value = 0.25f;
                navmeshAvoidanceRadiusMultiplier.Value = 1.5f;
                damageOutgoingExecuteTreshold.Value = 90;
                weaponSpherecastRadius.Value = 0.15f;
                zombieAggroedSpeedMultiplier.Value = 0.9f;
                break;
            case 2:
                damgeReceivedBias.Value = 0.25f;
                maxDamageInstancesPerSecond.Value = 2;
                enemyTargetingProbability.Value = 0.4f;
                navmeshAvoidanceRadiusMultiplier.Value = 1.2f;
                damageOutgoingExecuteTreshold.Value = 75;
                weaponSpherecastRadius.Value = 0.15f;
                zombieAggroedSpeedMultiplier.Value = 0.95f;
                break;
            case 3:
                damgeReceivedBias.Value = 0.4f;
                maxDamageInstancesPerSecond.Value = 3;
                enemyTargetingProbability.Value = 0.45f;
                navmeshAvoidanceRadiusMultiplier.Value = 1.1f;
                damageOutgoingExecuteTreshold.Value = 60;
                weaponSpherecastRadius.Value = 0.15f;
                zombieAggroedSpeedMultiplier.Value = 1f;
                break;
            case 4: // Normal
                damgeReceivedBias.Value = 0.5f;
                maxDamageInstancesPerSecond.Value = 3;
                enemyTargetingProbability.Value = 0.5f;
                navmeshAvoidanceRadiusMultiplier.Value = 1f;
                damageOutgoingExecuteTreshold.Value = 50;
                weaponSpherecastRadius.Value = 0.1f;
                zombieAggroedSpeedMultiplier.Value = 1f;
                break;
            case 5:
                damgeReceivedBias.Value = 0.6f;
                maxDamageInstancesPerSecond.Value = 4;
                enemyTargetingProbability.Value = 0.55f;
                navmeshAvoidanceRadiusMultiplier.Value = 1f;
                damageOutgoingExecuteTreshold.Value = 50;
                weaponSpherecastRadius.Value = 0.075f;
                zombieAggroedSpeedMultiplier.Value = 1f;
                break;
            case 6: 
                damgeReceivedBias.Value = 0.7f;
                maxDamageInstancesPerSecond.Value = 4;
                enemyTargetingProbability.Value = 0.6f;
                navmeshAvoidanceRadiusMultiplier.Value = 1f;
                damageOutgoingExecuteTreshold.Value = 40;
                weaponSpherecastRadius.Value = 0.05f;
                zombieAggroedSpeedMultiplier.Value = 1.05f;
                break;
            case 7: // hardest
                damgeReceivedBias.Value = 0.8f;
                maxDamageInstancesPerSecond.Value = 5;
                enemyTargetingProbability.Value = 0.8f;
                navmeshAvoidanceRadiusMultiplier.Value = 0.9f;
                damageOutgoingExecuteTreshold.Value = 30;
                weaponSpherecastRadius.Value = 0.0f;
                zombieAggroedSpeedMultiplier.Value = 1.15f;
                break;
        }
    }

    [Rpc(SendTo.Owner)]
    public void IncreaseCurrentDDALevelRpc()
    {
        adjustmentLevel.Value++;
        SetDDAParameters(adjustmentLevel.Value);
    }

    [Rpc(SendTo.Owner)]
    public void DecreaseCurrentDDALevelRpc()
    {
        adjustmentLevel.Value--;
        SetDDAParameters(adjustmentLevel.Value);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(DDAData))]
public class DDADataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        DDAData ddaData = (DDAData)target;
        if (GUILayout.Button("Increase DDA Level"))
        {
            ddaData.IncreaseCurrentDDALevelRpc();
        }
        if (GUILayout.Button("Decrease DDA Level"))
        {
            ddaData.DecreaseCurrentDDALevelRpc();
        }
    }
}
#endif