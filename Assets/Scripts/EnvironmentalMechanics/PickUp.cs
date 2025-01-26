using Unity.Netcode;
using UnityEngine;

public class PickUp : NetworkBehaviour
{
    public ParticleSystem bubbles;
    public GameObject bottle;
    public float rotateSpeed = 45f;


    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0); //When we don't specify a RPC, ALL clients run this code.
    }

    public void OnTriggerEnter(Collider col)
    {
        if (!IsServer) return;//This will ensure the SERVER is preforming this. 

        if (col.CompareTag("Player")) 
        {
            ulong clientId = col.GetComponentInParent<NetworkObject>().OwnerClientId; //Get the client who did the deed.
            
            HandlePickup(clientId);
        }
    }

    private void HandlePickup(ulong clientId)
    {
        VisualPickupHandlerClientRpc(); //When not specifying, it sends to ALL. Remember that Client RPCs must be called from the server.

        PickupStatChangeServerRpc(clientId);

        
    }

    [ClientRpc]
    private void VisualPickupHandlerClientRpc() //Have all clients see the visual change.
    {
        bubbles.Play();
        bottle.SetActive(false);
        NetworkObject.Destroy(gameObject, 4f); //This will ensure the object is destroyed across ALL clients. Obvs pickups will need to be network objs.
    }

    [ServerRpc(RequireOwnership = false)]
    private void PickupStatChangeServerRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client)) //Ensuring we only apply the change to the correct client.
        {
           /* var player = client.PlayerObject.GetComponent<playerClassOrSomething>();
            if (player != null)
            {
               player.ApplySpeedBoost(speedBoostAmount, boostDuration); ex
            }*/
        }

        //You would call a client RPC here to update healthbars for everyone or somethin.
    }
}

