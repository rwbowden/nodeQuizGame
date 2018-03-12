using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TestGameData))]
public class SendToServerEditor : Editor {

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TestGameData myScript = (TestGameData)target;
        if (GUILayout.Button("Send Data"))
        {
            myScript.SendGameData();
        }

        if (Application.isEditor)
            EditorGUILayout.HelpBox("Must be running the game", MessageType.Warning);
    }

}
