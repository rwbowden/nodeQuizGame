using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

    private int playerScore;
    private float timer;

    public Text questionText;
    public Text scoreText;
    public Text timeText;

    public GameObject questionDisplay;
    public GameObject endGameDisplay;

    private DataController dataController;
    private RoundData roundData;
    private QuestionData[] questionData;
    private bool isRoundActive;
    private int questionIndex = 0;

    public Transform answersPanel;

    private BasicObjectPool answerButtonPool;
    private List<GameObject> answerButtonObjects = new List<GameObject>();


    void Awake()
    {

    }

    // Use this for initialization
    void Start() {

        dataController = FindObjectOfType<DataController>();
        roundData = dataController.GetCurrentRoundData();
        answerButtonPool = FindObjectOfType<BasicObjectPool>();
        questionData = roundData.questions;
        ShowQuestions();
        playerScore = 0;
        timer = roundData.timeLimitInSeconds;
        isRoundActive = true;
    }

    void Update()
    {
        if(isRoundActive)
            UpdateTime();
    }

    private void ShowQuestions()
    {
        RemoveAnswerButtons();
        QuestionData question = questionData[questionIndex];
        questionText.text = question.questionText;
        for (int i = 0; i < question.answers.Length; i++)
        {
            GameObject answer = answerButtonPool.GetObject();

            answer.transform.SetParent(answersPanel, false);
            answer.SetActive(true);
            answerButtonObjects.Add(answer);

            AnswerButton answerButton = answer.GetComponent<AnswerButton>();
            answerButton.SetUp(question.answers[i]);
        }

    }

    private void RemoveAnswerButtons()
    {
        while(answerButtonObjects.Count > 0)
        {
            answerButtonPool.ReturnObject(answerButtonObjects[0]);
            answerButtonObjects.RemoveAt(0);
        }
    }

    private void UpdateTime()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
            EndRound();

        string minutes = Mathf.Floor(timer / 60).ToString("00");
        string seconds = Mathf.Floor(timer % 60).ToString("00");

        timeText.text = "Timer: " + minutes + ":" + seconds;
    }

    public void EndRound()
    {
        isRoundActive = false;

        questionDisplay.SetActive(false);
        endGameDisplay.SetActive(true);

    }

    public void StartOver()
    {
        SceneManager.LoadScene("MenuScreen");
    }

    public void AnswerClicked(bool isCorrect)
    {
        // If correct answer
        if (isCorrect)
        {
            // Add to player score
            playerScore += roundData.pointsAddedForCorrectAnswer;
            scoreText.text = "Score: " + playerScore.ToString();
        }

        // If there are more questions
        if(questionData.Length > questionIndex + 1)
        {
            questionIndex++;
            ShowQuestions();
        }
        else // Last question
        {
            // End Game
            EndRound();
        }
    }
	
}
