
using MoreMountains.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class StatusManager : NetworkBehaviour
{
    public enum Faction
    {
        Player,
        Demon,
        Neutral
    }

    public Faction faction = Faction.Neutral;
    public HitType materialType = HitType.Entity;
    public SoundType deathSound;
    public int level = 1;
    public int maxHp = 30;
    public NetworkVariable<int> hp = new NetworkVariable<int>(30,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Server);
    public int maxStamina = 0;
    [SerializeField] private int stamina = 0;
    [SerializeField] private int staminaRegenPerSecond = 5;
    public int staminaConsumptionPerSecond = 0;
    [SerializeField] private int baseAttackDamage = 1;
    public float movementSpeedMultiplier = 1;
    public int bonusDefense = 0;

    public int experienceDrop = 1;

    public StatsScaling statsScaling;

    public int bonusAttackDamage = 0;
    public float bonusAttackDamageMultiplier = 1;


    public int weaponAttackDamage = 0;

    public static Dictionary<Faction, List<StatusManager>> factionMembers = new Dictionary<Faction, List<StatusManager>>();

    public UnityEvent OnDeath;
    public UnityEvent OnDamage;
    public UnityEvent<int> OnDamageOwner;
    public UnityEvent NetworkDespawnEvent;
    public UnityEvent OnStaminaUpdate;

    public List<StatusEffect> statusEffects = new List<StatusEffect>();
    private DDAData ddaData;

    private float[] damagedTimers = new float[10];


    // Start is called before the first frame update
    public virtual void Start()
    {
        if(statsScaling != null)
        {
            maxHp += statsScaling.hpGrowth * level-1;
            baseAttackDamage += statsScaling.attackGrowth * level-1;
            experienceDrop += statsScaling.expGrowth * level-1;

        }
        if(maxStamina > 0) 
        { 
            StartCoroutine(RegenStamina());
        }
        // Stop all coroutines on death
        OnDeath.AddListener(() => StopAllCoroutines());
        OnDeath.AddListener(() => AddToFactionDictonary());
        ddaData = GetComponent<DDAData>() ?? null;
    }

    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            hp.Value = maxHp;
        }

    }

    private void OnEnable()
    {
        AddToFactionDictonary();
    }
    private void OnDisable()
    {
        factionMembers[faction].Remove(this);
    }

    private void AddToFactionDictonary()
    {
        if (!factionMembers.ContainsKey(faction))
        {
            factionMembers.Add(faction, new List<StatusManager>());
        }
        factionMembers[faction].Add(this);
    }

    private void Update()
    {
        for(int i = 0; i < statusEffects.Count; i++)
        {
            statusEffects[i].OnUpdate(this);
            statusEffects[i].duration -= Time.deltaTime;
            if (statusEffects[i].duration <= 0)
            {
                statusEffects[i].OnRemove(this);
                statusEffects.RemoveAt(i);
            }
        }
    }

    [Rpc(SendTo.Server)]
    public void HealHpRpc(int amount)
    {
        Hp.Value = (int)(Mathf.Clamp(Hp.Value + amount,0,maxHp));
    }   

    private IEnumerator RegenStamina()
    {
        float regenFloat = 0;
        while (true)
        {
            yield return new WaitForFixedUpdate();
            if (staminaConsumptionPerSecond > 0)
            {
                regenFloat -= staminaConsumptionPerSecond * Time.fixedDeltaTime;
                if (regenFloat <= 0)
                {
                    regenFloat += 1;
                    Stamina--;
                }
                continue;
            }

            if (Stamina < maxStamina)
            {

                regenFloat += staminaRegenPerSecond * Time.fixedDeltaTime;
                if (regenFloat >= 1)
                {
                    regenFloat -= 1;
                    Stamina++;
                }
            }
        }
    }
    [Rpc(SendTo.Everyone)]
    public void ApplyDamageRpc(int damage,Vector3 position,float force,ulong sourceId = 99999)
    {
        if (Hp.Value <= 0) return;
        int calculatedDamage = Mathf.Clamp(damage - bonusDefense, 1, 9999);
        if (IsServer)
        {
            DDAData sourceData = null;
            if(sourceId != 99999)
            {
                sourceData = NetworkManager.Singleton.ConnectedClients[sourceId].PlayerObject.GetComponent<DDAData>();
            }
            bool canTakeDamage = true;
            if (ddaData != null)
            {
                canTakeDamage = false;
                // Check damage timers
                for(int i = 0; i < ddaData.maxDamageInstancesPerSecond.Value; i++)
                {
                    if (Time.time -1 >= damagedTimers[i])
                    {
                        damagedTimers[i] = Time.time;
                        canTakeDamage = true;
                        break;
                    }
                }
            }
            if (canTakeDamage)
            {
                OnDamageOwnerTriggerRpc(calculatedDamage);
                if (sourceId != 99999)
                {
                    FPSController damageSource = NetworkManager.Singleton.ConnectedClients[sourceId].PlayerObject.GetComponent<FPSController>();
                    damageSource.TrackDamageDealtRpc(calculatedDamage);
                    if(sourceData != null)
                    {
                        if(Hp.Value - calculatedDamage <= sourceData.damageOutgoingExecuteTreshold.Value) // execute on the spot
                        {
                            calculatedDamage = Hp.Value;
                        }
                    }
                }
                Hp.Value -= calculatedDamage;
            }
            if (Hp.Value <= 0)
            {
                InvokeDeathRpc();
                if(sourceId != 99999)
                {
                    FPSController damageSource = NetworkManager.Singleton.ConnectedClients[sourceId].PlayerObject.GetComponent<FPSController>();
                    damageSource.TrackEnemiesKilledRpc();
                }
            }
        }
        GetComponentInChildren<Animator>().SetTrigger("Damage");
    }

    [Rpc(SendTo.Owner)]
    private void OnDamageOwnerTriggerRpc(int calculatedDamage)
    {
        print("test");
        OnDamageOwner.Invoke(calculatedDamage);
    }

    [Rpc(SendTo.Everyone)]
    public void InvokeDeathRpc()
    {
        OnDeath.Invoke();
    }

    public void RagdollForce(Vector3 source,float force)
    {
        Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rbs)
        {
            rb.isKinematic = false;
            rb.AddForce(source * force, ForceMode.Impulse);
        }
    }

    public void UpdateHPBar(MMProgressBar progressbar)
    {
        progressbar.UpdateBar01(Hp.Value / (float)maxHp);
    }

    public static List<StatusManager> GetEnemies(Faction faction)
    {
        // Hard coded for now
        switch(faction)
        {
            case Faction.Player:
                return factionMembers[Faction.Demon];
            case Faction.Demon:
                return factionMembers[Faction.Player];
            default:
                return new List<StatusManager>();
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        NetworkDespawnEvent.Invoke();
    }

    public NetworkVariable<int> Hp 
    { 
        get => hp;
        set
        {
            OnDamage.Invoke();
            hp = value;
        }
    }

    public float MovementSpeedMultiplier
    {
        get => Mathf.Clamp(movementSpeedMultiplier,0.2f,3);
        set
        {
            movementSpeedMultiplier = value;
        }
    }

    public int AttackDamage { get => Mathf.CeilToInt((baseAttackDamage + weaponAttackDamage + bonusAttackDamage) * bonusAttackDamageMultiplier); }
    public int Defense { get => bonusDefense; }
    public int Stamina 
    { 
        get => stamina;
        set
        {
            stamina = Mathf.Clamp(value,0,maxStamina);
            OnStaminaUpdate.Invoke();
            
        }
    }
}
