using System;
using UnityEngine;

public class PerformanceTracker : MonoBehaviour
{
    // Singelton
    public static PerformanceTracker instance;

    public PerformanceStack currentStack;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public static void StartNewStack(string stackName, string playerName)
    {
        instance.currentStack = new PerformanceStack();
        instance.currentStack.startTime = DateTime.Now.TimeOfDay;
        instance.currentStack.stackName = stackName;
        instance.currentStack.playerName = playerName;
    }

    public static void EndCurrentStack()
    {
        instance.currentStack.endTime = DateTime.Now.TimeOfDay;
        // Save to file
        instance.SaveStackToFile();
        instance.currentStack = new PerformanceStack();
    }

    private void SaveStackToFile()
    {
        string path = Application.persistentDataPath + "/PerformanceStacks";
        if (!System.IO.Directory.Exists(path))
        {
            System.IO.Directory.CreateDirectory(path);
        }
        string fileName = currentStack.stackName +".json";
        string json = JsonUtility.ToJson(currentStack);
        // Write file replace if exists
        System.IO.File.WriteAllText(path + "/" + fileName, json);
    }

    public static void WriteToCurrentStack(bool shotHit, bool headShot)
    {
        if (instance.currentStack.stackName == null) return;
        instance.currentStack.shootsFired++;
        if (shotHit)
        {
            instance.currentStack.shootsHit++;
            if (headShot)
            {
                instance.currentStack.headShots++;
            }
        }
    }
}

[Serializable]
public struct PerformanceStack
{
    public TimeSpan startTime;
    public TimeSpan endTime;
    public string playerName;
    public string stackName;

    public int shootsFired;
    public int shootsHit;
    public int headShots;
    public float accuracy
    {
        get
        {
            if (shootsFired == 0) return 0;
            return (shootsHit / shootsFired) * 100;
        }
    }
    public float headShotPercentage
    {
        get
        {
            if (shootsHit == 0) return 0;
            return (headShots / shootsHit) * 100;
        }
    }
}
