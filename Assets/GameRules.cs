using UnityEngine;
using OmnicatLabs.Timers;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem.Controls;

public class GameRules : MonoBehaviour
{
    public int stocks = 3;
    public float matchTime = 180f;
    [HideInInspector]
    public Timer matchTimeRemaining;
    public float killLevel = -10f;

    private Dictionary<BubbleController, int> stockDictionary = new Dictionary<BubbleController, int>();
    private GameObject[] spawns;

    public static GameRules Instance {
        get; private set; 
    }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(this);
        }

        spawns = GameObject.FindGameObjectsWithTag("SpawnPoint");
    }

    private BubbleController[] GetAllPlayers() => FindObjectsByType<BubbleController>(FindObjectsSortMode.None);

    private void Start() {
        TimerManager.Instance.CreateTimer(matchTime, EndMatch, out matchTimeRemaining);

        stockDictionary = GetAllPlayers().ToDictionary(item => item, item => stocks);

        //TODO spawn the players at spawn points
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

        player.transform.position = spawns[Random.Range(0, spawns.Length)].transform.position;
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
}
