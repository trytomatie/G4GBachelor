
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DayNightController : MonoBehaviour
{

    public int timeMultiplier = 120;
    public float normalizedDayTime;
    public float normalizedDayCycleTime; // The Time in the Current Cycle (Day-Cycle, Night-Cycle)
    public AnimationCurve sunIntensity;
    public AnimationCurve sunAngle;
    [SerializeField]
    public Gradient sunColor;
    public Gradient heightFogColorStart;
    public Gradient heightFogColorEnd;
    public float sunAngleOffset = -90;

    [Header("References")]
    public Light sun;
    public Transform sunCrane;

    private TimeSpan currentTime;

    public static DayNightController Instance { get; protected set; }

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {

            Instance = this;
        }
        DontDestroyOnLoad(this);
    }

    public static void SetTime(TimeSpan value)
    {
        Instance.currentTime = value;
    }

    // Update is called once per frame
    void Update()
    {
        //UpdateTime();
        UpdateDayNightCycle();
    }

    private int NormalziedTimeAnimatorHash = Animator.StringToHash("NormalizedTime");
    //private void UpdateTime()
    //{
    //    string ampm = (currentTIme.Hours % 24) < 12 ? "AM" : "PM";
    //    currentTIme = currentTIme.Add(TimeSpan.FromSeconds(Time.deltaTime * timeMultiplier));
    //    string hours = (currentTIme.Hours % 12).ToString("D2");
    //    if(ampm == "PM" && currentTIme.Hours % 12 == 0)
    //    {
    //        hours = "12";
    //    }
    //    GameUI.Instance.timeText.text = $"{hours}:{currentTIme.Minutes.ToString("D2")} {ampm}";
    //    GameUI.Instance.timeIconAnmiator.SetFloat(NormalziedTimeAnimatorHash, normalizedDayTime);
    //}
    private void UpdateDayNightCycle()
    {
        currentTime = currentTime.Add(TimeSpan.FromSeconds(Time.deltaTime * timeMultiplier));
        normalizedDayTime = (float)(((currentTime.TotalSeconds% 86400) / 86400f)+0.75f) % 1;
        bool isNight = normalizedDayTime < 0.5f;

        float angle = Mathf.Lerp(0f, 360f, normalizedDayTime);
        sunCrane.localRotation = Quaternion.Euler(0, angle, 0f);
        sun.transform.localRotation = Quaternion.Euler(sunAngle.Evaluate(normalizedDayTime), 0, 0f);
        sun.intensity = sunIntensity.Evaluate(normalizedDayTime);
        sun.color = sunColor.Evaluate(normalizedDayTime);
        //if (normalizedDayTime <= 0.6f)
        //{
        //    lensFlare.enabled = true;
        //}
        //else
        //{
        //    lensFlare.enabled = false;
        //}
    }

   



}

