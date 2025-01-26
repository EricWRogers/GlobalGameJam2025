using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public GameObject playerUIPrefab;
    public GameObject playerListContainer;
    public TMP_Text joinCodeText;

    public GameObject startButton;
    public string joinCode;

    private JankCodeBetweenScenes JankCodeBetweenScenes = JankCodeBetweenScenes.Instance;

    private Dictionary<ulong, GameObject> playerUIInstances = new Dictionary<ulong, GameObject>();

    public List<GameObject> playerObjects = new List<GameObject>();


    void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += AddPlayerToLobby;
        NetworkManager.Singleton.OnClientDisconnectCallback += RemovePlayerFromLobby;
    }

    

    private void AddPlayerToLobby(ulong clientId)
    {
       
      ServerHandleLobbyUIServerRPC();
        
        


        int colorIndex = 0;
        
        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            if (!playerUIInstances.ContainsKey(client.Key))
            {
                GameObject playerUI = Instantiate(playerObjects[colorIndex], playerListContainer.transform);

                playerUIInstances[client.Key] = playerUI;



                colorIndex++;

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
            NetworkManager.Singleton.SceneManager.LoadScene("AidenAndEricTestScene", LoadSceneMode.Single);
        }
        else
        {
            startButton.SetActive(false); //Hide the button from scrubs.
        }
    }

    private void Start()
    {
        AddPlayerToLobby(NetworkManager.Singleton.LocalClientId);

        joinCodeText.text = JankCodeBetweenScenes.joinCode;

    }

    [ServerRpc(RequireOwnership = false)]
    void ServerHandleLobbyUIServerRPC()
    {
        int colorIndex = 0;
        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            if (!playerUIInstances.ContainsKey(client.Key))
            {
                GameObject playerUI = Instantiate(playerObjects[colorIndex], playerListContainer.transform);

                playerUIInstances[client.Key] = playerUI;



                colorIndex++;

            }
        }
    }

}

public class UIData
{
    public string playerId;
    public Image backgroundImage;
    public Image colorImage;
    public int ping;
}

