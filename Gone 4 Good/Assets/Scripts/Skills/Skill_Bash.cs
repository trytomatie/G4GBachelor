using UnityEngine;
using static PlayerController;
using UnityEngine.InputSystem;
using static Unity.VisualScripting.Member;

[CreateAssetMenu(fileName = "Skill_Bash", menuName = "ScriptableObjects/Skills/Skill_Bash", order = 1)]
public class Skill_Bash : Skill
{
    private IEntityControlls controller;

    public float knockbackForce = 10;

    public override void OnEnter(GameObject source)
    {
        controller = source.GetComponent<IEntityControlls>();
        GameObject vfx = VFXManager.Instance.PlayFeedback(15, controller.VfxTransform, controller.GetGameObject().transform.rotation);
        Destroy(vfx, 5);

        GameManager.Instance.SpawnHitbox(controller.GetGameObject().transform, 0).GetComponent<DamageObject>().source = controller.StatusManager;
        controller.GetAnimator().SetFloat("speed", 0);
        controller.GetAnimator().Play("Attack");
        controller.SkillColldowns[controller.SkillIndex] = Time.time;

    }
    public override void OnUpdate(GameObject source)
    {
        controller.SwitchState(PlayerState.Controlling);
    }

    public override void OnExit(GameObject source)
    {
        controller.GetAnimator().CrossFade("Movement",0.2f);
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