using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TalentManager : MonoBehaviour
{
    public List<Talent> activeTalents = new List<Talent>();
    private int skillPoints = 0;

    public Talent[] talents;

    // Singelton
    public static TalentManager Instance { get; private set; }
    public int SkillPoints 
    { 
        get => skillPoints;
        set 
        { 
            skillPoints = value;
            GameUI.instance.skillPointsText.text = "Skillpoints: " +skillPoints.ToString();
        } 
    }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ActivateTalent(int index)
    {
        if(SkillPoints > 0)
        {
            Talent talent = talents[index];
            talent.talentFunctionality.ApplyTalentEffect(GameManager.Instance.player.GetComponent<StatusManager>());
            activeTalents.Add(talent);
            SkillPoints--;
        }
    }

    public void DeactivateTalent(int index)
    {
        Talent talent = talents[index];
        talent.talentFunctionality.RemoveTalentEffect(GameManager.Instance.player.GetComponent<StatusManager>());
        activeTalents.Remove(talent);
        SkillPoints++;
    }
}

public enum TalentType
{
    Hero,
    Mage,
    Warrior,
    Rogue,
}

