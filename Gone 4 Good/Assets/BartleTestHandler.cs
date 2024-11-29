using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class BartleTestHandler : MonoBehaviour
{
    // Json File
    public TextAsset bartleTestJson;
    public Questionnaire questionnaire;
    public bool isQuestionaireStarted = false;

    public int currentQuestion = 0;
    public TextMeshProUGUI questionCounter;
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI answer1Text;
    public TextMeshProUGUI answer2Text;
    public TextMeshProUGUI answer3Text;
    public TextMeshProUGUI answer4Text;
    [Header("Analysis")]
    public TextMeshProUGUI analysisTitle;
    public TextMeshProUGUI analysisText;

    public Toggle[] toggles;

    public CanvasGroup canvasGroupPrelude;
    public CanvasGroup canvasGroupQuestionnaire;
    public CanvasGroup canvasGroupAnalysis;

    public int score = 0;
    public int[] questionAnswerTypes = new int[4]; // 0 = Competitive 1 = Casual 2 = Teamplayer 3 = Explorer/Solo
    private float startTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // find the file
        if(bartleTestJson == null)
        {
            bartleTestJson = Resources.Load<TextAsset>("BartleTest.json");
        }
        ReadFile();
        SetUpQuestion(currentQuestion);
        StartCoroutine(FadeInCanvasGroup(canvasGroupPrelude, 3));
        startTime = Time.time;
    }

    private void Update()
    {
        if (startTime+3 < Time.time)
        {
            return;
        }
        if (!isQuestionaireStarted && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
        {
            StartQuestionaire();
        }
    }

    public void NextQuestion()
    {
        bool answered = false;
        int gainedScore = 3;
        int i = 0;
        foreach (Toggle toggle in toggles)
        {
            if (toggle.isOn)
            {
                questionAnswerTypes[i]++;
                answered = true;
                break;
            }
            gainedScore--;
            i++;
        }
        if(!answered)
        {
            return;
        }
        score += gainedScore;
        if (currentQuestion < questionnaire.questions.Count - 1)
        {
            currentQuestion++;
            SetUpQuestion(currentQuestion);
        }
        else
        {
            canvasGroupQuestionnaire.gameObject.SetActive(false);
            canvasGroupAnalysis.gameObject.SetActive(true);
            StartCoroutine(FadeInCanvasGroup(canvasGroupAnalysis, 3));
            analysisTitle.text = GetAnalysisTitle(score);
            analysisText.text = GetAnalysisText(score);
            // Save to File
            string path = Application.persistentDataPath;
            if (System.IO.Directory.Exists(path)) // Check if the folder exists
            {
                BartleTestResults result = new BartleTestResults();
                result.score = score;
                string json = JsonUtility.ToJson(result);
                System.IO.File.WriteAllText(path + "/BartleTestResults.json", json);
            }
            else
            {
                print("The specified folder does not exist.");
            }
        }
        foreach (Toggle toggle in toggles)
        {
            toggle.isOn = false;
        }
    }

    public void StartQuestionaire()
    {
        isQuestionaireStarted = true;
        canvasGroupPrelude.gameObject.SetActive(false);
        canvasGroupQuestionnaire.gameObject.SetActive(true);
        StartCoroutine(FadeInCanvasGroup(canvasGroupQuestionnaire, 3));
    }

    IEnumerator FadeInCanvasGroup(CanvasGroup canvasGroup, float duration)
    {
        float counter = 0;
        while (counter < duration)
        {
            counter += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, counter / duration);
            yield return null;
        }
    }

    IEnumerator FadeOutCanvasGroup(CanvasGroup canvasGroup, float duration)
    {
        float counter = 0;
        while (counter < duration)
        {
            counter += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1, 0, counter / duration);
            yield return null;
        }
        canvasGroup.gameObject.SetActive(false);
    }

    public void SetUpQuestion(int index)
    {
        questionText.text = questionnaire.questions[index].question;
        answer1Text.text = questionnaire.questions[index].answers[0];
        answer2Text.text = questionnaire.questions[index].answers[1];
        answer3Text.text = questionnaire.questions[index].answers[2];
        answer4Text.text = questionnaire.questions[index].answers[3];
        questionCounter.text = "Question " + (index + 1) + " of " + questionnaire.questions.Count;
    }

    public void ReadFile()
    {         
        questionnaire = JsonUtility.FromJson<Questionnaire>(bartleTestJson.text);
        Debug.Log(questionnaire.questions[0].question);
        Debug.Log(questionnaire.questions[0].answers[0]);
    }

    public void ShowAnalysis()
    {

    }

    public string GetAnalysisTitle(int score)
    {
        switch(score)
        {
            case > 50:
                return "Competitive Gamer";
            case > 35:
                return "Advanced Teamplayer";
            case > 30:
                return "Balanced Gamer";
            case > 20:
                return "Casual Gamer";
            case >= 0:
                return "Relaxed Gamer";
        }
        return "Error";
    }

    public string GetAnalysisText(int score)
    {
        switch (score)
        {
            case > 50:
                return "You’re at the top of your game! Your fast reactions, dedication to skill mastery, and strategic mindset show that you’re serious about improvement. You thrive under pressure and adapt quickly to changes, keeping one step ahead of the competition.";
            case > 35:
                return "Your team-oriented skills are strong! You’re comfortable communicating, sharing intel, and leading when needed, helping your team succeed. With a bit more focus on personal skills, you’d be an unstoppable teammate.";
            case > 25:
                return "You have a versatile playstyle, balancing personal skill with teamwork. You adapt well and enjoy playing competitively, while still having a casual approach. Whether solo or with a team, you can hold your own";
            case > 15:
                return "You play for the fun of it! While you might not focus heavily on drills or map knowledge, you enjoy each game as it comes. Your spontaneous playstyle is perfect for relaxed gaming sessions.";
            case >= 0:
                return "You prefer to play at your own pace, discovering and enjoying each game’s world. Strategy and drills aren’t a priority. You’re here for the journey and enjoy the surprises along the way.";
        }
        return "Error";
    }

    public void GoBackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}


[Serializable]
public class Questionnaire
{
    public List<Question> questions;
}

[Serializable]
public class Question
{
    public string question;
    public List<string> answers;
}

public class BartleTestResults
{
    public int score;
}
