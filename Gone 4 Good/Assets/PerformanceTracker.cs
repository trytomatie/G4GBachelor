using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PerformanceTracker : MonoBehaviour
{
    // Singelton
    public static PerformanceTracker instance;
    private float startTime;

    public PerformanceStack currentStack;
    public List<PerformanceStack> performanceStacks = new List<PerformanceStack>();
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

    public static void StartNewStack(string stackName, string playerName,string description)
    {
        instance.currentStack = new PerformanceStack();
        instance.startTime = Time.time;
        instance.currentStack.stackName = stackName;
        instance.currentStack.playerName = playerName;
        instance.currentStack.stackDescription = description;
    }

    public static void EndCurrentStack()
    {
        instance.currentStack.timeElapsed = Time.time - instance.startTime;
        // Save to file
        instance.performanceStacks.Add(instance.currentStack);
        instance.currentStack = new PerformanceStack();
        instance.SaveStackToFile();

    }

    public void SaveStackToFile()
    {
        string path = Application.persistentDataPath;
        if (!System.IO.Directory.Exists(path))
        {
            System.IO.Directory.CreateDirectory(path);
        }
        string fileName = "PerformanceTest" +".json";
        string json = JsonConvert.SerializeObject(performanceStacks, Formatting.Indented);
        print("Json: " + json);
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
public class PerformanceStack
{
    public float timeElapsed;
    public string playerName;
    public string stackName;
    public string stackDescription;

    public int shootsFired;
    public int shootsHit;
    public int headShots;
    public float custom;
}
