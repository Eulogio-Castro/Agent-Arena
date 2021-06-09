using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Health : NetworkBehaviour
{

    [SyncVar(hook = nameof(HandleHealthChanged))]
    public float health = 100f;

    public float maxHealth = 100;
    public bool overHeal;
    public float overHealAmount = 20f;
    public float overHealMax = 30f;
    public bool overHealLasting;
    [SerializeField] private float respawnTime = 5f;
    [SerializeField] private PlayerSpawnSystem spawnSystem;
    [SerializeField] private NetworkPlayerCameraController cameraController;
    [SerializeField] private NetworkWeaponSystem weaponSystem;

    [SerializeField] private HealthBarFill healthBar;
    [SerializeField] private KDManager manager;


    
    [SerializeField] bool isDamagable;

    public static event Action OnHealthChanged;



    private NetworkManagerLobby room;
    private NetworkManagerLobby Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManagerLobby;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        spawnSystem = FindObjectOfType<PlayerSpawnSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void HandleHealthChanged(float oldValue, float newValue) => ChangeHealth();


    [Server]
    public void AddHealth(float addValue)
    {
        health = Mathf.Min(health + addValue, maxHealth);
    }




    [Server]
    public void RemoveHealth(float removeValue)
    {
        health = Mathf.Max(health - removeValue, 0);

    }


    public void ChangeHealth()
    {
        Debug.Log("HandleHealthChanged");
        OnHealthChanged?.Invoke();
        healthBar.updateHealthUI(health);

        if(health <= 0)
        {
            if (isLocalPlayer) 
            {
                StartCoroutine(RespawnTimer());
            }

        }
    }


    IEnumerator RespawnTimer()
    {
        yield return new WaitForSeconds(respawnTime);
        Debug.Log("RespawnTimerEnd");
        spawnSystem.RespawnPlayer(this.gameObject);
        startRPC();
            
    }

    private void startRPC()
    {
        weaponSystem.PlayerSpawn();
        cameraController.PlayerSpawn();
        manager.AddDeath();
        health = maxHealth;
        healthBar.updateHealthUI(health);
    }



    [Command]
    public void cmdSendHealth()
    {
        
        Health[] healths = FindObjectsOfType<Health>();

        foreach(Health health in healths)
        {
            if (health.isServer)
            {
                health.ChangeHealth();
            }
        }

    }
}
