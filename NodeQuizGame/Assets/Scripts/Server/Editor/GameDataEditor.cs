using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using SocketIO;
using System;

public class GameDataEditor : EditorWindow
{

    string gameDataFilePath = "/StreamingAssets/data.json";

    public GameData editorData;

    DataController dataController;

    GameObject server;
    public SocketIOComponent socket = null;

    [MenuItem("Window/Game Data Editor")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(GameDataEditor)).Show();

    }

    void OnGUI()
    {

        if (editorData != null)
        {
            // Display data from json
            SerializedObject serializedObject = new SerializedObject(this);
            SerializedProperty serializedProperty = serializedObject.FindProperty("editorData");
            EditorGUILayout.PropertyField(serializedProperty, true);
            serializedObject.ApplyModifiedProperties();


            if (GUILayout.Button("Save Game Data"))
            {
                SaveGameData();
            }

            if (GUILayout.Button("Send Game Data"))
            {
                SendGameData();

            }
        }

        if (GUILayout.Button("Load Game Data"))
        {
           
            server = GameObject.Find("Server");
            socket = server.GetComponent<SocketIOComponent>();

            socket.socket.Connect();

            socket.On("loaded", OnLoad);

            LoadGameData();

        }
        if (Application.isEditor)
            EditorGUILayout.HelpBox("Must be running game to load", MessageType.Warning);


    }

    void LoadGameData()
    {
        socket.Emit("get data");

    }

    private void OnLoad(SocketIOEvent e)
    {
        editorData = JsonUtility.FromJson<GameData>(e.data.ToString());
        dataController.allRoundData = editorData.allRounds.ToArray();

    }

    void Update()
    {
    }

    void SaveGameData()
    {
        string jsonObj = JsonUtility.ToJson(editorData);

        string filePath = Application.dataPath + gameDataFilePath;

        File.WriteAllText(filePath, jsonObj);
    }

    void SendGameData()
    {
        server = GameObject.Find("Server");
        socket = server.GetComponent<SocketIOComponent>();

        string jsonObj = JsonUtility.ToJson(editorData);

        socket.Emit("send data", new JSONObject(jsonObj));
    }
}
