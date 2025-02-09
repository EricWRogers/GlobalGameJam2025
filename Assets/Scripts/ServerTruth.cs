using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/* This script is the SUPER server script. Essentially, it will be used to keep up with that the server is doing and serve as a way to get information for the clients.
 * 
 * It is the one truth.
 * Praise be to the Server.
 * 
 */
public class ServerTruth : NetworkBehaviour
{
    public static ServerTruth Instance { get; private set; }

    public Dictionary<ulong, PlayerData> connectedPlayers { get; private set; } //List of connected players and stuff about em.
    public enum ServerState
    {
        Prep, //Game setup
        Idle, //Not gameplay but something else.
        Ready, //The server is up and running, the network is healthy, and we are ready for the clients to speak to us.
        Closing //Closing down the server
    }

    private ServerState currentState;

    private void Awake() //This ensures ONLY the server has one of these bad boys. The clients will have to make RPC's into this singleton it.
    {
        
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        if (IsServer)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start() //Right as we start up, go into prep phase.
    {
       
            currentState = ServerState.Prep;
            HandleState();
        
    }

    public void SetState(ServerState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            HandleState();
        }
    }

    private void Update()
    {

        SceneManager.sceneUnloaded += OnSceneUnloaded; //Checking if we are preparing to leave a scene. We should so some clean up just in case.

    }

    private void OnSceneUnloaded(Scene scene)
    {
        
            SetState(ServerState.Prep);
        
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
            SpawnNetworkObjects();
        
    }

    private void HandleState()
    {

        switch (currentState)
        {
            case ServerState.Prep:
                Debug.Log("Server is in Prep state. Preparing...");
                SceneManager.sceneLoaded += OnSceneLoaded; 

                SetState(ServerState.Ready);
                break;

            case ServerState.Idle:
                Debug.Log("Server is in Idle state. Waiting for players...");
                break;

            case ServerState.Ready:
                Debug.Log("Server is in Ready state. Running the game...");
                //CustomMessagingManager.SendNamedMessage("ServerReady", NetworkManager.Singleton.ConnectedClientsIds, new ClientRpcParams());

                ServerIsReadyClientRPC();


                break;

            case ServerState.Closing:
                Debug.Log("Server is in Closing state. Shutting down...");
                break;
        }
    }

    private void SpawnNetworkObjects() //Spawns all networkobjects.
    {
        NetworkObject[] networkObjects = FindObjectsOfType<NetworkObject>();

        foreach (NetworkObject networkObject in networkObjects)
        {
            if (!networkObject.IsSpawned)
            {
                networkObject.Spawn();
                Debug.Log($"Spawned NetworkObject: {networkObject.gameObject.name}");
            }
        }
    }

    


    //--------------------------------------------- All of these are tools the clients will invoke with a ServerRPC to get specific game info. The clientbase class will be useful.
    public bool IsServerReady() //Quick way to figure out if the server is good or not.
    {
        return currentState == ServerState.Ready;
    }

    [ClientRpc]
    private void ServerIsReadyClientRPC() //Don't touch. This tells ALL clients they can start sending RPC's
    {
        ClientBase[] allClientBases = FindObjectsByType<ClientBase>(FindObjectsSortMode.None);
        foreach (var clientBase in allClientBases)
        {
            clientBase.isReady = true;
        }
    }
    public PlayerData PlayerDataConstruction(ulong clientId) //Constructs a player for joining players. Players will request what they should look like.
    {
        
        List<ulong> connectedClients = new List<ulong>(NetworkManager.Singleton.ConnectedClientsIds);
        int index = connectedClients.IndexOf(clientId);

        ulong id = clientId;
        string playerName = "Player " + (index + 1);
        Color playerColor = GetPlayerColor(index);
        int stocks = 3;
       
        PlayerData data = new PlayerData(id, playerName, playerColor, stocks);

        connectedPlayers.Add(clientId, data); 
        return data;
    }

    
    private Color GetPlayerColor(int index)
    {
        switch (index)
        {
            case 0: return Color.blue;
            case 1: return Color.red;
            case 2: return Color.yellow;
            case 3: return Color.green;
            default: return Color.white; 
        }
    }

    public Dictionary<ulong, PlayerData> GetAllPlayers()
    {
        return connectedPlayers;
    }

}



