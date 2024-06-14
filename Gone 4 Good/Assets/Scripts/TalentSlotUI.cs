using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TalentSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public Image talentIcon;
    public Image coloredImage;
    public Talent talentReference;

    public float pressTime;
    public float pressConfirmationTime = 3.5f;
    [Header("Press Animation Parameters")]
    public MMF_Player pressFeedback;
    public AnimationCurve coloredImageFill;
    public AnimationCurve shakeIntensity;

    private bool isPressing = false;

    private void Start()
    { 
        GetComponent<Button>().onClick.AddListener(() => UnlockTalent());
        SetTalent(talentReference);
    }

    private void Update()
    {
        HandlePressing();
    }
    public void SetTalent(Talent talent)
    {
        talentReference = talent;
        talentIcon.sprite = talent.talentIcon;
        coloredImage.sprite = talent.talentIcon;
    }

    public void UnlockTalent()
    { 
        TalentManager talentManager = TalentManager.Instance;
        if (talentManager.SkillPoints <= 0)
        {
            return;
        }
        int i = 0;
        foreach(Talent talent in talentManager.talents)
        {
            if(!talentManager.activeTalents.Contains(talent))
            {
                if (talent == talentReference)
                {
                    talentManager.ActivateTalent(i);
                    talentIcon.material = null;
                }
            }
            i++;
        }
    }

    private void HandlePressing()
    {
        if(isPressing && TalentManager.Instance.SkillPoints > 0)
        {
            pressTime += Time.unscaledDeltaTime;
            if(pressTime >= pressConfirmationTime)
            {
                UnlockTalent();
                pressTime = 0;
                pressFeedback.StopFeedbacks();
                isPressing = false;
                GameUI.instance.EndLevelUp();


            }
            else
            {
                coloredImage.fillAmount = coloredImageFill.Evaluate(pressTime / pressConfirmationTime);
                pressFeedback.FeedbacksIntensity = shakeIntensity.Evaluate(pressTime / pressConfirmationTime);
                pressFeedback.PlayFeedbacks();
            }
        }
        else
        {
            coloredImage.fillAmount = 0;
            pressFeedback.StopFeedbacks();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GameUI.instance.ShowTalentDescription(talentReference);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameUI.instance.HideTalentDescription();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressing = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressing = false;
        pressTime = 0;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TalentSlotUI))]
public class  TallenSlotUIEditor:Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        TalentSlotUI talentSlotUI = (TalentSlotUI)target;
        if(GUILayout.Button("Set Talent"))
        {
            talentSlotUI.SetTalent(talentSlotUI.talentReference);
            // Save the changes back to the object
            EditorUtility.SetDirty(talentSlotUI);
        }
    }
}
#endif

