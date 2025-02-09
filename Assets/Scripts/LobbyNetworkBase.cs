using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
using System.Linq;

public class LobbyNetworkBase : ClientBase
{
    private void OnEnable()
    {
        
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }
    private void OnClientConnected(ulong clientId)
    {
        UpdatePlayers();
    }
    void Start()
    {
        UpdatePlayers();
    }


    void Update() {
    
    
    }

    public void UpdatePlayers()
    {
        if (IsClient)
        {
            UpdatePlayersServerRPC();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePlayersServerRPC()
    {
        Dictionary<ulong, PlayerData> players = ServerTruth.Instance.GetAllPlayers();

        List<PlayerDataNetworkContainer> playerList = new List<PlayerDataNetworkContainer>(); //So uh. . Yeah we had to slap this into a JSON so we could send it over the network.
        foreach (var player in players)
        {
            var playerData = player.Value;
            playerList.Add(new PlayerDataNetworkContainer(player.Key, playerData.playerName, playerData.playerColor, playerData.stocks));
        }


        string json = JsonUtility.ToJson(new PlayerDataListWrapper(playerList));

        
        UpdatePlayersClientRPC(json);
    }

    [ClientRpc]
    private void UpdatePlayersClientRPC(string json)
    {
        PlayerDataListWrapper wrapper = JsonUtility.FromJson<PlayerDataListWrapper>(json);
        List<PlayerDataNetworkContainer> playerList = wrapper.playerDataList;

        Dictionary<ulong, PlayerDataNetworkContainer> playerDict = new Dictionary<ulong, PlayerDataNetworkContainer>();
        foreach (var player in playerList)
        {
            playerDict.Add(player.playerId, player);
        }

        
        UpdateUI(playerDict);
    }

    protected virtual void UpdateUI(Dictionary<ulong, PlayerDataNetworkContainer> playerDict)
    {
        if (playerDict.Any())
        {
            Debug.Log("Players is aquired!");
        }
        else
        {
            Debug.Log("Players have not been aquired.");
        }
    }
}

#region serialization
[System.Serializable]
public class PlayerDataNetworkContainer
{
    public ulong playerId;
    public string playerName;
    public Color playerColor;
    public int stocks;

    // Constructor
    public PlayerDataNetworkContainer(ulong playerId, string playerName, Color playerColor, int stocks)
    {
        this.playerId = playerId;
        this.playerName = playerName;
        this.playerColor = playerColor;
        this.stocks = stocks;
    }
}

[System.Serializable]
public class PlayerDataListWrapper
{
    public List<PlayerDataNetworkContainer> playerDataList;

    public PlayerDataListWrapper(List<PlayerDataNetworkContainer> playerDataList)
    {
        this.playerDataList = playerDataList;
    }
}
#endregion