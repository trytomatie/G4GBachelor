﻿using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;


public class NetworkProjectile : NetworkBehaviour
    {
    public int penetrationCounter = 0;
    public int damage = 1;
    private List<GameObject> hitObjects = new List<GameObject>();
    public GameObject attchedVFX;
    private NetworkObject networkObject;
    public StatusManager soruce;
    public float physicsForce = 100;
    private void Start()
    {
        if(!IsServer) enabled = false;
        networkObject = GetComponent<NetworkObject>();
        DespawnTimer(10);

    }
    private void OnCollisionEnter(Collision other)
    {
        if(other.collider.isTrigger || !IsServer) return;
        if(other.collider.GetComponent<StatusManager>() != null && !hitObjects.Contains(other.gameObject))
        {
            StatusManager otherStatus = other.collider.GetComponent<StatusManager>();
            if(otherStatus.faction == soruce.faction) return;
            other.collider.GetComponent<StatusManager>().ApplyDamageRpc(damage,transform.position,physicsForce);
            hitObjects.Add(other.gameObject);
            if (penetrationCounter > 0)
            {
                penetrationCounter--;
            }
            else
            {
                DespawnLogic(other.contacts[0].point);
            }
        }
        else
        {
            if(networkObject.IsSpawned)
            {
                DespawnLogic(other.contacts[0].point);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.isTrigger || !IsServer) return;
        if(other.GetComponent<StatusManager>() != null && !hitObjects.Contains(other.gameObject))
        {
            StatusManager otherStatus = other.GetComponent<StatusManager>();
            if(otherStatus.faction == soruce.faction) return;
            other.GetComponent<StatusManager>().ApplyDamageRpc(damage,transform.position,physicsForce);
            hitObjects.Add(other.gameObject);
            if (penetrationCounter > 0)
            {
                penetrationCounter--;
            }
            else
            {
                DespawnLogic(other.ClosestPoint(transform.position));
            }
        }
        else
        {
            if(networkObject.IsSpawned)
            {
                DespawnLogic(other.ClosestPoint(transform.position));
            }
        }
    }

    private void DespawnLogic(Vector3 impactPoint)
    {
        if(attchedVFX!= null)
        {
            attchedVFX.transform.parent = null;
            Destroy(attchedVFX, 0.3f);
        }
        NetworkVFXManager.Instance.SpawnVFXRpc(0, impactPoint, transform.rotation);
        networkObject.DontDestroyWithOwner = true;
        networkObject.Despawn();
    }

    IEnumerator DespawnTimer(float time)
    {
        yield return new WaitForSeconds(time);
        networkObject.DontDestroyWithOwner = true;
        networkObject.Despawn();

    }
}
