using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using BioIK;
using System;

public class NetworkWeaponSystem : NetworkBehaviour

{
    [SerializeField] private Animator animator;
    [SerializeField] private AudioHandler audioHandler;
    [SerializeField] private Health playerHealth;
    [SerializeField] private KDManager manager;
    [SerializeField] private AbilityUI abilityUI;
    [SerializeField] private NetworkAnimator netAnimator;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private bool isBurst;
    [SerializeField] private float burstDelay = .15f;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float fireRate;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private GameObject[] weaponObject;
    [SerializeField] private Weapon[] weapons;
    [SerializeField] private int currentWeapon = 0;
    [SerializeField] private float weaponSwitchTime = .6f;
    [SerializeField] private HandTargets handTargets;
    [SerializeField] private GameObject hipsObject;
    [SerializeField] private BioIK.BioIK bioIK;
    [SerializeField] private bool activeAbility1 = false;
    [SerializeField] private bool activeAbility2 = false;
    [SerializeField] private float ability1Cooldown;
    [SerializeField] private float ability2Cooldown;
    [SerializeField] private float ablitity1Time = .75f;
    [SerializeField] private float throwSpeed = 60f;
    [SerializeField] private float ability2Time = .75f;
    [SerializeField] private float regenAmount = 20f;
    [SerializeField] private bool ability1Ready;
    [SerializeField] private bool ability2Ready;
    [SerializeField] private GameObject ability1Prefab;
    [SerializeField] private GameObject ability2Prefab;
    [SerializeField] private bool ability1Throwable = false;
    [SerializeField] private bool ability2Throwable = false;
    [SerializeField] private bool ability2IsRegen;
    [SerializeField] private int ownerNetID;


    private bool isFiring;
    private bool fireIsPressed;
    private bool canFire = true;
    private bool isDead = false;
    private bool doabil1 = false;
    private bool doabil2 = false;
    private bool undoSilence = false;
    private bool isSilenced = false;

    /// <summary>
    /// This is invoked for NetworkBehaviour objects when they become active on the server.
    /// <para>This could be triggered by NetworkServer.Listen() for objects in the scene, or by NetworkServer.Spawn() for objects that are dynamically created.</para>
    /// <para>This will be called for objects on a "host" as well as for object on a dedicated server.</para>
    /// </summary>
    public override void OnStartServer() { }

    /// <summary>
    /// Invoked on the server when the object is unspawned
    /// <para>Useful for saving object data in persistant storage</para>
    /// </summary>
    public override void OnStopServer() { }

    /// <summary>
    /// Called on every NetworkBehaviour when it is activated on a client.
    /// <para>Objects on the host have this function called, as there is a local client on the host. The values of SyncVars on object are guaranteed to be initialized correctly with the latest state from the server when this function is called on the client.</para>
    /// </summary>
    public override void OnStartClient() {

        weaponObject[0].SetActive(true);
        weaponObject[1].SetActive(false);
        animator.SetFloat("WeaponType", 0f);
        bioIK = hipsObject.GetComponent<BioIK.BioIK>();
        ability1Ready = true;
        ability2Ready = true;
        RpcupdateWeaponParameters();

    }

    /// <summary>
    /// This is invoked on clients when the server has caused this object to be destroyed.
    /// <para>This can be used as a hook to invoke effects or do client specific cleanup.</para>
    /// </summary>
    public override void OnStopClient() { }

    /// <summary>
    /// Called when the local player object has been set up.
    /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStartLocalPlayer()
    {
        enabled = true;
        InputManager.Controls.Player.Fire.performed += ctx => Fire_performed();
        InputManager.Controls.Player.Fire.canceled += Fire_canceled;
        InputManager.Controls.Player.Weapon1.performed += ctx => Weapon1_performed();
        InputManager.Controls.Player.Weapon2.performed += ctx => Weapon2_performed();
        InputManager.Controls.Player.Ability1.performed += ctx => Ability1_Performed();
        InputManager.Controls.Player.Ability2.performed += ctx => Ability2_Performed();
    }





    /// <summary>
    /// This is invoked on behaviours that have authority, based on context and <see cref="NetworkIdentity.hasAuthority">NetworkIdentity.hasAuthority</see>.
    /// <para>This is called after <see cref="OnStartServer">OnStartServer</see> and before <see cref="OnStartClient">OnStartClient.</see></para>
    /// <para>When <see cref="NetworkIdentity.AssignClientAuthority"/> is called on the server, this will be called on the client that owns the object. When an object is spawned with <see cref="NetworkServer.Spawn">NetworkServer.Spawn</see> with a NetworkConnection parameter included, this will be called on the client that owns the object.</para>
    /// </summary>
    public override void OnStartAuthority()
    {
        
        
        





    }




    /// <summary>
    /// This is invoked on behaviours when authority is removed.
    /// <para>When NetworkIdentity.RemoveClientAuthority is called on the server, this will be called on the client that owns the object.</para>
    /// </summary>
    public override void OnStopAuthority() { }

    [ClientCallback]
    private void Update() => clientUpdate();


    
     void clientUpdate()
     {
        if (isDead) { return; }

        if (fireIsPressed)
        {
            Debug.Log("*Calling Fire method on Server*");
            FireBullet();
            
        }

        if(playerHealth.health <= 0 && isLocalPlayer)
        {
            PlayerDeath();
        }

        animator.SetBool("Shooting", isFiring);


        if (undoSilence)
        {
            undoSilence = false;
            cmdUndoSilence();
        }


        if (doabil1)
        {
            doabil1 = false;
            CmdAbility1();
            if (!ability1Throwable)
            {
                ScanAbility();
            }
            

            animator.SetBool("Ability1", false);
            StartCoroutine(stallIK(1.2f));
            isFiring = false;
            bioIK.enabled = true;
            bioIK.ResetPosture(bioIK.Root);
            StartCoroutine(Ability1Cooldown());
        }

        if (doabil2)
        {
            
            doabil2 = false;
            CmdAbility2();
            if (!ability2IsRegen)
            {
                StartCoroutine(silenceCountdown());
            }

            animator.SetBool("Ability2", false);
            StartCoroutine(stallIK(1.0f));
            isFiring = false;
            bioIK.enabled = true;
            bioIK.ResetPosture(bioIK.Root);
            StartCoroutine(Ability2Cooldown());

        }

        



     }


private void Ability1_Performed()
    {


        if(!ability1Ready || !activeAbility1 || isDead)
        {
            return;
        }

        StartAbility1();
        if (!ability1Throwable)
        {
            StartCoroutine(ScanCountdown());
        }

    }

    private void Ability2_Performed()
    {

        if (!ability2Ready || !activeAbility2 || isDead)
        {
            return;
        }
        StartAbility2();

    }

    private void Weapon1_performed()
    {
        if(isDead) { return; }

        int nextWeapon = 0;


        if(nextWeapon == currentWeapon)
        {
            return;
        }

        
        currentWeapon = nextWeapon;

        weaponObject[0].SetActive(true);
        weaponObject[1].SetActive(false);
        animator.SetFloat("WeaponType", 0f);
        

        canFire = false;

        animator.SetBool("Switching", true);
        bioIK.enabled = false;
        StartCoroutine(WeaponSwitch());
        CmdUpdateWeapons();


    }

    private void Weapon2_performed()
    {
        if (isDead) { return; }

        int nextWeapon = 1;


        if (nextWeapon == currentWeapon)
        {
            return;
        }


        currentWeapon = nextWeapon;

        weaponObject[0].SetActive(false);
        weaponObject[1].SetActive(true);
        animator.SetFloat("WeaponType", 1f);
        

        canFire = false;

        animator.SetBool("Switching", true);
        bioIK.enabled = false;
        StartCoroutine(WeaponSwitch());
        CmdUpdateWeapons();
    }


    private void Fire_performed()
    {
        if (isDead) { return; }
        fireIsPressed = true;
        Debug.Log("Fire Action Performed: " + fireIsPressed);
    }

    private void Fire_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (isDead) { return; }
        animator.SetBool("Shooting", false);
        fireIsPressed = false;
    }

    //[Command]
    private void FireBullet()
    {

        Debug.Log("Running Method on Server");
        if (isFiring || !canFire) { return; }
        CmdBulletSpawn();
        Debug.Log(netIdentity.netId);
        animator.SetBool("Shooting", true);
        Debug.Log("Using AudioSource: " + weaponObject[currentWeapon].name + "Fire");
        StartCoroutine(Firing());
        if (isBurst)
        {
            StartCoroutine(DelayBurstBullet1());
        }
        

    }

    [Command]
    private void CmdBulletSpawn()
    {
        RpcfireFlash();
        GameObject bulletInstance = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        bulletInstance.GetComponent<Rigidbody>().velocity = bulletSpawnPoint.forward * bulletSpeed;
        bulletInstance.GetComponent<NetworkBulletCollision>().ownerID = ownerNetID;
        bulletInstance.GetComponent<NetworkBulletCollision>().bulletDamage = weapons[currentWeapon].GetDamage();
        NetworkServer.Spawn(bulletInstance, ClientScene.localPlayer.gameObject);
        RpcplaySound(weaponObject[currentWeapon].name + "Fire");
    }

    

    private void StartAbility1()
    {
        if (isFiring || !canFire) { return; }

        isFiring = true;
        ability1Ready = false;
        animator.SetBool("Ability1", true);
        abilityUI.Ability1Start();
        bioIK.enabled = false;
        StartCoroutine(Ability1Stall());
        bioIK.ResetPosture(bioIK.Root);

    }

    
    private void StartAbility2()
    {
        if (isFiring || !canFire) { return; }

        isFiring = true;
        ability2Ready = false;
        //INSERT ABILITY 2 STUFF
        animator.SetBool("Ability2", true);
        audioHandler.Play("Ability2Sound");
        abilityUI.Ability2Start();
        bioIK.enabled = false;
        StartCoroutine(Ability2Stall());

    }



    [ClientRpc]
    private void RpcfireFlash()
    {
        muzzleFlash.Play();
    }


    [ClientRpc]
    private void RpcplaySound(string soundName)
    {
        if (isSilenced) { return; }
        audioHandler.Play(soundName);
    }

    IEnumerator Firing()
    {
        isFiring = true;
        
        yield return new WaitForSecondsRealtime(fireRate);
        isFiring = false;
    }

    
    IEnumerator DelayBurstBullet1()
    {
        yield return new WaitForSeconds(burstDelay);
        CmdBulletSpawn();
        //AudioSource.PlayClipAtPoint(audioHandler.getClip(weaponObject[currentWeapon].name + "Fire"), transform.position);
        RpcplaySound(weaponObject[currentWeapon].name + "Fire");
        Debug.Log("Using AudioSource: " + weaponObject[currentWeapon].name + "Fire");
        StartCoroutine(DelayBurstBullet2());
    }


    IEnumerator DelayBurstBullet2()
    {
        yield return new WaitForSeconds(burstDelay);
        CmdBulletSpawn();
        //AudioSource.PlayClipAtPoint(audioHandler.getClip(weaponObject[currentWeapon].name + "Fire"), transform.position);
        RpcplaySound(weaponObject[currentWeapon].name + "Fire");
        Debug.Log("Using AudioSource: " + weaponObject[currentWeapon].name + "Fire");
    }

    IEnumerator WeaponSwitch()
    {


        

        yield return new WaitForSeconds(weaponSwitchTime);
        animator.SetBool("Switching", false);
        handTargets.SetHands(currentWeapon);
        
        
        
        StartCoroutine(stallIK(.7f));
       
    }

    IEnumerator stallIK(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        bioIK.enabled = true;
        bioIK.ResetPosture(bioIK.Root);
        canFire = true;
    }

    IEnumerator Ability1Stall()
    {
        isFiring = true;
        yield return new WaitForSeconds(ablitity1Time);
        doabil1 = true;
    }

    [Command]
    private void CmdAbility1()
    {
        if (ability1Throwable)
        {
            GameObject ability1Instance = Instantiate(ability1Prefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            ability1Instance.GetComponent<Rigidbody>().velocity = bulletSpawnPoint.forward * throwSpeed;

            NetworkBulletCollision nbc;
            ability1Instance.TryGetComponent<NetworkBulletCollision>(out nbc);
            if (nbc != null)
            {
                ability1Instance.GetComponent<NetworkBulletCollision>().ownerID = ownerNetID;
            }

            GrenadeScript grenade;
            ability1Instance.TryGetComponent<GrenadeScript>(out grenade);
            if (grenade != null)
            {
                ability1Instance.GetComponent<GrenadeScript>().ownerID = ownerNetID;
            }


            NetworkServer.Spawn(ability1Instance, gameObject);
            RpcplaySound("ThrowingSound");
        }
        else
        {
            RpcplaySound("Ability1Sound");
        }
        
    }

    
    private void ScanAbility()
    {

        Outline[] scannables;
        scannables = GameObject.FindObjectsOfType<Outline>();
        foreach (Outline scan in scannables)
        {
            scan.GetComponent<Outline>().enabled = true;
        }



    }


    IEnumerator Ability2Stall()
    {
       
        isFiring = true;
        yield return new WaitForSeconds(ability2Time);
        doabil2 = true;
    }

    IEnumerator silenceCountdown()
    {
        yield return new WaitForSeconds(10f);
        undoSilence = true;
    }


    [Command]
    private void CmdAbility2()
    {
        RpcplaySound("Ability2Sound");
        if (ability2IsRegen)
        { 
        
            if(playerHealth.health >= 100) 
            {
                playerHealth.health = 100f + playerHealth.overHealMax;
            }
            else
            {
                playerHealth.health += playerHealth.overHealMax;
            }
        }

        else
        {
            isSilenced = true;

        }
    }

    [Command]
    private void cmdUndoSilence()
    {
        isSilenced = false;
    }


    IEnumerator ScanCountdown()
    {
        //Outline Script to remain active for 7 seconds
        yield return new WaitForSeconds(7f);
        Outline[] scannables;
        scannables = GameObject.FindObjectsOfType<Outline>();
        foreach (Outline scan in scannables)
        {
            scan.GetComponent<Outline>().enabled = false;
        }

    }



    IEnumerator Ability1Cooldown()
    {
        yield return new WaitForSeconds(ability1Cooldown);
        ability1Ready = true;


    }

    IEnumerator Ability2Cooldown()
    {
        yield return new WaitForSeconds(ability2Cooldown);
        ability2Ready = true;


    }


    public void ResetAbilities()
    {
        ability1Ready = true;
        ability2Ready = true;
    }

    public float GetAbility1Cooldown()
    {
        return ability1Cooldown;
    }

    public float GetAbility2Cooldown()
    {
        return ability2Cooldown;
    }

    [ClientRpc]
    void RpcupdateWeaponParameters()
    {
        bulletPrefab = weapons[currentWeapon].GetBulletPrefab();
        bulletSpeed = weapons[currentWeapon].GetBulletSpeed();
        fireRate = weapons[currentWeapon].GetFireRate();
        muzzleFlash = weapons[currentWeapon].GetParticleSystem();
        muzzleFlash.gameObject.SetActive(true);
        isBurst = weapons[currentWeapon].GetIsBurst();
        ownerNetID = (int)gameObject.GetComponent<NetworkIdentity>().netId;
    }

    private void PlayerDeath()
    {
        bioIK.enabled = false;
        isDead = true;
        canFire = false;
        animator.SetBool("Death", true);

    }

    public void PlayerSpawn()
    {
        
        animator.SetBool("Death", false);
        bioIK.enabled = true;
        bioIK.ResetPosture(bioIK.Root);
        isDead = false;
        canFire = true;
        ResetAbilities();
        CmdUpdateWeapons();
    }

    [Command]
    private void CmdUpdateWeapons()
    {
        RpcupdateWeaponParameters();
    }

}
