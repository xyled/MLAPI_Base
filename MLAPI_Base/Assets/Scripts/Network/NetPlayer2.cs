using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.Spawning;
using System;


public class NetPlayer2 : NetworkBehaviour{

    public GameManager gameMan;

    public NetworkVariableVector3 Position = new NetworkVariableVector3(new NetworkVariableSettings {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });


    public float movespeed = 1.5f;
    public float runspeed = 2f;

    public Rigidbody _body;
    public NetworkVariableString objectType; //Stores wether a client, host, or server so I can make sure things are showing up correctly
    public NetworkVariableULong clientId;
    public Vector3 oldPosition;

    public NetworkVariableBool move = new NetworkVariableBool(new NetworkVariableSettings {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    public NetworkVariableBool running = new NetworkVariableBool(new NetworkVariableSettings {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    public NetworkVariableVector3 _inputs = new NetworkVariableVector3(new NetworkVariableSettings {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    public override void NetworkStart() {
        base.NetworkStart();
        Debug.Log("NetPlayer Network Start firing.");

        //Grab the Main script
        gameMan = GameObject.Find("GameManager").GetComponent<GameManager>();

        //Connect camera and main object to player
        if (IsClient && IsLocalPlayer) {
            CameraController cc = Camera.main.GetComponent<CameraController>();
            cc.player = this.transform.gameObject;

            if(IsClient && !IsHost){
                //Assign this client to main goClient if localplayer
                gameMan.goClient = this.transform.gameObject;
            }
        }
        else if (IsServer) {
            gameMan.goServer = this.transform.gameObject;
            objectType.Value = "Server";
        }

    }



    [ServerRpc]
    void SubmitMoveRequestServerRPC(bool tempMove, bool tempRunning, Vector3 tInputs) {
        move.Value = tempMove;
        running.Value = tempRunning;
        _inputs.Value = tInputs;
    }


    private void Update() {

        if (IsClient && IsLocalPlayer) {
            CheckMovement();
        }
        else if (IsServer) {

        }
    }

    private void FixedUpdate() {
        //This is where any rigidbody actions go
        if(IsClient){
            if (move.Value == true) {
                transform.forward = _inputs.Value;
                _body.MovePosition(_body.position + _inputs.Value * (movespeed * runspeed) * Time.fixedDeltaTime);
            }
        }

    }

    private void CheckMovement() {
        //Rigidbody and character controller dont work very well together
        bool tMove = false;
        bool tRunning = false;
        Vector3 tInputs = Vector3.zero;
        tInputs = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) { tInputs.z = 1; }
        else if (Input.GetKey(KeyCode.S)) { tInputs.z = -1; }
        if (Input.GetKey(KeyCode.D)) { tInputs.x = 1; }
        else if (Input.GetKey(KeyCode.A)) { tInputs.x = -1; }

        if (Input.GetKey(KeyCode.LeftShift)) {
            runspeed = 2f; //Runspeed is a multiplier!
            tRunning = true;
        }
        else {
            runspeed = 1.0f;
            tRunning = false;
        }

        if(tInputs != Vector3.zero){
            //Player is moving
            tMove = true;

            if (tInputs != oldPosition) {
                //Players position changed from old position, send info to server, we only want to send info when player has changed previous position, like moving forward then sideways, etc
                oldPosition = tInputs;
                SubmitMoveRequestServerRPC(tMove, tRunning, tInputs);
            }
            else if(tRunning != running.Value){
                //Player has either started running or stopped running, let server know
                SubmitMoveRequestServerRPC(tMove, tRunning, tInputs);
            }
        }
        else if(tMove == false && move.Value == true){
            //Player was moving but has stopped, send info to server
            oldPosition = tInputs;
            SubmitMoveRequestServerRPC(tMove, tRunning, tInputs);
        }
    }
}

