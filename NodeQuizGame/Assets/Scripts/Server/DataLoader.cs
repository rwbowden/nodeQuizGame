using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIO;

public class DataLoader : MonoBehaviour {

    SocketIOComponent socket;

	// Use this for initialization
	void Start () {
        socket = GameObject.Find("Server").GetComponent<SocketIOComponent>();
        socket.Emit("get data");
    }

    // Update is called once per frame
    void Update () {
		
	}
}
