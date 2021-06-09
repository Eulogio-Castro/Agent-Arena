using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Mirror;

public class SteamLobby : MonoBehaviour
{
    [SerializeField] private GameObject buttons = null;
    private NetworkManagerLobby networkManager;
    private const string HostAddressKey = "HostAddress";
    public CSteamID LobbyID;

    //Listen for Callback for Steam Lobby Creation
    protected Callback<LobbyCreated_t> lobbyCreated;
    //Listen for Callback for Steam user requesting to joining lobby
    protected Callback<GameLobbyJoinRequested_t> lobbyJoinRequested;
    //Listen for Client joining lobby, initializes client through NetworkManager
    protected Callback<LobbyEnter_t> lobbyEntered;

    

    private void Start()
    {
        networkManager = GetComponent<NetworkManagerLobby>();

        //If Steam is not open on client's unit, returnw
        if (!SteamManager.Initialized) 
        {
            Debug.Log("SteamManager Not Found");
            return; }

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        lobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void HostLobby()
    {
        buttons.SetActive(false);

        //Create a Steam Lobby open to Friends Only, with the Max Connections defined in the Network Manager Object
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);

    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        //If the Steam Lobby creation was unsuccessful
        if(callback.m_eResult != EResult.k_EResultOK)
        {
            buttons.SetActive(true);
            return; 
        }

        networkManager.StartHost();

        LobbyID = new CSteamID(callback.m_ulSteamIDLobby);


        //Set our Lobby Data using the Lobby ID from the Created callback, "HostAddress", and our host's Steam ID
        SteamMatchmaking.SetLobbyData(LobbyID,
            HostAddressKey,
            SteamUser.GetSteamID().ToString());

        Debug.Log("SetLobbyData complete");

        CSteamID lobbyowner = SteamMatchmaking.GetLobbyOwner(new CSteamID(callback.m_ulSteamIDLobby));
        Debug.Log(lobbyowner.ToString());
    }


    private void OnLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);    
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (NetworkServer.active) { return; }

        string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);
        LobbyID = new CSteamID(callback.m_ulSteamIDLobby);

        networkManager.networkAddress = hostAddress;
        networkManager.StartClient();

        buttons.SetActive(false);
    }

    public void JoinLobby()
    {
        SteamFriends.ActivateGameOverlay("Friends");


    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
