using Newtonsoft.Json;
using System.IO;
using UnityEngine;

public class PerformanceEvaluationHandler : MonoBehaviour
{

    public PerformanceStack hostageSituation = new PerformanceStack();
    public PerformanceStack reactionTest = new PerformanceStack();
    public PerformanceStack survivalTest = new PerformanceStack();

    [Header("Bartle Test Score (Max Score: 60)")]
    public int bartleScore = 0;
    [Header("Hostage Situation (Max Score: 130")]
    public int hostageSituationScore = 0;
    [Header("Reaction Test (Max Score: 50")]
    public int reactionTestScore = 0;
    [Header("Survival Test (Max Score: 160")]
    public int survivalTestScore = 0;

    public static int ddaRating = 0;

    private void Start()
    {
        LoadPerformanceData();
        hostageSituationScore = EvaluateHostageSituation();
        reactionTestScore = EvaluateReactionTest();
        survivalTestScore = EvaluateSurvivalTest();
        FinalEvaluation(hostageSituationScore, reactionTestScore, survivalTestScore, bartleScore);

    }
    public void LoadPerformanceData()
    {
        string path = Application.persistentDataPath;
        string pathTutorial = path + "/PerformanceTest.json";
        string pathBartleTest = path + "/BartleTestResults.json";

        // Check if the file exists
        if (File.Exists(pathTutorial))
        {
            string dataAsJson = File.ReadAllText(pathTutorial);
            PerformanceStack[] loadedData = JsonConvert.DeserializeObject<PerformanceStack[]>(dataAsJson);
            hostageSituation = loadedData[0];
            reactionTest = loadedData[1];   
            survivalTest = loadedData[2];
        }
        else
        {
            Debug.LogError("Cannot load performance data!");
        }

        if (File.Exists(pathBartleTest))
        {
            string dataAsJson = File.ReadAllText(pathBartleTest);
            bartleScore = JsonUtility.FromJson<BartleTestResults>(dataAsJson).score;
        }
        else
        {
            Debug.LogError("Cannot load Bartle test data!");
        }
    }

    private int EvaluateHostageSituation()
    {
        // Calculate accuracy as a percentage (0-100 range)
        float accuracy = (float)hostageSituation.shootsHit / hostageSituation.shootsFired * 100;

        // Calculate headshot accuracy as a percentage (0-100 range)
        float headshotAccuracy = (float)hostageSituation.headShots / hostageSituation.shootsHit * 100;

        // Custom modifier, scaled inversely and normalized to max 20 points
        float maxCustomValue = Mathf.Lerp(0, 20, 1 - (hostageSituation.custom / 10));

        float accuracyWeight = 0.7f;
        float headshotWeight = 0.4f;
        float customWeight = 0.5f;
        // Max Score: 70 + 40 + 20 = 130

        // Calculate the final score
        float finalScore = (accuracy * accuracyWeight) + (headshotAccuracy * headshotWeight) + (maxCustomValue * customWeight);

        return (int)finalScore;
    }

    private int EvaluateReactionTest()
    {
        float timeElapsed = reactionTest.timeElapsed;
        int score = 0;
        switch (timeElapsed-0.5f)
        {
            case < 0.3f:
                score = 100;
                break;
            case < 0.5f:
                score = 90;
                break;
            case < 0.6f:
                score = 80;
                break;
            case < 0.8f:
                score = 70;
                break;
            case < 1f:
                score = 60;
                break;
            case < 1.5f:
                score = 50;
                break;
            case < 1.75f:
                score = 40;
                break;
            case < 2.5f:
                score = 30;
                break;
            case < 3.5f:
                score = 20;
                break;
            case < 5:
                score = 10;
                break;
            default:
                score = 0;
                break;
        
        }
        float scoreWeight = 0.5f;
        // Max Score: 50    
        return Mathf.RoundToInt(score * scoreWeight);
    }

    public int EvaluateSurvivalTest()
    {
        // Calculate accuracy as a percentage (0-100 range)
        float accuracy = (float)hostageSituation.shootsHit / hostageSituation.shootsFired * 100;

        // Calculate headshot accuracy as a percentage (0-100 range)
        float headshotAccuracy = (float)hostageSituation.headShots / hostageSituation.shootsHit * 100;

        //  SurvivalTimeTiers
        float survivalTimeMultiplier = 0;
        int survivalBonusScore = 0;
        switch(survivalTest.timeElapsed)
        {
            case < 30:
                survivalTimeMultiplier = 0.4f;
                survivalBonusScore = 10;
                break;
            case < 60:
                survivalTimeMultiplier = 0.5f;
                survivalBonusScore = 20;
                break;
            case < 90:
                survivalTimeMultiplier = 0.6f;
                survivalBonusScore = 30;
                break;
            case < 120:
                survivalTimeMultiplier = 0.7f;
                survivalBonusScore = 40;
                break;
            case < 150:
                survivalTimeMultiplier = 0.8f;
                survivalBonusScore = 50;
                break;
            case < 180:
                survivalTimeMultiplier = 1f;
                survivalBonusScore = 60;
                break;
            default:
                survivalTimeMultiplier = 0.3f;
                survivalBonusScore = 0;
                break;
        }

        float accuracyWeight = 0.7f;
        float headshotWeight = 0.4f;

        int finalScore = Mathf.RoundToInt(((accuracy * accuracyWeight) + (headshotAccuracy * headshotWeight) + (survivalBonusScore)) * survivalTimeMultiplier);
        // Max Score: 70 + 40 + 50 = 160
        return finalScore;
    }

    public void FinalEvaluation(int hostageSituationScore, int reactionTestScore, int survivalTestScore, int bartleTestScore)
    {
        int maxScore = 60+130+50+160; // 400
        int maxScoreAdjusted = maxScore - 100;
        int finalScore = (hostageSituationScore + reactionTestScore + survivalTestScore + bartleTestScore);
        PerformanceEvaluationHandler.ddaRating = Mathf.Clamp(Mathf.RoundToInt((float)finalScore / maxScoreAdjusted * 7),0,7);
        Debug.Log("Final Score: " + finalScore + " DDARating: " + ddaRating);

    }
}