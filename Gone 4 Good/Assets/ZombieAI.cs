using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.AI.Navigation;
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

    public State[] enemyStates;
    public EnemyState currentState;

    public GameObject target;
    private float aggroRange = 12;

    [Header("Movement")]
    public float moveSpeed = 1;
    public float currentMovespeed;
    public float climbSpeed = 0.4f;
    private NavMeshLink currentLink;

    [Header("Senses")]
    public Transform eyes;

    [Header("Attack")]
    public Collider hitbox;
    public float triggerAttack = 0; // if this is greater than 0, spawn the hitbox

    [Header("Debug")]
    public bool debugPathfinding = false;
    public bool hasPath = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnNetworkSpawn()
    {
        if(!IsServer)
        {
            enabled = false;
            return;
        }
        currentMovespeed = moveSpeed;
        statusManager = GetComponent<StatusManager>();
        enemyStates = new State[4];
        enemyStates[0] = new IdleState();
        enemyStates[1] = new ChaseState();
        enemyStates[2] = new WanderState();
        enemyStates[(int)currentState].OnEnter(this);
        animator.SetFloat("RunAnimation", Random.Range(0, 3));
        DeactivateRagdoll();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsSpawned) return;
        enemyStates[(int)currentState].OnUpdate(this);
        Animation();
        agent.speed = currentMovespeed * statusManager.MovementSpeedMultiplier;
        CheckOffMeshLink();
    }

    public void CheckOffMeshLink()
    {
        if(agent.isOnOffMeshLink)
        {
            OffMeshLinkData data = agent.currentOffMeshLinkData;
            // if can be cast to offmeshlink data, skip
            if (data.linkType == OffMeshLinkType.LinkTypeDropDown || data.linkType == OffMeshLinkType.LinkTypeJumpAcross)
            {
                currentMovespeed = moveSpeed / 2;
                animator.SetTrigger("Jumping");
                return;
            }
            currentLink = (NavMeshLink)data.owner;
            if(currentLink == null)
            {
                return;
            }
            currentLink.costModifier = 3000;

            // Rotate towards link 
            agent.updateRotation = false;
            Vector3 direction = (currentLink.endPoint - currentLink.startPoint).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(-direction.x, 0, -direction.z));
            transform.rotation = lookRotation;
            // if link length is greater than 1, use climbing anim, otherwise jump
            if (Vector3.Distance(currentLink.endPoint, currentLink.startPoint) > 1.5f)
            {
                currentMovespeed = climbSpeed;
                animator.SetBool("Climbing",true);
            }
            else
            {
                animator.SetTrigger("Jumping");
            }
        }
        else
        {
            agent.updateRotation = true;
            currentMovespeed = moveSpeed;
            animator.SetBool("Climbing", false);
            if(currentLink != null)
            {
                currentLink.costModifier = -1;
                currentLink = null;
            }
        }
    }

    public void SwitchState(EnemyState newState)
    {
        enemyStates[(int)currentState].OnExit(this);
        currentState = newState;
        enemyStates[(int)currentState].OnEnter(this);
    }

    public void SpawnHitbox(float lingerTime)
    {
        triggerAttack = lingerTime;
    }

    public void RotateTowardsTarget()
    {
        if (target == null)
        {
            return;
        }
        Vector3 direction = (target.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);
    }

    public void RotateTowardsTargetInstant()
    {
        if (target == null)
        {
            return;
        }
        Vector3 direction = (target.transform.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = lookRotation;
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
            if(distance > aggroRange)
            {
                continue;
            }
            if (distance < closestDistance)
            {
                target = player.gameObject;
                closestDistance = distance;
            }
        }
        if(target == null)
        {
            return false;
        }
        return true;
    }
    public void ZombieDeath()
    {
        StartCoroutine(Dissolve());
        StartCoroutine(Despawn());
        Ragdoll();

    }

    private IEnumerator Despawn()
    {
        yield return new WaitForSeconds(13);
        NetworkObject networkObject = GetComponent<NetworkObject>();
        networkObject.Despawn(true);
    }

    private IEnumerator Dissolve()
    {
        yield return new WaitForSeconds(0);
        float timer = 0;
        float dissolveTime = 12;
        Material mat = GetComponentInChildren<SkinnedMeshRenderer>().material;
        while (timer < dissolveTime)
        {
            mat.SetFloat("_DissolveProgression", Mathf.Lerp(0, 1, timer / dissolveTime));
            timer += Time.deltaTime;
            yield return null;
        }
    }
    public void Animation()
    {
        animator.SetFloat("Speed", agent.velocity.magnitude);
    }

    public void PathfindToDestination(Vector3 pos)
    {
        agent.SetDestination(pos);
        // check if the path is valid
        if (debugPathfinding)
        {
            NavMeshPath path = new NavMeshPath();
            hasPath = agent.CalculatePath(pos, path);
            Color pathColor  = hasPath ? Color.green : Color.red;
            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                Debug.DrawLine(path.corners[i], path.corners[i + 1], pathColor);
            }
        }
    }

    public void Attack()
    {
        StartCoroutine(InvokeAttack());
    }

    public void ShootProjectile(Quaternion relativeDirection)
    {
        NetworkSpellManager.Instance.FireNPCProjectileRpc(GetComponent<NetworkObject>(), 3, 12, relativeDirection, 6);

    }

    private IEnumerator InvokeAttack()
    {
        while(triggerAttack == 0)
        {
            yield return null;
        }
        hitbox.gameObject.SetActive(true);
        yield return new WaitForSeconds(triggerAttack);
        hitbox.gameObject.SetActive(false);
        triggerAttack = 0;
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

    public void DeactivateRagdoll()
    {
        Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rbs)
        {
            rb.isKinematic = true;
        }

    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, aggroRange);
    }
}

public enum EnemyState
{
    Idle,
    Chase,
    Wander,
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
    private float wanderTimer = 0;
    public void OnEnter(ZombieAI pc)
    {
        wanderTimer = Time.time + Random.Range(2, 5);
    }

    public void OnUpdate(ZombieAI pc)
    {
        if(pc.LookForClosestTarget())
        {
            pc.SwitchState(EnemyState.Chase);
        }
        if(wanderTimer < Time.time)
        {
            pc.SwitchState(EnemyState.Wander);
        }
    }

    public void OnExit(ZombieAI pc)
    {

    }
}

public class WanderState : State
{
    Slowness slowness = new Slowness();
    private float enterTime;
    public void OnEnter(ZombieAI pc)
    {
        pc.PathfindToDestination(pc.transform.position + new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5)));
        slowness.duration = 1000;
        slowness.slowAmount = 0.5f;
        slowness.ApplyStatusEffect(pc.statusManager);
        enterTime = Time.time;
    }

    public void OnUpdate(ZombieAI pc)
    {
        if(Vector3.Distance(pc.transform.position,pc.agent.destination) < 1)
        {
            pc.SwitchState(EnemyState.Idle);
        }
        if (pc.LookForClosestTarget())
        {
            pc.SwitchState(EnemyState.Chase);
        }
        if(enterTime + 5 < Time.time)
        {
            pc.SwitchState(EnemyState.Idle);
        }
        pc.Animation();
    }

    public void OnExit(ZombieAI pc)
    {
        slowness.OnRemove(pc.statusManager);
        pc.statusManager.statusEffects.Remove(slowness);

    }
}

public class ChaseState : State
{
    private float attackTimer = 0;
    private float attackCooldown = 1.5f;
    private float[] attackspeed = { 2f, 2, 3 };

    public void OnEnter(ZombieAI pc)
    {
        pc.LookForClosestTarget();
    }

    public void OnUpdate(ZombieAI pc)
    {
        pc.PathfindToDestination(pc.target.transform.position);
        pc.Animation();
        if (attackTimer + attackCooldown < Time.time)
        {
            if(pc.target == null)
            {
                pc.SwitchState(EnemyState.Idle);
                return;
            }
           
            if (Vector3.Distance(pc.transform.position, pc.target.transform.position) < 1.5f)
            {
                pc.RotateTowardsTargetInstant();
                int rnd = Random.Range(0, 3);
                pc.Attack();
                attackTimer = Time.time;
                pc.animator.speed = attackspeed[rnd];
                pc.animator.SetTrigger("Attack");
                pc.animator.SetInteger("AttackAnimation", rnd);
                //pc.PathfindToDestination(pc.transform.position);
            }
        }

    }

    public void OnExit(ZombieAI pc)
    {

    }
}
