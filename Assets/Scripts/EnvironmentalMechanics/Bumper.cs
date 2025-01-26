using Unity.Netcode;
using UnityEngine;

public class Bumper : NetworkBehaviour
{
    public Animator anim;
    public float bounceFactor = 5f;
    public float minMagnitude = 10f;

    public void OnTriggerEnter(Collider col)
    {
        //if (!IsServer) return;

        if (col.gameObject.CompareTag("Player"))
        {
            ulong clientId = col.GetComponentInParent<NetworkObject>().OwnerClientId;

            bool isLocal = true;

            if (NetworkManager.Singleton)
                isLocal = NetworkManager.Singleton.LocalClientId == clientId;


            if (isLocal)
            {
                // server
                HandleBounce(clientId, col);

                // local
                GameObject player = col.gameObject;
                anim.SetBool("didHit", true);

                Rigidbody rb = player.GetComponentInParent<Rigidbody>();
                float mag = rb.linearVelocity.magnitude;
                Vector3 dir = (col.transform.position - transform.position).normalized;
                Vector3 force = dir * (bounceFactor + mag);
                if(force.magnitude < minMagnitude)
                {
                    force = force.normalized * minMagnitude;
                }
                rb.AddForce(force, ForceMode.Impulse);
            }
        }
    }

    public void HandleBounce(ulong clientId, Collider col) //Probably a good workflow tbh.
    {
        ulong playerNetworkObjectRef = col.GetComponentInParent<NetworkObject>().NetworkObjectId;
        BounceHandlerClientRPC(clientId, playerNetworkObjectRef); //Cant send complicated types unless you tell it how to serialize. It's a pain.
        BounceHandlerServerRPC(clientId, playerNetworkObjectRef);
    }

    [ClientRpc]
    public void BounceHandlerClientRPC(ulong clientId, ulong reference)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(reference, out var playerNetworkObject))
        {
            if (NetworkManager.Singleton.LocalClientId != clientId)
            {
                anim.SetBool("didHit", true);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void BounceHandlerServerRPC(ulong clientId, ulong reference)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(reference, out var playerNetworkObject))
        {
            if (NetworkManager.Singleton.LocalClientId != clientId)
            {
                anim.SetBool("didHit", true);
            }
        }
    }

    public void OnTriggerExit(Collider col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            ulong clientId = col.GetComponentInParent<NetworkObject>().OwnerClientId;

            bool isLocal = true;

            if (NetworkManager.Singleton)
                isLocal = NetworkManager.Singleton.LocalClientId == clientId;
            
            if (isLocal)
            {
                anim.SetBool("didHit", false);

                ulong playerNetworkObjectRef = col.GetComponentInParent<NetworkObject>().NetworkObjectId;
                BounceResetClientRPC(clientId, playerNetworkObjectRef);
            }
        }
    }

    [ClientRpc]
    public void BounceResetClientRPC(ulong clientId, ulong reference)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(reference, out var playerNetworkObject))
        {
            if (NetworkManager.Singleton.LocalClientId != clientId)
            {
                anim.SetBool("didHit", false);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void BounceResetServerRPC(ulong clientId, ulong reference)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(reference, out var playerNetworkObject))
        {
            if (NetworkManager.Singleton.LocalClientId != clientId)
            {
                anim.SetBool("didHit", false);
            }
        }
    }
}
