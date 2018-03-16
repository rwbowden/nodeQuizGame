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

    public Text endScoreText;
    public InputField initialField;

    public GameObject questionDisplay;
    public GameObject endGameDisplay;
    public GameObject highScoreDisplay;

    private DataController dataController;
    private RoundData roundData;
    private QuestionData[] questionData;
    private bool isRoundActive;
    private int questionIndex = 0;
    private int roundNum = 0;

    public Transform answersPanel;

    private BasicObjectPool answerButtonPool;
    private List<GameObject> answerButtonObjects = new List<GameObject>();


    void Awake()
    {

    }

    // Use this for initialization
    void Start() {

        dataController = FindObjectOfType<DataController>();
        roundData = dataController.GetCurrentRoundData(roundNum);
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
        questionText.text = question.questionText.ToLower();
        for (int i = 0; i < question.answers.Length; i++)
        {
            GameObject answer = answerButtonPool.GetObject();

            answer.transform.SetParent(answersPanel, false);
            answer.SetActive(true);
            answerButtonObjects.Add(answer);

            AnswerButton answerButton = answer.GetComponent<AnswerButton>();
            if(i == 0)
            {
                if (roundNum == 0)
                    questionText.font = answerButton.star;
                else
                    questionText.font = answerButton.lord;
            }
                
            answerButton.SetUp(question.answers[i], roundNum);
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
        if(roundNum < dataController.allRoundData.Length - 1)
        {
            roundNum++;
            roundData = dataController.GetCurrentRoundData(roundNum);
            questionIndex = 0;
            questionData = roundData.questions;
            ShowQuestions();
            timer = roundData.timeLimitInSeconds;
            isRoundActive = true;
        }
        else
        {
            isRoundActive = false;
            endScoreText.text = ": " + playerScore;

            initialField.Select();
            questionDisplay.SetActive(false);
            if (dataController.socket.socket.IsConnected)
                highScoreDisplay.SetActive(true);
            else
                endGameDisplay.SetActive(true);
        }


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

    public void SendScore(Text initials)
    {
        if (initials.text != "")
            dataController.playerScore.initials = initials.text;
        else
            dataController.playerScore.initials = "AAA";

        dataController.playerScore.score = playerScore;

        string jsonObj = JsonUtility.ToJson(dataController.playerScore);

        dataController.socket.Emit("send score", new JSONObject(jsonObj));

        StartOver();
    }

    public void CapString()
    {
        initialField.text = initialField.text.ToUpper();
    }
	
}
