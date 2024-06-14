using UnityEngine;
using static PlayerController;
using UnityEngine.InputSystem;
using static Unity.VisualScripting.Member;

[CreateAssetMenu(fileName = "Skill_BladeStorm", menuName = "ScriptableObjects/Skills/Skill_BladeStorm", order = 1)]
public class Skill_BladeStorm : Skill
{
    private IEntityControlls controller;

    [Header("State Variables")]
    public float bladeStormDuration = 1.5f;
    private Vector3 facingDirection;
    private float airStepInputLockoutTime = 0.9f; // Percentage of the airStepDuration
    private float vfxInterval = 0.25f;
    public float vfxIntervalTimer = 0;
    public override void OnEnter(GameObject source)
    {
        controller = source.GetComponent<IEntityControlls>();
        onEnterTime = Time.time + bladeStormDuration;
        controller.GetAnimator().SetFloat("speed", 0);
        controller.GetAnimator().SetBool("BladeStorm",true);
    }
    public override void OnUpdate(GameObject source)
    {
        controller.ManualMovement();
        if (Time.time > vfxIntervalTimer + vfxInterval)
        {
            vfxIntervalTimer = Time.time;
            VFXManager.Instance.PlayFeedback(6, controller.GetGameObject().transform);
            GameManager.Instance.SpawnHitbox(controller.GetGameObject().transform,2);
        }
        if (Time.time > onEnterTime)
        {

            controller.SwitchState(PlayerState.Controlling);
            controller.SkillColldowns[controller.SkillIndex] = Time.time;
        }
    }

    public override void OnExit(GameObject source)
    {
        controller.GetAnimator().SetBool("BladeStorm", false);
    }

    public override bool CheckSkillConditions(GameObject source)
    {
        controller = source.GetComponent<IEntityControlls>();
        if (controller.SkillColldowns[controller.SkillIndex] + skillCooldown < Time.time)
        {
                return true;
        }
        return false;
    }
}