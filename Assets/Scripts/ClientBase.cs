using Unity.Netcode;
using UnityEngine;

public class ClientBase : NetworkBehaviour
{

    #region IsServerReady

    /// <summary>
    /// A method which returns the readyness of the ServerTruthSingleton. 
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
