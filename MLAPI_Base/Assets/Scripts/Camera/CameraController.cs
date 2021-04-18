using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour{
    public GameObject player; //Player this camera follows, local player
    public float yOff;
    public float zOff;

    // Start is called before the first frame update
    void Start(){
        yOff = 12f;
        zOff = -5f;
    }

    // Update is called once per frame
    void LateUpdate(){
        if(player != null) {
            transform.position = new Vector3(player.transform.position.x, player.transform.position.y + yOff, player.transform.position.z + zOff);
        }
        
    }
}
