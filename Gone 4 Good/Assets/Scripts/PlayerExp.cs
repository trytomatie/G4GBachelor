using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PlayerExp : MonoBehaviour
{
    public int experience;
    public int[] levelCaps;
    public int level = 0;

    public UnityEvent<int> OnExperienceGained;

    public MMProgressBar experienceBar;
    public TextMeshProUGUI expText;
    public MMF_Player expFloatingText;

    void Start()
    {
        levelCaps = new int[10];
        levelCaps[0] = 16;
        for (int i = 1; i < levelCaps.Length; i++)
        {
            levelCaps[i] = levelCaps[i - 1] * 2;
        }
        AddExperience(0);
    }
    public void AddExperience(Vector3 pos, int amount)
    {
        expFloatingText.transform.position = pos + new Vector3(0,1,0);
        MMF_FloatingText floatingText = expFloatingText.GetFeedbackOfType<MMF_FloatingText>();
        floatingText.Value = $"+{amount} EXP";
        expFloatingText.PlayFeedbacks();
        AddExperience(amount);
    }
    public void AddExperience(int amount)
    {
        experience += amount;
        if (experience >= levelCaps[level])
        {
            LevelUp();
            level++;
            experience = 0;
        }
        OnExperienceGained.Invoke(amount);
        experienceBar.UpdateBar01((float)experience / levelCaps[level]);
        expText.text = $"Level {level}: {experience}/{levelCaps[level]}";
    }

    private void LevelUp()
    {
        level++;
        TalentManager.Instance.SkillPoints++;
        //Time.timeScale = 0.1f;
        GameUI.instance.LevelUp();
    }

}
