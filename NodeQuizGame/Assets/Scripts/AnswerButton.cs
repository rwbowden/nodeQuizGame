using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnswerButton : MonoBehaviour {
    public Text answerText;
    private AnswerData answerData;
    private GameController gameController;

    public Font star;
    public Font lord;

	// Use this for initialization
	void Start () {
        answerText = GetComponentInChildren<Text>();
        gameController = FindObjectOfType<GameController>();
	}
	
    public void SetUp(AnswerData data, int roundNum)
    {
        if (roundNum == 0)
            answerText.font = star;
        else
            answerText.font = lord;

        answerData = data;
        answerText.text = data.answerText.ToLower();
    }

    public void Clicked()
    {
        gameController.AnswerClicked(answerData.isCorrect);
    }
}
