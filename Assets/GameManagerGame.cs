using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManagerGame : MonoBehaviour
{
    public GameObject playerPrefab; 
    public Transform spawnPoint;     

    public List<Transform> spawnPoints = new List<Transform>();

    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;
    }

    private void OnDisable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= SpawnPlayer;
    }

    private void Start()
    {
        SpawnPlayersForAllClients();
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

