using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadHighScore : MonoBehaviour {
    [Range(1, 10)]
    public int rank = 1;
    public Text text;

    DataController dataController;

	// Use this for initialization
	void Start () {
        dataController = FindObjectOfType<DataController>();

        if (dataController.highScoreData.Length > rank)
        {
            HighScoreData score = dataController.highScoreData[rank - 1];
            text.text = score.initials + ": " + score.score;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
