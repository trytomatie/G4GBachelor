using UnityEngine;

[CreateAssetMenu(fileName = "Talent", menuName = "ScriptableObjects/Talent")]
public class Talent : ScriptableObject
{
    public TalentType talentType;
    public int tier = 1;
    public string talentName;
    [TextArea(3, 10)]
    public string talentDescription;
    public Sprite talentIcon;
    public TalentFunctionality talentFunctionality;
}

