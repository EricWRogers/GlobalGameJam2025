using Unity.Netcode;
using UnityEngine;

public class Bumper : NetworkBehaviour
{
    public Animator anim;
    public float bounceFactor = 5f;

    public void OnTriggerEnter(Collider col)
    {
        if (!IsServer) return;

        if (col.gameObject.CompareTag("Player"))
        {
            ulong clientId = col.GetComponent<NetworkObject>().OwnerClientId;
            HandleBounce(clientId, col);


           
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
            GameObject playerObject = playerNetworkObject.gameObject;

            Collider col = playerObject.GetComponent<Collider>();

            if (col != null)
            {
                anim.SetBool("didHit", true);
                Vector3 bounce = (col.transform.position - transform.position).normalized;
                col.GetComponent<Rigidbody>().AddForce(bounce * bounceFactor, ForceMode.Impulse);
            }
        }
    }

    [ServerRpc]
    public void BounceHandlerServerRPC()
    {

    }

    public void OnTriggerExit(Collider col) 
    {
        if(col.gameObject.CompareTag("Player"))
        {
            BounceResetClientRPC();
        }
    }

    [ClientRpc]
    public void BounceResetClientRPC()
    {
        anim.SetBool("didHit", false);
    }
}
