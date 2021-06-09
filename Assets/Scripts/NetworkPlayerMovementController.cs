using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class NetworkPlayerMovementController : NetworkBehaviour
{

    [Header("MovementSettings")]
    public float speed = 5;
    public float gravityMultiplier = 2f;
    public CharacterController charController;
    public float jumpForce = 10f;
    [SerializeField] private bool canDoubleJump = false;
    [SerializeField] private bool inSecondJump = false;
    private bool m_PreviouslyGrounded;
    public bool hasJumped = false;
    [SerializeField] private bool m_Jumping;
    public Vector2 oldInput = new Vector2(0f, 0f);
    public Vector3 moveVector = Vector3.zero ;
    private Quaternion m_CharacterTargetRot;
    private Quaternion m_CameraTargetRot;
    [SerializeField] private Animator animator;
    [SerializeField] private NetworkAnimator netAnimator;
    [SerializeField] private NetworkPlayerCameraController cameraController;
    [SerializeField] private AudioHandler audioHandler;
    public Transform playerTransform;
    private bool settingsens;
    private bool isDead = false;

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
        enabled = true;
    }



    /// <summary>
    /// This is invoked on behaviours that have authority, based on context and <see cref="NetworkIdentity.hasAuthority">NetworkIdentity.hasAuthority</see>.
    /// <para>This is called after <see cref="OnStartServer">OnStartServer</see> and before <see cref="OnStartClient">OnStartClient.</see></para>
    /// <para>When <see cref="NetworkIdentity.AssignClientAuthority"/> is called on the server, this will be called on the client that owns the object. When an object is spawned with <see cref="NetworkServer.Spawn">NetworkServer.Spawn</see> with a NetworkConnection parameter included, this will be called on the client that owns the object.</para>
    /// </summary>
    public override void OnStartAuthority() {
        InputManager.Controls.Player.Move.performed += ctx => UpdateMovement(ctx.ReadValue<Vector2>());
        InputManager.Controls.Player.Move.canceled += ctx => ResetMovement();
        InputManager.Controls.Player.Sensitivity.started += ctx => Sensitivity_performed(ctx.ReadValue<float>());
        InputManager.Controls.Player.Jump.performed += ctx => Jump_performed();
        animator = GetComponent<Animator>();
        cameraController = GetComponent<NetworkPlayerCameraController>();


    }

    private void Jump_performed()
    {
        Debug.Log("Jump Performed");
        if (!m_PreviouslyGrounded && m_Jumping)
        {
            if(!m_PreviouslyGrounded && canDoubleJump && !inSecondJump)
            {
                Debug.Log("In 2nd Jump");
                inSecondJump = true;
                StartCoroutine(AllowJump());
                moveVector = new Vector3(moveVector.x, jumpForce, moveVector.z);
                Debug.Log("Move Vector: " + moveVector);
            }
            Debug.Log("Jump Caught");
            return;
        }
        audioHandler.Play("JumpSound");
        m_Jumping = true;
        hasJumped = true;
        moveVector = new Vector3(moveVector.x, jumpForce, moveVector.z);
        StartCoroutine(AllowJump());
        animator.SetTrigger("Jump");
        netAnimator.SetTrigger("Jump");
    }

    private void Sensitivity_performed(float sensitivityInput)
    {
        if (!settingsens)
        {
            if (sensitivityInput > 0)
            {
                //Debug.Log("Detected Sensitivity");
                cameraController.IncreaseSensitivity();
                StartCoroutine(SettingSense());
            }

            if (sensitivityInput < 0)
            {
                //Debug.Log("Detected Sensitivity");
                cameraController.DecreaseSensitivity();
                SettingSense();
                StartCoroutine(SettingSense());
            }
        }
    }

    /// <summary>
    /// This is invoked on behaviours when authority is removed.
    /// <para>When NetworkIdentity.RemoveClientAuthority is called on the server, this will be called on the client that owns the object.</para>
    /// </summary>
    public override void OnStopAuthority() { }


    void FixedUpdate()
    {

        
        
    }

    [ClientCallback]
    private void Update() =>  Move();
    

    [Client]
    private void Move()
    {
        if (cameraController.isDead && isLocalPlayer) { return; }

        Vector3 right = charController.transform.right;
        Vector3 forward = charController.transform.forward;
        Vector3 up = charController.transform.up;


        Vector3 move = right.normalized * moveVector.x + up.normalized*moveVector.y + forward.normalized * moveVector.z;

        

        HandleGravity(moveVector);
        if(m_PreviouslyGrounded != charController.isGrounded)
        {
            audioHandler.Play("LandSound");
        }
        m_PreviouslyGrounded = charController.isGrounded;
        m_Jumping = !charController.isGrounded;




        animator.SetFloat("Horizontal", moveVector.x);
        animator.SetFloat("Vertical", moveVector.z);
        animator.SetBool("isGrounded", m_PreviouslyGrounded);

        move.x = (move.x*speed);
        move.z = (move.z*speed);

        //Debug.Log("Movement Vector: \n" + move);
        charController.Move(move * Time.deltaTime);
    }

    [Client]
    private void ResetMovement()
    {
        if (cameraController.isDead) { return; }
        oldInput = Vector2.zero;
        moveVector.x = oldInput.x;
        moveVector.z = oldInput.y;
        animator.SetFloat("Speed", oldInput.magnitude);
        //Debug.Log("WASD input vector: " + oldInput);
    }


    [Client]
    private void UpdateMovement(Vector2 moveInput)
    {
       // Debug.Log("WASD input vector:" + moveInput);
        if (cameraController.isDead) { return; }
        oldInput = moveInput;
        Vector3 moveDir = new Vector3(oldInput.x, moveVector.y, oldInput.y);
        animator.SetFloat("Speed", moveDir.magnitude);
        moveVector = moveDir;

    }

    [Client]
    private void HandleGravity(Vector3 moveDir)
    {
        if (hasJumped)
        {
            return;
        }

        Debug.DrawRay(transform.position + new Vector3(0, .85f, 0), Vector3.down, Color.blue, charController.height / 2f, false);
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position + new Vector3(0, .9f ,0), charController.radius, Vector3.down, out hitInfo,
                            charController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            
            moveDir.y = -2f; //Stick to ground force
            inSecondJump = false;
        }

        else
        {
            moveDir += Physics.gravity * gravityMultiplier * Time.deltaTime;
        }


        moveVector = moveDir;
    }

    [Client]
    IEnumerator SettingSense()
    {
        settingsens = true;
        yield return new WaitForSecondsRealtime(.1f);
        settingsens = false;
    }

    IEnumerator AllowJump()
    {
        yield return new WaitForSeconds(.25f);
        hasJumped = false;
    }


}
