using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(StatusManager))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]

public class ZombieAI : MonoBehaviour
{
    public StatusManager statusManager;
    public Animator animator;
    public NavMeshAgent agent;

    public State currentState;
    public State[] enemyStates;

    public GameObject target;

    [Header("Movement")]
    public float moveSpeed = 1;

    [Header("Senses")]
    public Transform eyes;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        statusManager = GetComponent<StatusManager>();
        animator = GetComponent<Animator>();
        enemyStates = new State[4];
        enemyStates[0] = new IdleState();
        enemyStates[1] = new ChaseState();
        currentState = enemyStates[0];
        currentState.OnEnter(this);

    }

    // Update is called once per frame
    void Update()
    {
        currentState.OnUpdate(this);
        Animation();
    }

    public void SwitchState(State newState)
    {
        currentState.OnExit(this);
        currentState = newState;
        currentState.OnEnter(this);
    }

    public bool LookForClosestTarget()
    {
        List<NetworkObject> players = NetworkGameManager.Instance.connectedClients.Values.ToList();
        if (players.Count == 0)
        {
            return false;
        }
        float closestDistance = Mathf.Infinity;
        foreach (NetworkObject player in players)
        {
            // Skip players that are dead
            if (player.GetComponent<StatusManager>().Hp.Value <= 0)
            {
                continue;
            }
            float distance = Vector3.Distance(player.transform.position, transform.position);
            if (distance < closestDistance)
            {
                target = player.gameObject;
                closestDistance = distance;
            }
        }
        return true;
    }

    public void Animation()
    {
        animator.SetFloat("Speed", agent.velocity.magnitude);
    }

    public void PathfindToDestination(Vector3 pos)
    {
        agent.SetDestination(pos);
    }
}

public enum EnemyState
{
    Idle,
    Chase,
    Attack,
    Dead
}
public interface State
{
    void OnEnter(ZombieAI pc);
    void OnExit(ZombieAI pc);
    void OnUpdate(ZombieAI pc);
}
public class IdleState : State
{
    public void OnEnter(ZombieAI pc)
    {

    }

    public void OnUpdate(ZombieAI pc)
    {
        if(pc.LookForClosestTarget())
        {
            pc.SwitchState(pc.enemyStates[1]);
        }
    }

    public void OnExit(ZombieAI pc)
    {

    }
}

public class ChaseState : State
{
    public void OnEnter(ZombieAI pc)
    {
        pc.LookForClosestTarget();
    }

    public void OnUpdate(ZombieAI pc)
    {
        pc.PathfindToDestination(pc.target.transform.position);
        pc.Animation();
    }

    public void OnExit(ZombieAI pc)
    {

    }
}
