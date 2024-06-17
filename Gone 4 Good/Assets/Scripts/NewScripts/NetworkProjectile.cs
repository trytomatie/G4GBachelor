using NUnit.Framework;
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

    private NetworkObject networkObject;
    private void Start()
    {
        if(!IsServer) enabled = false;
        networkObject = GetComponent<NetworkObject>();
        DespawnTimer(10);

    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<StatusManager>() != null && !hitObjects.Contains(other.gameObject))
        {
            other.GetComponent<StatusManager>().ApplyDamageRpc(damage,transform.position);
            hitObjects.Add(other.gameObject);
            if (penetrationCounter > 0)
            {
                penetrationCounter--;
            }
            else
            {
                networkObject.DontDestroyWithOwner = true;
                networkObject.Despawn();
            }
        }
        else
        {
            networkObject.DontDestroyWithOwner = true;
            networkObject.Despawn();
        }
    }

    IEnumerator DespawnTimer(float time)
    {
        yield return new WaitForSeconds(time);
        networkObject.DontDestroyWithOwner = true;
        networkObject.Despawn();
    }
}
