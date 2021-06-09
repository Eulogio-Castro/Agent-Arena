using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour
{

    [SerializeField] private NetworkManagerLobby networkManager = null;


    [Header("UI")]
    [SerializeField] private GameObject AgentSelectPanel= null;


    private void OnEnable()
    {
        NetworkManagerLobby.OnClientConnected += HandleClientConnected;
        NetworkManagerLobby.OnClientDisconnected += HandleClientDisconnected;
    }

    private void OnDisable()
    {
        NetworkManagerLobby.OnClientConnected -= HandleClientConnected;
        NetworkManagerLobby.OnClientDisconnected -= HandleClientDisconnected;
    }

   
    private void HandleClientConnected()
    {
        gameObject.SetActive(false);
        AgentSelectPanel.SetActive(false);
    }

    private void HandleClientDisconnected()
    {
        
    }

}
