using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TalentDescriptionUI : MonoBehaviour
{
    public Image talentImage;
    public TextMeshProUGUI talentName;
    public TextMeshProUGUI talentDescription;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void SetTalent(Talent talent)
    {
        talentImage.sprite = talent.talentIcon;
        talentName.text = talent.talentName;
        talentDescription.text = string.Format(talent.talentDescription);
    }
}
