using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(StatusManager))]
[RequireComponent(typeof(NavMeshAgent))]

public class ZombieAI : NetworkBehaviour
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

    [Header("Attack")]
    public Collider hitbox;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        if(!IsServer)
        {
            enabled = false;
            return;
        }
        statusManager = GetComponent<StatusManager>();
        enemyStates = new State[4];
        enemyStates[0] = new IdleState();
        enemyStates[1] = new ChaseState();
        currentState = enemyStates[0];
        currentState.OnEnter(this);
        animator.SetFloat("RunAnimation", Random.Range(0, 3));
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsSpawned) return;
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
        pos += new Vector3(Random.Range(-2, 2), 0, Random.Range(-2, 2));
        agent.SetDestination(pos);
    }

    public void Attack(float delay)
    {
        Invoke("InvokeAttack", delay);
    }

    private void InvokeAttack()
    {
        hitbox.gameObject.SetActive(true);
    }

    public void Ragdoll()
    {
        animator.enabled = false;
        agent.enabled = false;
        Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>();
        foreach(Rigidbody rb in rbs)
        {
            rb.isKinematic = false;
        }
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
    private float attackTimer = 0;
    private float attackCooldown = 1.5f;

    public float[] attackDelay = new float[3] { 0.4f, 0.4f, 0.55f };
    public void OnEnter(ZombieAI pc)
    {
        pc.LookForClosestTarget();
    }

    public void OnUpdate(ZombieAI pc)
    {
        if (attackTimer + attackCooldown < Time.time)
        {
            if(pc.target == null)
            {
                pc.SwitchState(pc.enemyStates[0]);
                return;
            }
            pc.PathfindToDestination(pc.target.transform.position);
            pc.Animation();
            if (Vector3.Distance(pc.transform.position, pc.target.transform.position) < 1.5f)
            {
                int rnd = Random.Range(0, 3);
                Debug.Log("Attack");
                pc.Attack(attackDelay[rnd]);
                attackTimer = Time.time;
                pc.animator.SetTrigger("Attack");
                pc.animator.SetInteger("AttackAnimation", rnd);
                pc.PathfindToDestination(pc .transform.position);
            }

        }

    }

    public void OnExit(ZombieAI pc)
    {

    }
}
