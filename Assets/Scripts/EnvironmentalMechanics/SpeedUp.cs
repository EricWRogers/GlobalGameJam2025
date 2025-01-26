using Unity.Netcode;
using UnityEngine;

public class SpeedUp : NetworkBehaviour
{
    public float speedingTicket = 20f;
    public GameObject direction;

    void OnTriggerEnter(Collider col)
    {
        if(col.gameObject.CompareTag("Player"))
        {
            ulong clientId = col.GetComponent<NetworkObject>().OwnerClientId;

            bool isLocal = true;

            if (NetworkManager.Singleton)
                isLocal = NetworkManager.Singleton.LocalClientId == clientId;

            //If using Ben's Player Controller switch from GetComponent to GetComponentInParent
            if (isLocal)
            {
                // server
                HandleSpeed(clientId, col);    
                col.GetComponent<Rigidbody>().AddForce(direction.transform.forward * (col.GetComponent<Rigidbody>().linearVelocity.magnitude + speedingTicket), ForceMode.Impulse);
            }
        }
    }

    public void HandleSpeed(ulong clientId, Collider col) //Probably a good workflow tbh.
    {
        ulong playerNetworkObjectRef = col.GetComponent<NetworkObject>().NetworkObjectId;
        SpeedHandlerClientRPC(clientId, playerNetworkObjectRef); //Cant send complicated types unless you tell it how to serialize. It's a pain.
    }

    [ClientRpc]
    public void SpeedHandlerClientRPC(ulong clientId, ulong reference)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(reference, out var playerNetworkObject))
        {
            if (NetworkManager.Singleton.LocalClientId != clientId)
            {
                //do nothing
                //continue;
            }
        }
    }

    [ServerRpc]
    public void SpeedHandlerServerRPC()
    {

    }

    public void OnTriggerExit(Collider col)
    {
        if(col.gameObject.CompareTag("Player"))
        {
            ulong clientId = col.GetComponent<NetworkObject>().OwnerClientId;

            bool isLocal = true;

            if (NetworkManager.Singleton)
                isLocal = NetworkManager.Singleton.LocalClientId == clientId;
            
            if (isLocal)
            {
                ulong playerNetworkObjectRef = col.GetComponent<NetworkObject>().NetworkObjectId;
                SpeedResetClientRPC(clientId, playerNetworkObjectRef);
            }
        }
    }

    [ClientRpc]
    public void SpeedResetClientRPC(ulong clientId, ulong reference)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(reference, out var playerNetworkObject))
        {
            if (NetworkManager.Singleton.LocalClientId != clientId)
            {
                //do nothing
                //continue;
            }
        }
    }
}
