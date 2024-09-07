using System.Collections;
using UnityEngine;


public class BlasterAI : ZombieAI
{
    public override void OnNetworkSpawn()
    {
        if (!IsServer)
        {
            enabled = false;
            return;
        }
        statusManager = GetComponent<StatusManager>();
        enemyStates = new State[4];
        enemyStates[0] = new IdleState();
        enemyStates[1] = new ChaseState();
        enemyStates[2] = new WanderState();
        enemyStates[(int)currentState].OnEnter(this);
        animator.SetFloat("RunAnimation", Random.Range(0, 3));
    }
}
