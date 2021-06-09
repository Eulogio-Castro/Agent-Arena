using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : NetworkBehaviour
{
    
    private Controls controls;
    private float speed = 5f;
    // Start is called before the first frame update
    public override void OnStartClient()
    {
        base.OnStartAuthority();
        controls = new Controls();
        controls.Enable();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
        controls.Disable();
    }

    [Client]
    // Update is called once per frame
    private void Update()
    {
        if (!hasAuthority) { return; }

        Vector2 move = controls.Player.Move.ReadValue<Vector2>();
        CmdMove(move);
        Debug.Log(move.ToString() + "1");
    }

    [Command]
    private void CmdMove(Vector2 move)
    {
        //Validate logic here

        RpcMove(move);
        Debug.Log(move.ToString() + "2");
    }


    [ClientRpc]
    private void RpcMove(Vector2 move)
    {
        Vector3 movement = new Vector3(move.x, 0, move.y);
        transform.Translate(movement*Time.deltaTime*speed);
        Debug.Log(move.ToString() + "3");
    }
}
