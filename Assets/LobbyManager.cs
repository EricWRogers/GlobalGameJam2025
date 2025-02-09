using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : LobbyNetworkBase
{
    public GameObject playerUIPrefab;
    public GameObject playerListContainer;
    public TMP_Text joinCodeText;

    public GameObject startButton;
    public string joinCode;

    private JankCodeBetweenScenes JankCodeBetweenScenes = JankCodeBetweenScenes.Instance;


    protected override void UpdateUI(Dictionary<ulong, PlayerDataNetworkContainer> playerDict)
    {
        base.UpdateUI(playerDict);

        foreach (var player in playerDict.Values)
        {
            Instantiate(playerUIPrefab, playerListContainer.transform);
        }
    }

}



