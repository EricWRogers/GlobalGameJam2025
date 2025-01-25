using Unity.Netcode;
using UnityEngine;

public class Bumper : NetworkBehaviour
{
    public Animator anim;
    public float bounceFactor = 5f;

    public void OnTriggerEnter(Collider col)
    {
        //if (!IsServer) return;

        if (col.gameObject.CompareTag("Player"))
        {
            ulong clientId = col.GetComponent<NetworkObject>().OwnerClientId;

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

                Rigidbody rb = player.GetComponent<Rigidbody>();
                float mag = rb.linearVelocity.magnitude;
                Vector3 dir = (col.transform.position - transform.position).normalized;
                rb.AddForce(dir * (bounceFactor + mag), ForceMode.Impulse);
            }
        }
    }

    public void HandleBounce(ulong clientId, Collider col) //Probably a good workflow tbh.
    {
        ulong playerNetworkObjectRef = col.GetComponent<NetworkObject>().NetworkObjectId;
        BounceHandlerClientRPC(clientId, playerNetworkObjectRef); //Cant send complicated types unless you tell it how to serialize. It's a pain.
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

    [ServerRpc]
    public void BounceHandlerServerRPC()
    {

    }

    public void OnTriggerExit(Collider col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            ulong clientId = col.GetComponent<NetworkObject>().OwnerClientId;

            bool isLocal = true;

            if (NetworkManager.Singleton)
                isLocal = NetworkManager.Singleton.LocalClientId == clientId;
            
            if (isLocal)
            {
                anim.SetBool("didHit", false);

                ulong playerNetworkObjectRef = col.GetComponent<NetworkObject>().NetworkObjectId;
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
}
