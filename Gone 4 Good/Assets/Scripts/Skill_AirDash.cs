using UnityEngine;
using static PlayerController;
using UnityEngine.InputSystem;
using static Unity.VisualScripting.Member;

[CreateAssetMenu(fileName = "Skill_AirDash", menuName = "ScriptableObjects/Skills/Skill_AirDash", order = 1)]
public class Skill_AirDash : Skill
{
    private IEntityControlls controller;

    [Header("Air Stepping")]
    public int dashLimit = 0;
    public int dashCount = 0;

    public float airStepDistance = 5;
    public AnimationCurve airStepCurve;
    public float airStepDuration = 0.5f;

    [Header("State Variables")]
    private Vector3 facingDirection;
    private float airStepInputLockoutTime = 0.9f; // Percentage of the airStepDuration

    public override void OnEnter(GameObject source)
    {
        controller = source.GetComponent<IEntityControlls>();
        dashCount++;
        AudioManager.PlaySound(source.transform.position, SoundType.Player_Dash);
        VFXManager.Instance.PlayFeedback(0, source.transform);
        onEnterTime = Time.time + airStepDuration;
        facingDirection = controller.GetMovmentDirection();
        if (facingDirection == Vector3.zero)
        {
            facingDirection = controller.GetGameObject().transform.forward;
        }
        controller.GetAnimator().SetFloat("speed", 0);
    }
    public override void OnUpdate(GameObject source)
    {
        AirStep(controller,facingDirection, airStepDuration - (onEnterTime - Time.time));

        if (Time.time > onEnterTime)
        {
            controller.SwitchState(PlayerState.Controlling);
            dashCount = 0;
            controller.SkillColldowns[controller.SkillIndex] = Time.time;
        }
    }

    public override void OnExit(GameObject source)
    {

    }

    public override bool CheckSkillConditions(GameObject source)
    {
        controller = source.GetComponent<IEntityControlls>();
        if (controller.SkillColldowns[controller.SkillIndex] + skillCooldown < Time.time)
        {
            if (controller.GetMovmentDirection() != Vector3.zero && dashCount < dashLimit)
            {
                return true;
            }
        }
        return false;
    }

    public void AirStep(IEntityControlls controller,Vector3 airStepDirection, float delta)
    {

        if (airStepDirection == Vector3.zero)
        {
            return;
        }
        float airStepDistance = this.airStepDistance;
        float airStepCurveValue = airStepCurve.Evaluate(delta / airStepDuration);
        Vector3 airStepVector = airStepDirection * airStepDistance * airStepCurveValue;
        controller.Movement(airStepVector * Time.deltaTime);
    }
}