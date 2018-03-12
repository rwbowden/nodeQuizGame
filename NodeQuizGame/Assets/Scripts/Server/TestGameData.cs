using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using SocketIO;

public class TestGameData : MonoBehaviour {
    public GameData gameData;

    string gameDataFilePath = "/Streaming Assets/data.json";


    GameObject server;
    SocketIOComponent socket;

    DataController dataController;
	// Use this for initialization
	void Start () {
        dataController = FindObjectOfType<DataController>();
        DontDestroyOnLoad(this);


        //OnSave();
	}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            SendGameData();
        }
    }

    void OnConnected(SocketIOEvent e)
    {
        Debug.Log("Connected");
    }

    void OnSave()
    {
        gameData.allRounds.Clear();
        foreach (RoundData r in dataController.allRoundData)
        {
            gameData.allRounds.Add(r);
        }

        string jsonObj = JsonUtility.ToJson(gameData);

        string filePath = Application.dataPath + gameDataFilePath;
        File.WriteAllText(filePath, jsonObj);

        print(jsonObj);
        SendGameData();


    }

   public void SendGameData()
    {
        dataController = FindObjectOfType<DataController>();


        gameData.allRounds.Clear();

        foreach (RoundData r in dataController.allRoundData)
        {
            gameData.allRounds.Add(r);
        }

        server = GameObject.Find("Server");
        socket = server.GetComponent<SocketIOComponent>();

        string jsonObj = JsonUtility.ToJson(gameData);

        socket.Emit("send data", new JSONObject(jsonObj));
    }
}
