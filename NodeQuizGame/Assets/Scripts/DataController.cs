using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using SocketIO;
using System.IO;

public class DataController : MonoBehaviour
{

    public RoundData[] allRoundData;

    SocketIOComponent socket;

    string gameDataFilePath = "/StreamingAssets/data.json";


    // Use this for initialization
    void Start()
    {

        socket = GameObject.Find("Server").GetComponent<SocketIOComponent>();
        DontDestroyOnLoad(gameObject);
        socket.socket.Connect();

        if(socket.socket.IsConnected)
            socket.On("loaded", OnLoad);
        else
       {
            LoadFromFile();
        }
        SceneManager.LoadScene("MenuScreen");
    }

    private void LoadFromFile()
    {
        string filePath = Application.dataPath + gameDataFilePath;
        GameData editorData;

        if (File.Exists(filePath))
        {
            string gameData = File.ReadAllText(filePath);
            editorData = JsonUtility.FromJson<GameData>(gameData);
            allRoundData = editorData.allRounds.ToArray();
        }
    }

    private void OnLoad(SocketIOEvent e)
    {
        GameData editorData = JsonUtility.FromJson<GameData>(e.data.ToString());

        SaveToFile(editorData);

        print(e.data.ToString());
        allRoundData = editorData.allRounds.ToArray();
    }

    private void SaveToFile(GameData editorData)
    {
        string jsonObj = JsonUtility.ToJson(editorData);

        string filePath = Application.dataPath + gameDataFilePath;

        File.WriteAllText(filePath, jsonObj);
    }

    public RoundData GetCurrentRoundData()
    {

        return allRoundData[0];
    }

}
