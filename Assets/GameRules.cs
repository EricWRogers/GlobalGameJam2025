using UnityEngine;
using OmnicatLabs.Timers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem.Controls;
using Unity.Netcode;

public class GameRules : MonoBehaviour
{
    public int stocks = 3;
    public float matchTime = 180f;
    [HideInInspector]
    public Timer matchTimeRemaining;
    public float killLevel = -10f;

    private Dictionary<BubbleController, int> stockDictionary = new Dictionary<BubbleController, int>();

    public GameObject playerPrefab;
    public List<Transform> spawnPoints = new List<Transform>();

    public static GameRules Instance {
        get; private set; 
    }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(this);
        }
    }

    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;
    }

    private void Start() {
        SpawnPlayersForAllClients();


        TimerManager.Instance.CreateTimer(matchTime, EndMatch, out matchTimeRemaining);

        foreach (var kvp in NetworkManager.Singleton.ConnectedClients) {
            stockDictionary.Add(kvp.Value.PlayerObject.GetComponent<BubbleController>(), stocks);
        }

       


    }

    public void KillPlayer(BubbleController player) {
        if (stockDictionary[player] >= 0) {
            stockDictionary[player]--;
        } else {
            //TODO transition to spectator
            //TODO turn off player
        }

        if (stockDictionary.Values.Count(value => value <= 0) == 1) {
            EndMatch();
        }

        player.transform.position = spawnPoints[Random.Range(0, spawnPoints.Count)].transform.position;
    }

    private void EndMatch() {
        var winner = stockDictionary.OrderByDescending(kvp => kvp.Value).First();
        ShowWinner();
        stockDictionary.Remove(winner.Key);
        ShowLosers();
        
    }
    
    public void ShowLosers() {
        foreach (var item in stockDictionary)
        {
            Debug.Log($"Loser: {item.Key.name}");
        }
    }

    public void ShowWinner() {
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
