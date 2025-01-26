using OmnicatLabs.Timers;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRules : NetworkBehaviour
{
    public int stocks = 3;

    [HideInInspector]
    public Timer matchTimeRemaining;
    public float killLevel = -10f;

    private Dictionary<BubbleController, int> stockDictionary = new Dictionary<BubbleController, int>();

    public GameObject playerPrefab;
    public List<Transform> spawnPoints = new List<Transform>();

    public float prepDuration = 5f;
    public float matchTime = 10f; //180
    public TMP_Text timerText;
    bool prepTimeEnd = false;
    bool gameEnd = false;
    public NetworkVariable<float> remainingTime = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

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

    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;
    }

    private void Start()
    {
        SpawnPlayersForAllClients();

        if (IsServer) //Server must handle it.
        {
            remainingTime.Value = prepDuration; //We should lock ALL movement player and server untill this ends.
        }

        // TimerManager.Instance.CreateTimer(matchTime, EndMatch, out matchTimeRemaining);

        foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
        {
            stockDictionary.Add(kvp.Value.PlayerObject.GetComponent<BubbleController>(), stocks);
        }
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

    public void KillPlayer(BubbleController player)
    {
        if (stockDictionary[player] >= 0)
        {
            stockDictionary[player]--;
        }
        else
        {
            //TODO transition to spectator
            //TODO turn off player
        }

        if (stockDictionary.Values.Count(value => value <= 0) == 1)
        {
            EndMatch();
        }

        player.transform.position = spawnPoints[Random.Range(0, spawnPoints.Count)].transform.position;
    }

    private void EndMatch()
    {
        var winner = stockDictionary.OrderByDescending(kvp => kvp.Value).First();
        ShowWinner();
        stockDictionary.Remove(winner.Key);
        ShowLosers();

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
            GameObject playerInstance = Instantiate(playerPrefab, GetSpawnPoint().position, GetSpawnPoint().rotation);


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
}
