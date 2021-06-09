using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class PlayerSpawnSystem : NetworkBehaviour
{
    [SerializeField] private GameObject[] playerPrefabs = null;
    public int[] playerSelections = new int[8];

    private static List<Transform> spawnPoints = new List<Transform>();

    private int nextIndex = 0;

    public static void AddSpawnPoint(Transform transform)
    {
        spawnPoints.Add(transform);

        spawnPoints = spawnPoints.OrderBy(x => x.GetSiblingIndex()).ToList();
    }
    public static void RemoveSpawnPoint(Transform transform) => spawnPoints.Remove(transform);

    public override void OnStartServer() => NetworkManagerLobby.OnServerReadied += SpawnPlayer;

    public override void OnStartClient()
    {
        InputManager.Add("Player");
        InputManager.Controls.Player.Look.Enable();
        InputManager.Controls.Player.Sensitivity.Enable();
        InputManager.Controls.Player.Move.Enable();
        InputManager.Controls.Player.Fire.Enable();
        InputManager.Controls.Player.Jump.Enable();
        InputManager.Controls.Player.Weapon1.Enable();
        InputManager.Controls.Player.Weapon2.Enable();
        InputManager.Controls.Player.Ability1.Enable();
        InputManager.Controls.Player.Ability2.Enable();
        
    }

    [ServerCallback]
    private void OnDestroy() => NetworkManagerLobby.OnServerReadied -= SpawnPlayer;

    [Server]
    public void SpawnPlayer(NetworkConnection conn)
    {
        Transform spawnPoint = spawnPoints.ElementAtOrDefault(nextIndex);

        if (spawnPoint == null)
        {
            Debug.LogError($"Missing spawn point for player {nextIndex}");
            return;
        }



        int agentForSpawn = playerSelections[nextIndex];
        GameObject playerInstance = Instantiate(playerPrefabs[agentForSpawn], spawnPoints[nextIndex].position, spawnPoints[nextIndex].rotation);


        NetworkServer.ReplacePlayerForConnection(conn, playerInstance, true);
        //NetworkServer.AddPlayerForConnection(conn, playerInstance);

        nextIndex++;
    }


    
    public void RespawnPlayer(GameObject playerObject) {

        
        playerObject.transform.position = findFarthestSpawn(playerObject).position;
        Debug.Log(findFarthestSpawn(playerObject).position);
        
        


    }

    private Transform findFarthestSpawn(GameObject playerObject)
    {
        List<float> finalComp = new List<float>();
        List<float> tempDistances = new List<float>();
        List<Transform> playerTransforms = new List<Transform>();

        NetworkGamePlayerLobby[] players = FindObjectsOfType<NetworkGamePlayerLobby>();

        for(int i = 0; i < players.Length; i++)
        {
            NetworkGamePlayerLobby player = players[i];

            if (player.isLocalPlayer)
            {
                break;
            }

            playerTransforms.Add(player.transform);
        }

        if (playerTransforms.Count == 0)
        {
            return spawnPoints.ElementAtOrDefault(nextIndex);
        }


        for(int outer = 0; outer < spawnPoints.Count; outer++)
        {
            for(int counter = 0; counter < playerTransforms.Count; counter++)
            {
               

                tempDistances.Add(Vector3.Distance(playerTransforms[counter].position, transform.position));

            }

            float nearestDistance = tempDistances.Min();

            finalComp.Add(nearestDistance);

        }

        int farthestSpawnIndex = finalComp.IndexOf(finalComp.Max());

        return spawnPoints[farthestSpawnIndex];
        
        
    }








}
