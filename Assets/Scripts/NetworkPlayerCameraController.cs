using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;
using Cinemachine;
using UnityEngine.Animations.Rigging;

public class NetworkPlayerCameraController : NetworkBehaviour
{

    
    [Header("CameraSettings")]
    [SerializeField] private CinemachineVirtualCamera playerCam = null;
    [SerializeField] private CinemachineVirtualCamera deathCam = null;
    private CinemachineTransposer transposer;
    [SerializeField] private Transform camRotationTarget;
    public float XSensitivity = 2f;
    public float YSensitivity = 2f;
    [SerializeField] private float MinimumX = -90F;
    [SerializeField] private float MaximumX = 90F;
    [SerializeField] private bool settingsens = false;
    [Header("MovementSettings")]
    public float speed = 5;
    public float gravityMultiplier = 2f;
    public CharacterController charController;
    private Quaternion m_CharacterTargetRot;
    [SerializeField] private Quaternion m_CameraTargetRot;
    public Transform playerTransform;
    [SerializeField] private Quaternion lateChestRotation;
    [SerializeField] private Quaternion lateSpineRotation;
    public Vector3 hingeRotation = Vector3.zero;
    [SerializeField] private Health playerHealth;
    public bool isDead = false;




    private Controls controls;
    private Controls Controls
    {
        get
        {
            if (controls != null) { return controls; }
            return controls = new Controls();
        }
    }

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
    public override void OnStartClient() { }

    /// <summary>
    /// This is invoked on clients when the server has caused this object to be destroyed.
    /// <para>This can be used as a hook to invoke effects or do client specific cleanup.</para>
    /// </summary>
    public override void OnStopClient() { }

    /// <summary>
    /// Called when the local player object has been set up.
    /// <para>This happens after OnStartClient(), as it is triggered by an ownership message from the server. This is an appropriate place to activate components or functionality that should only be active for the local player, such as cameras and input.</para>
    /// </summary>
    public override void OnStartLocalPlayer() {
        
        Cursor.lockState = CursorLockMode.Locked;
        

        
        playerCam.gameObject.SetActive(true);

        enabled = true;



        transposer = playerCam.GetCinemachineComponent<CinemachineTransposer>();

        Controls.Player.Look.performed += ctx => UpdateLook(ctx.ReadValue<Vector2>());
        Controls.Player.Sensitivity.started += ctx => Sensitivity_performed(ctx.ReadValue<float>());


        m_CharacterTargetRot = playerTransform.localRotation;
        m_CameraTargetRot = camRotationTarget.localRotation;

    }




    /// <summary>
    /// This is invoked on behaviours that have authority, based on context and <see cref="NetworkIdentity.hasAuthority">NetworkIdentity.hasAuthority</see>.
    /// <para>This is called after <see cref="OnStartServer">OnStartServer</see> and before <see cref="OnStartClient">OnStartClient.</see></para>
    /// <para>When <see cref="NetworkIdentity.AssignClientAuthority"/> is called on the server, this will be called on the client that owns the object. When an object is spawned with <see cref="NetworkServer.Spawn">NetworkServer.Spawn</see> with a NetworkConnection parameter included, this will be called on the client that owns the object.</para>
    /// </summary>
    public override void OnStartAuthority() {

        
    }

    /// <summary>
    /// This is invoked on behaviours when authority is removed.
    /// <para>When NetworkIdentity.RemoveClientAuthority is called on the server, this will be called on the client that owns the object.</para>
    /// </summary>
    public override void OnStopAuthority() { }



    [ClientCallback]
    private void OnEnable() => Controls.Enable();

    [ClientCallback]
    private void OnDisable()=> Controls.Disable();

    [ClientCallback]
    private void Update() => clientUpdate();


    void clientUpdate()
    {
        if(playerHealth.health <= 0)
        {
            isDead = true;
            PlayerDeath();
            
        }

    }


    private void UpdateLook(Vector2 lookInput)
    {


        if (!hasAuthority || isDead) { return; }
   

        
        float yRot = lookInput.x * XSensitivity * 0.01f;
        float xRot = lookInput.y * YSensitivity * 0.01f;

        //Debug.Log("Camera Rotation: \n" + camRotationTarget.rotation.eulerAngles);
        //Debug.Log("MouseDelta input vector: \n" + lookInput);


        m_CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
        m_CameraTargetRot *= Quaternion.Euler(-xRot, 0f, 0f);


        m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

        

        

        playerTransform.localRotation = m_CharacterTargetRot;
        camRotationTarget.localRotation = m_CameraTargetRot;
        //Debug.Log("Camera Rotation after applying input:\n" + camRotationTarget.rotation.eulerAngles);



        if (Application.isFocused){
            Cursor.lockState = CursorLockMode.Locked;
        }
    }



    private Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

        angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }

    private Quaternion ClampRotationAroundXAxis(Quaternion q, float min, float max)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

        angleX = Mathf.Clamp(angleX, min, max);

        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }


    private void Sensitivity_performed(float setSense)
    {
        Debug.Log("SETTING SENSE" + setSense);
        if (!settingsens)
        {
            if (setSense > 0)
            {
                Debug.Log("Detected Sensitivity");
                IncreaseSensitivity();
                StartCoroutine(SettingSense());
            }

            if (setSense < 0)
            {
                Debug.Log("Detected Sensitivity");
                DecreaseSensitivity();
                SettingSense();
                StartCoroutine(SettingSense());
            }
        }
    }

    IEnumerator SettingSense()
    {
        settingsens = true;
        yield return new WaitForSecondsRealtime(.1f);
        settingsens = false;
    }

    [Client]
    public void IncreaseSensitivity()
    {

        XSensitivity += .1f;
        YSensitivity += .1f;
    }

    [Client]
    public void DecreaseSensitivity()
    {
        if (XSensitivity <= .1f || YSensitivity <= .1f) { return; }

        XSensitivity -= .1f;
        YSensitivity -= .1f;
    }

    

    private void PlayerDeath()
    {
        if (!isLocalPlayer) { return; }
        playerCam.gameObject.SetActive(false);
        deathCam.gameObject.SetActive(true);
    }

    public void PlayerSpawn()
    {
        if (!isLocalPlayer) { return; }
        playerCam.gameObject.SetActive(true);
        deathCam.gameObject.SetActive(false);
        camRotationTarget.localEulerAngles = Vector3.zero;
        isDead = false;
    }



}
