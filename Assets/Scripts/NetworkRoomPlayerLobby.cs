using System;
using System.Linq;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Steamworks;
using TMPro;
using UnityEngine.Rendering.Universal;

public class NetworkRoomPlayerLobby : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject lobbyMembersUI = null;
    [SerializeField] private GameObject AgentLoadoutUI = null;
    [SerializeField] private GameObject AgentLoadoutModels = null;
    [SerializeField] private TMP_Text[] playerNameTexts = new TMP_Text[8];
    [SerializeField] private TMP_Text[] playerReadyTexts = new TMP_Text[8];
    [SerializeField] private RawImage[] playerImages = new RawImage[8];
    [SerializeField] private Image[] playerAgentImages = new Image[8];
    [SerializeField] private Button startGameButton = null;
    [SerializeField] private GameObject selectedAgentPrefab;
    public GameObject[] agentPrefabs;
    public Texture2D[] agentImages;
    private Camera mainCamera;
    private UniversalAdditionalCameraData mainCamData;
    [SerializeField] private Camera loadoutCamera;
    
    public ulong steamID;

    protected Callback<AvatarImageLoaded_t> avatarImageLoaded;

    [SyncVar(hook = nameof(HandleDisplayNameChanged))]
    public string DisplayName = "Loading...";
    [SyncVar(hook = nameof(HandleReadyStatusChanged))]
    public bool IsReady = false;
    [SyncVar(hook = nameof(HandleAgentSelectedChanged))]
    public int selectedAgent;


    private bool isLeader;
    public bool IsLeader
    {
        set
        {
            isLeader = value;
            startGameButton.gameObject.SetActive(value);
        }
    }

    private NetworkManagerLobby room;
    private NetworkManagerLobby Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManagerLobby;
        }
    }

    public override void OnStartAuthority()
    {

        string steamName = SteamFriends.GetPersonaName();




        CmdSetDisplayName(steamName);
        if (isLocalPlayer)
        {
            AgentLoadoutUI.SetActive(true);
            AgentLoadoutModels.SetActive(true);
            lobbyMembersUI.SetActive(false);

        }



    }

    [Client]
    public void Start()
    {


        transform.position = new Vector3(0, 0, 0);
        mainCamera = Camera.main;
        mainCamData = mainCamera.GetUniversalAdditionalCameraData();

        Canvas canvas = GetComponentInChildren<Canvas>();
        canvas.worldCamera = mainCamera;
        mainCamData.cameraStack.Add(loadoutCamera);

        Room.RoomPlayers.Add(this);



        UpdateDisplay();
        
    }

    public override void OnStopClient()
    {
        //Room.RoomPlayers.Remove(this);

        UpdateDisplay();

    }

    public void HandleReadyStatusChanged(bool oldValue, bool newValue) => UpdateDisplay();
    public void HandleDisplayNameChanged(string oldValue, string newValue) => UpdateDisplay();

    public void HandleAgentSelectedChanged(int oldValue, int newValue) => UpdateDisplay();

    private void UpdateDisplay()
    {
        if (!isLocalPlayer)
        {
            foreach (var player in Room.RoomPlayers)
            {
                if (player.isLocalPlayer)
                {
                    player.UpdateDisplay();
                    break;
                }
            }

            return;
        }

        for (int i = 0; i < playerNameTexts.Length; i++)
        {
            playerNameTexts[i].text = "Waiting For Player...";
            playerReadyTexts[i].text = string.Empty;
        }

        for (int i = 0; i < Room.RoomPlayers.Count; i++)
        {
            playerNameTexts[i].text = Room.RoomPlayers[i].DisplayName;
            playerReadyTexts[i].text = Room.RoomPlayers[i].IsReady ?
                "Ready" :
                "Not Ready";

            playerReadyTexts[i].color = Room.RoomPlayers[i].IsReady ?
                Color.green :
                Color.red;
        }



        for (int i = 0; i < Room.RoomPlayers.Count; i++)
        {
            CSteamID csteamID = SteamMatchmaking.GetLobbyMemberByIndex(
                room.steamLobby.LobbyID,
                i);

            int imageID = SteamFriends.GetLargeFriendAvatar(csteamID);

            if (imageID == -1) { return; }
            playerImages[i].texture = GetSteamImageAsTexture(imageID);
        }


        for (int i = 0; i < Room.RoomPlayers.Count; i++)
        {
            Texture2D texture2D = agentImages[Room.RoomPlayers[i].selectedAgent];
            playerAgentImages[i].sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), Vector2.zero);
            selectedAgentPrefab = agentPrefabs[selectedAgent];
            

        }


    }

    public void HandleReadyToStart(bool readyToStart)
    {
        if (!isLeader) { return; }

        startGameButton.interactable = readyToStart;
    }

    [Command]
    private void CmdSetDisplayName(string displayName)
    {
        DisplayName = displayName;
    }

    [Command]
    public void selectAgent(int agentSelection)
    {
       
        selectedAgent = agentSelection;


    }


    [Command]
    public void CmdReadyUp()
    {
        IsReady = !IsReady;

        Room.NotifyPlayersOfReadyState();
    }

    [Command]
    public void CmdStartGame()
    {
        if (Room.RoomPlayers[0].connectionToClient != connectionToClient) { return; }

        Debug.Log("THIS IS WHERE WE START THE GAME");

        Room.StartGame();
    }



    private Texture2D GetSteamImageAsTexture(int iImage)
    {
        Texture2D texture = null;
        bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);

        if (isValid)
        {
            byte[] image = new byte[width * height * 4];

            isValid = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));

            if (isValid)
            {
                texture = new Texture2D(((int)width), (int)height, TextureFormat.RGBA32, false, true);
                texture.LoadRawTextureData(image);
                texture.Apply();
            }
        }

        return texture;
    }


    private void OnAvatarImageLoaded(AvatarImageLoaded_t callback)
    {
        if (callback.m_steamID.m_SteamID == steamID)
        {

            for (int i = 0; i < Room.RoomPlayers.Count; i++)
            {
                if (playerNameTexts[i].text == SteamFriends.GetPersonaName())
                {
                    steamID = SteamUser.GetSteamID().m_SteamID;
                    int imageID = SteamFriends.GetLargeFriendAvatar(new CSteamID(steamID));
                    playerImages[i].texture = GetSteamImageAsTexture(imageID);
                }
            }

        }
    }


    public void OpenLoadoutUI()
    {
        if (isLocalPlayer)
        {


            lobbyMembersUI.SetActive(false);
            AgentLoadoutUI.SetActive(true);
            AgentLoadoutModels.SetActive(true);
        }
    }

    public void CloseLoadoutUI()
    {
        if (isLocalPlayer)
        {
            lobbyMembersUI.SetActive(true);
            AgentLoadoutUI.SetActive(false);
            AgentLoadoutModels.SetActive(false);

        }
    }



}
