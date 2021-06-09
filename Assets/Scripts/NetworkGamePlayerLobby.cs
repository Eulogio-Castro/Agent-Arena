using System;
using System.Linq;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkGamePlayerLobby : NetworkBehaviour
{

    [SyncVar]
    public string DisplayName = "Loading...";

    public int selectedAgent;
    [SerializeField] private GameObject[] agentPrefabs;


    private NetworkManagerLobby room;
    private NetworkManagerLobby Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManagerLobby;
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        

    }

    public override void OnStartClient()
    {
        DontDestroyOnLoad(gameObject);
        Room.GamePlayers.Add(this);
    }



    public override void OnStopClient()
    {
        Room.GamePlayers.Remove(this);
        base.OnStopClient();
    }

    [Server]
    public void SetDisplayName(string displayName)
    {
        this.DisplayName = displayName;
    }



}
