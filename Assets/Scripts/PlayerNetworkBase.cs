using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkBase : ClientBase
{
    public PlayerData playerData;
    /// <summary>
    /// A method which sets up the player when they join on the server.
    /// </summary>
    public void FirstJoinSetupPlayer()
    {
        if (IsClient)
        {
            SetupPlayerServerRPC();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetupPlayerServerRPC()
    {
        playerData = ServerTruth.Instance.PlayerDataConstruction(NetworkManager.Singleton.LocalClientId);

        SetupPlayerClientRPC(playerData.id, playerData.playerName, playerData.playerColor, playerData.stocks);
    }

    [ClientRpc]
    private void SetupPlayerClientRPC(ulong id, string playerName, Color playerColor, int stocks)
    {
        playerData = new PlayerData(id, playerName, playerColor, stocks);

    }
}

public class PlayerData
{
    public ulong id;
    public string playerName;
    public Color playerColor;
    public int stocks;

    public PlayerData(ulong id, string playerName, Color playerColor, int stocks)
    {
        this.id = id;
        this.playerName = playerName;
        this.playerColor = playerColor;
        this.stocks = stocks;
        
    }
}
