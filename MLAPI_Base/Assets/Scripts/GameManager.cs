using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

using MLAPI;
using MLAPI.Transports.UNET;
using MLAPI.Spawning;
using System;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour{
    public GameObject goMain_Panel;
    public GameObject goCanvas;
    
    public TMP_InputField inputField_ipAdd;


    //Netowrk Vars
    public NetworkManager netman;
    public GameObject goServer;
    public GameObject goHost;
    public GameObject goClient;
    public string ip;

    public bool isClient;
    public bool isServer;
    public bool isHost;

    public bool gameRunning = false;

    public List<GameObject> clientList;

    // Start is called before the first frame update
    void Start(){
        Debug.Log("GameManager is starting...");
        Application.runInBackground = true;
        Application.targetFrameRate = 60;

        goMain_Panel = GameObject.Find("Panel_Main");
        goCanvas = GameObject.Find("Canvas");

        goMain_Panel.transform.position = new Vector2(Screen.width/2,Screen.height/2);

        //ip = "192.168.11.15";
        ip = "127.0.0.1"; //On same machine

    }

    // Update is called once per frame
    void Update(){
        if(gameRunning == true){

            if(isServer){
                if (NetworkManager.Singleton.ConnectedClients.Count > 0){
                    if(NetworkManager.Singleton.ConnectedClients.Count > clientList.Count){
                        //Add latest client to client list
                        ulong tId = NetworkManager.Singleton.ConnectedClientsList[NetworkManager.Singleton.ConnectedClients.Count-1].ClientId;
                        AddClient(NetworkManager.Singleton.ConnectedClientsList[NetworkManager.Singleton.ConnectedClients.Count - 1].PlayerObject.transform.gameObject, tId);
                    }
                }
                
            }
        }
    }



    public void AddClient(GameObject tClient, ulong tClientId){
        Debug.Log("Adding client "+tClientId+" to list");

        //Set Client Id
        tClient.GetComponent<NetPlayer2>().clientId.Value = tClientId;

        //Set Client Object Type
        tClient.GetComponent<NetPlayer2>().objectType.Value = "Client";

        clientList.Add(tClient);
    }

    public void Start_Server(){
        goMain_Panel.transform.position = new Vector2(-4000,-4000);

        isServer = true;
        isHost = false;
        isClient = false;

        //Handle new connections
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartServer();
        //Debug.Log("Starting Host...");

        goServer = Instantiate(Resources.Load("Prefabs/Server") as GameObject);
        goServer.GetComponent<NetworkObject>().Spawn();


        //Zoom camera in so we can see players better, add camera movement later so server admin can move around and look at different parts of map
        Camera.main.fieldOfView = 30.0f;


        gameRunning = true;
    }

    public void Start_Host() {
        goMain_Panel.transform.position = new Vector2(-4000, -4000);

        isServer = false;
        isClient = false;
        isHost = true;

        //Handle new connections
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartHost();

        goHost = Instantiate(Resources.Load("Prefabs/Player_Host") as GameObject);
        ulong tClientID = NetworkManager.Singleton.ServerClientId;
        goHost.GetComponent<NetworkObject>().SpawnAsPlayerObject(tClientID);
        goHost.name = "Player_Host";
        gameRunning = true;
    }

    public void Start_Client(){
        if(inputField_ipAdd.text.Length > 0){
            ip = inputField_ipAdd.text;
        }
        else{
            ip = "127.0.0.1";
        }

        goMain_Panel.transform.position = new Vector2(-4000, -4000);

        isClient = true;
        isServer = false;
        isHost = false;

        netman.GetComponent<UNetTransport>().ConnectAddress = ip;
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes("room password");
        netman.GetComponent<NetworkManager>().StartClient();

        //Only Server can spawn objects.
        gameRunning = true;

    }

    private void ApprovalCheck(byte[] connectionData, ulong clientId, MLAPI.NetworkManager.ConnectionApprovedDelegate callback) {

        Debug.Log("Client " + clientId + " is joining server with password: "+ System.Text.Encoding.ASCII.GetString(connectionData));

        ////Your logic here
        bool approve = true;
        bool createPlayerObject = true;

        //// The prefab hash. Use null to use the default player prefab
        //// If using this hash, replace "MyPrefabHashGenerator" with the name of a prefab added to the NetworkPrefabs field of your NetworkManager object in the scene
        ulong? prefabHash = NetworkSpawnManager.GetPrefabHashFromGenerator("Player_Client");

        ////If approve is true, the connection gets added. If it's false. The client gets disconnected
        callback(createPlayerObject, prefabHash, approve, new Vector3(0,1,0), new Quaternion());
    }

    public void Exit_Game() {
        Destroy(goServer);
        Application.Quit();
    }


}

