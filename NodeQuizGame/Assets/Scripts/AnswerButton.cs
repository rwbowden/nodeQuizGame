using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnswerButton : MonoBehaviour {
    public Text answerText;
    private AnswerData answerData;
    private GameController gameController;

	// Use this for initialization
	void Start () {
        answerText = GetComponentInChildren<Text>();
        gameController = FindObjectOfType<GameController>();
	}
	
    public void SetUp(AnswerData data)
    {
        answerData = data;
        answerText.text = data.answerText;
    }

    public void Clicked()
    {
        gameController.AnswerClicked(answerData.isCorrect);
    }
}
