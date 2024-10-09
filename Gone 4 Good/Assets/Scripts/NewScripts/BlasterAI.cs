using System.Collections;
using Unity.Netcode;
using UnityEngine;


public class BlasterAI : ZombieAI
{
    public GameObject BlasterProjectile;
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
        enemyStates[1] = new BlasterChaseState();
        enemyStates[2] = new WanderState();
        enemyStates[(int)currentState].OnEnter(this);
        animator.SetFloat("RunAnimation", Random.Range(0, 3));
    }

    public void BlasterAttack()
    {
        StartCoroutine(HandleBlasterAttack());
    }

    public IEnumerator HandleBlasterAttack()
    {
        while (triggerAttack == 0)
        {
            yield return null;
        }
        ShootProjectile(transform.rotation);
        ShootProjectile(transform.rotation * Quaternion.Euler(0, 25, 0));
        ShootProjectile(transform.rotation * Quaternion.Euler(0, -25, 0));
        ShootProjectile(transform.rotation * Quaternion.Euler(0, -12.5f, 0));
        ShootProjectile(transform.rotation * Quaternion.Euler(0, 12.5f, 0));
        triggerAttack = 0;
    }

}

public class BlasterChaseState : State
{
    public float attackRange = 12;
    public float tickRate = 1f;
    public float tickTimer = 0;
    private float rnd;

    private float attackTimer = 0;
    private float attackCooldown = 2;

    public void OnEnter(ZombieAI pc)
    {

    }

    public void OnExit(ZombieAI pc)
    {

    }

    public void OnUpdate(ZombieAI pc)
    {
        // Tickrate
        if(tickTimer < Time.time)
        {
            rnd = Random.Range(0, 100);
            // if outside attack range, move into attack range
            if(Vector3.Distance(pc.transform.position, pc.target.transform.position) > attackRange)
            {
                pc.agent.SetDestination(pc.target.transform.position);
            }
            else
            {
                if(rnd < 80)
                {
                    pc.agent.SetDestination(pc.transform.position);
                    if(attackTimer < Time.time)
                    {
                        pc.animator.SetInteger("AttackAnimation", 4);
                        pc.animator.SetTrigger("Attack");
                        ((BlasterAI)pc).BlasterAttack();

                        attackTimer = Time.time + attackCooldown;
                    }
                }
                else
                {
                    // Move to random pos within 2m
                    Vector3 randomPos = pc.target.transform.position + new Vector3(Random.Range(-2, 2), 0, Random.Range(-2, 2));
                    pc.agent.SetDestination(randomPos);
                }

            }

            tickTimer = Time.time + tickRate;
        }

        pc.RotateTowardsTarget();
    }
}
