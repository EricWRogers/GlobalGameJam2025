using OmnicatLabs.Timers;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRules : NetworkBehaviour
{
    public int stocks = 3;

    [HideInInspector]
    public Timer matchTimeRemaining;
    public float killLevel = 30f;

    private Dictionary<BubbleController, int> stockDictionary = new Dictionary<BubbleController, int>();

    public GameObject playerPrefab;
    public List<Transform> spawnPoints = new List<Transform>();

    public float prepDuration = 5f;
    public float matchTime = 180f; //180
    public TMP_Text timerText;
    bool prepTimeEnd = false;
    bool gameEnd = false;
    public NetworkVariable<float> remainingTime = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int> colorIndex = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public static GameRules Instance
    {
        get; private set;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

    }


    private void Start()
    {
        SpawnPlayersForAllClients();
        

        if (IsServer) //Server must handle it.
        {

            remainingTime.Value = prepDuration; //We should lock ALL movement player and server untill this ends.

            foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
            {
                stockDictionary.Add(kvp.Value.PlayerObject.GetComponent<BubbleController>(), stocks);
            }
        }
        if (IsServer)
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                ulong playerNetworkObjectRef = client.PlayerObject.GetComponentInParent<NetworkObject>().NetworkObjectId;
                ColorAssignmentsClientRPC(client.ClientId, playerNetworkObjectRef);
                colorIndex.Value++;
            }
        }
        


        // TimerManager.Instance.CreateTimer(matchTime, EndMatch, out matchTimeRemaining);


    }

    

    private void Update()
    {
        if (IsServer && remainingTime.Value > 0)
        {
            remainingTime.Value -= Time.deltaTime;
        }
        else if (IsServer && prepTimeEnd && remainingTime.Value <= 0)
        {

            foreach (var client in NetworkManager.Singleton.ConnectedClients)
            {

                client.Value.PlayerObject.Despawn(true);
            }
            RemoveClientRPC();
            if (NetworkManager.Singleton.IsHost)
            {
                SceneManager.LoadScene("LobbyScene");

                //NetworkManager.Singleton.Shutdown();

            }

            if (IsServer)
            {
                if (stockDictionary.Values.Count(value => value <= 0) == 1)
                {
                    EndMatch();
                }
            }



        }
        else if (IsServer && remainingTime.Value <= 0) //Reenable movement somewhere here. If we hit this then we've elapsed prep time.
        {
            prepTimeEnd = true;

            if (prepTimeEnd)
            {
                remainingTime.Value = matchTime;
            }
        }


        UpdateTimerText(remainingTime.Value); //Everyone calls this. Convenient due to network var. It auto syncs so who cares.
    }


    [ClientRpc]
    public void RemoveClientRPC()
    {
        if (NetworkManager.Singleton.IsClient)
        {
            SceneManager.LoadScene("LobbyScene");
            //Debug.Log("Disconnecting as Client...");
            //NetworkManager.Singleton.Shutdown();


        }

    }

    private void UpdateTimerText(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    [ServerRpc(RequireOwnership = false)]
    public void KillPlayerServerRPC(ulong id)
    {
        NetworkManager.Singleton.ConnectedClients.TryGetValue(id, out var client);

        BubbleController player = client.PlayerObject.GetComponent<BubbleController>();

        

        if (stockDictionary[player] >= 0)
        {
            //client.PlayerObject.Despawn(true);
            Debug.Log("Ran Kill for " + gameObject.name + client.ClientId);
            stockDictionary[player]--;
            UpdateClientStockClientRPC(id, stockDictionary[player]);
        }
        else
        {
            //TODO transition to spectator
            //TODO turn off player
        }

        ClientPositionCorrectClientRPC(client.ClientId);
        player.transform.position = spawnPoints[Random.Range(0, spawnPoints.Count)].transform.position;
    }

    [ClientRpc]
    private void UpdateClientStockClientRPC(ulong id, int updatedStocks)
    {
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(id, out var client))
            return;

        BubbleController player = client.PlayerObject.GetComponent<BubbleController>();
        PlayerData playerData = client.PlayerObject.GetComponent<PlayerData>();

        
        playerData.stocks = updatedStocks;
        player.transform.position = spawnPoints[Random.Range(0, spawnPoints.Count)].transform.position;
    }

    [ClientRpc]
    private void ClientPositionCorrectClientRPC(ulong id)
    {
        if (!NetworkManager.Singleton.ConnectedClients.TryGetValue(id, out var client))
            return;

        BubbleController player = client.PlayerObject.GetComponent<BubbleController>();
        player.transform.position = spawnPoints[Random.Range(0, spawnPoints.Count)].transform.position;
    }

    private void EndMatch()
    {
        RemoveClientRPC();
        if (NetworkManager.Singleton.IsHost)
        {
            SceneManager.LoadScene("LobbyScene");

            //NetworkManager.Singleton.Shutdown();

        }
        /*
        var winner = stockDictionary.OrderByDescending(kvp => kvp.Value).First();
        ShowWinner();
        stockDictionary.Remove(winner.Key);
        ShowLosers();
        */
    }

    public void ShowLosers()
    {
        foreach (var item in stockDictionary)
        {
            Debug.Log($"Loser: {item.Key.name}");
        }
    }

    public void ShowWinner()
    {
        Debug.Log($"Winner: {stockDictionary.OrderByDescending(kvp => kvp.Value).First().Key}");
    }

    private void SpawnPlayersForAllClients()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            SpawnPlayer(client.Key);
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            Transform spawnpoint = GetSpawnPoint();
            GameObject playerInstance = Instantiate(playerPrefab, spawnpoint.position, spawnpoint.rotation);


            NetworkObject playerNetworkObject = playerInstance.GetComponent<NetworkObject>();
            playerNetworkObject.SpawnAsPlayerObject(clientId);
        }
    }
    private Transform GetSpawnPoint()
    {
        int random = Random.Range(0, spawnPoints.Count);
        Transform spawnPoint = spawnPoints[random];
        spawnPoints.Remove(spawnPoints[random]);

        return spawnPoint;
    }

    [ClientRpc]
    private void ColorAssignmentsClientRPC(ulong clientId, ulong obj)
    {
        //NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client);
        /* if (client == null)
         {
             Debug.Log("Client null??");
         }
         if (client.PlayerObject == null)
         {
             Debug.Log("Cant find player");
         }
         if (client.PlayerObject.GetComponent<PlayerData>())
         {
        */
        //NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(obj, out var playerNetworkObject);
        //client.PlayerObject.GetComponent<PlayerData>().color = colorIndex.Value;
        //client.PlayerObject.gameObject.GetComponent<PlayerData>().stocks = stocks;
        }
        



    
}
