using System.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// This is not necessarily meant for just clients or even just players. It's meant to provide tools which 
/// help with networking and syncronization. 
/// </summary>
public class ClientBase : NetworkBehaviour
{
    [HideInInspector]
    public bool isReady = false;

    private void Start()
    {
        StartCoroutine(BlockUntilReady()); // So basically, network objects that inherit from this class will be forced to wait for the ServerTruth to set up the server and the game before doing ANYTHING.
    }

    private IEnumerator BlockUntilReady()
    {
        while (!isReady)
        {
            yield return null;
        }
        
        Debug.Log("Client heard that the server is ready. Continuing execution.");
    }


    #region IsServerReady

    /// <summary>
    /// A method which returns the readyness of the ServerTruth Singleton. 
    /// </summary>
    public void IsServerReady()
    {
        if (IsClient)
        {
            IsServerReadyServerRPC();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void IsServerReadyServerRPC()
    {

        bool ready = ServerTruth.Instance.IsServerReady();

    
        IsServerReadyClientRPC(ready);
    }

    [ClientRpc]
    private void IsServerReadyClientRPC(bool ready)
    {
        HandleServerReadyState(ready);
    }

    protected virtual void HandleServerReadyState(bool ready)
    {
        if (ready)
        {
            Debug.Log("Server is ready!");
        }
        else
        {
            Debug.Log("Server is not ready.");
        }
    }
    #endregion
}
