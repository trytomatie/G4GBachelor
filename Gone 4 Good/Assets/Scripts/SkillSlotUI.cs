using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour
{
    public Image skillIcon;
    public Image skillCooldownDark;
    public Image skillCooldownRadial;
    public TextMeshProUGUI skillCooldownText;
    public Skill skill;
    public IEntityControlls entity;

    public int index;
    public float skillCooldown;
    private void Start()
    {
        entity = GameManager.Instance.player.GetComponent<IEntityControlls>();
    }
    public void SetupSkill(Skill skill)
    {
        if(skill == null)
        {
            skillIcon.sprite = null;
            skillIcon.enabled = false;
            skillCooldownDark.gameObject.SetActive(false);
            skillCooldownRadial.fillAmount = 0;
            skillCooldownText.gameObject.SetActive(false);
            this.skill = null;
            return;
        }
        skillIcon.sprite = skill.skillIcon;
        skillIcon.enabled = true;
        skillCooldownDark.gameObject.SetActive(false);
        skillCooldownRadial.fillAmount = 0;
        skillCooldownText.gameObject.SetActive(false);
        this.skill = skill;
    }

    private void Update()
    {
        CheckCooldown();
    }

    public void CheckCooldown()
    {
        if (skill == null) skillIcon.enabled = false;
        else skillIcon.enabled = true;

        // if (skill == null) return;
        if (CalculateCooldown() > 0)
        {
            skillCooldownRadial.gameObject.SetActive(true);
            skillCooldownDark.gameObject.SetActive(true);
            skillCooldownRadial.fillAmount = (CalculateCooldown() / skillCooldown);
            skillCooldownText.gameObject.SetActive(true);
            skillCooldownText.text = CalculateCooldown().ToString("F1");
        }
        else
        {

            skillCooldownDark.gameObject.SetActive(false);
            skillCooldownRadial.gameObject.SetActive(false);
            skillCooldownText.gameObject.SetActive(false);
        }
    }

    private float CalculateCooldown()
    {
        return Mathf.Clamp(entity.SkillColldowns[index] + skillCooldown - Time.time, 0, 999);
    }
}
