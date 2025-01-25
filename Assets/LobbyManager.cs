using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    public GameObject playerUIPrefab; 
    public GameObject playerListContainer; 

    private Dictionary<ulong, GameObject> playerUIInstances = new Dictionary<ulong, GameObject>();

    void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += AddPlayerToLobby;
        NetworkManager.Singleton.OnClientDisconnectCallback += RemovePlayerFromLobby;
    }

    void OnDisable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= AddPlayerToLobby;
        NetworkManager.Singleton.OnClientDisconnectCallback -= RemovePlayerFromLobby;
    }

    private void AddPlayerToLobby(ulong clientId)
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            if (!playerUIInstances.ContainsKey(client.Key))
            {
                GameObject playerUI = Instantiate(playerUIPrefab, playerListContainer.transform);

                

                playerUIInstances[client.Key] = playerUI;
            }
        }
    }

    private void RemovePlayerFromLobby(ulong clientId)
    {
        if (playerUIInstances.TryGetValue(clientId, out GameObject playerUI))
        {
            Destroy(playerUI);
            playerUIInstances.Remove(clientId);
        }
    }

    public void StartGame()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
        }
    }
}

