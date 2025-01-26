using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerData : NetworkBehaviour
{
    public List<GameObject> lives = new List<GameObject>();
    public List<GameObject> stockIconList = new List<GameObject>();
    public int stocks;
    [HideInInspector]
    public int color;

    public GameObject livesContainer;

    ulong clientId;
    bool isLocal = true;
    private void Start()
    {
        clientId = gameObject.GetComponentInParent<NetworkObject>().OwnerClientId;

        

        if (NetworkManager.Singleton)
            isLocal = NetworkManager.Singleton.LocalClientId == clientId;


        if (isLocal)
        {
            for (int i = 0; lives.Count < stocks; i++)
            {
                lives.Add(stockIconList[color]);
                Instantiate(lives[i], livesContainer.transform);
            }
        }
        else
        {
            GetComponentInChildren<Canvas>().gameObject.SetActive(false);
        }
    }


    private void Update()
    {
        if (isLocal)
        {
            if (lives.Count != stocks)
            {
                lives.RemoveAt(lives.Count - 1);
            }
        }
    }

}
