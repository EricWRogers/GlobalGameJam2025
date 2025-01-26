using Unity.Netcode;
using UnityEngine;

public class JumpPad : NetworkBehaviour
{
    public Animator anim;
    public GameObject direction;
    public float springForce = 10f;
    
    public void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.CompareTag("Player"))
        {
            ulong clientId = col.GetComponentInParent<NetworkObject>().OwnerClientId;

            bool isLocal = true;

            if (NetworkManager.Singleton)
                isLocal = NetworkManager.Singleton.LocalClientId == clientId;


            if (isLocal)
            {
                // server
                HandleJump(clientId, col);

                // local
                anim.SetBool("didHit", true);
                col.GetComponentInParent<Rigidbody>().AddForce(direction.transform.forward * (col.GetComponentInParent<Rigidbody>().linearVelocity.magnitude + springForce), ForceMode.Impulse);
            }
            
        }
    }

    public void HandleJump(ulong clientId, Collider col) //Probably a good workflow tbh.
    {
        ulong playerNetworkObjectRef = col.GetComponentInParent<NetworkObject>().NetworkObjectId;
        JumpHandlerClientRPC(clientId, playerNetworkObjectRef); //Cant send complicated types unless you tell it how to serialize. It's a pain.
    }

    [ClientRpc]
    public void JumpHandlerClientRPC(ulong clientId, ulong reference)
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
    public void JumpHandlerServerRPC()
    {

    }

    public void OnTriggerExit(Collider col)
    {
        if(col.gameObject.CompareTag("Player"))
        {
           ulong clientId = col.GetComponentInParent<NetworkObject>().OwnerClientId;

            bool isLocal = true;

            if (NetworkManager.Singleton)
                isLocal = NetworkManager.Singleton.LocalClientId == clientId;
            
            if (isLocal)
            {
                anim.SetBool("didHit", false);

                ulong playerNetworkObjectRef = col.GetComponentInParent<NetworkObject>().NetworkObjectId;
                JumpResetClientRPC(clientId, playerNetworkObjectRef);
            }
        }
    }

    [ClientRpc]
    public void JumpResetClientRPC(ulong clientId, ulong reference)
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
