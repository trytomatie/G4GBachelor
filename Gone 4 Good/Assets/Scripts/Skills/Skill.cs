using Unity.VisualScripting;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class Skill : ScriptableObject
{
    public string skillName;
    public Sprite skillIcon;
    public float skillCooldown = 3;
    public float castTime = 1.5f;

    [HideInInspector]public float onEnterTime;
    public virtual void OnEnter(GameObject source)
    {

    }

    public virtual void OnUpdate(GameObject source)
    {

    }

    public virtual void OnExit(GameObject source)
    {

    }

    public virtual bool CheckSkillConditions(GameObject source)
    {
        return true;
    }

    //public float GetCooldown() 
    //{ 
    //    return Mathf.Clamp(skillColldownTimer + skillCooldown - Time.time,0,999);
    //}

    public void AssignSkill(GameObject source,int index)
    {
        Skill instance = Instantiate(this);
        source.GetComponent<PlayerController>().skills[index] = instance;
        GameUI.instance.skillslots[index].SetupSkill(instance);
    }

    public void RemoveSkill(GameObject source,int index)
    {
        source.GetComponent<PlayerController>().skills[index] = null;
        GameUI.instance.skillslots[index].SetupSkill(null);
    }

}