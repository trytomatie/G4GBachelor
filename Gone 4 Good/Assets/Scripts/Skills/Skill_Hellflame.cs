using UnityEngine;
using static PlayerController;
using UnityEngine.InputSystem;
using static Unity.VisualScripting.Member;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "Skill_Hellflame", menuName = "ScriptableObjects/Skills/Skill_Hellflame", order = 1)]
public class Skill_Hellflame : Skill
{
    private IEntityControlls controller;
    private GameObject castingVFX;
    private GameObject spellVFX;
    private float tickrate = 0.25f;
    private float tickRateTimer;
    public int lifeTime = 10;


    public override void OnEnter(GameObject source)
    {
        controller = source.GetComponent<IEntityControlls>();
        onEnterTime = Time.time;

        castingVFX = VFXManager.Instance.PlayFeedback(13, controller.CastingPivot, controller.CastingPivot.transform.rotation);
        castingVFX.transform.parent = controller.CastingPivot;
        castingVFX.transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
        ParticleSystem.MainModule main= castingVFX.transform.GetChild(0).GetComponent<ParticleSystem>().main;
        main.duration = castTime-0.4f;
        castingVFX.transform.GetChild(0).GetComponent<ParticleSystem>().Play();
        controller.GetAnimator().SetBool("chrageStaff", true);
        spellVFX = null;
    }
    public override void OnUpdate(GameObject source)
    {
        controller.ManualMovement();
        controller.CastRotation();
        if(CastingFinsihed())
        {
            if(spellVFX == null)
            {
                spellVFX = VFXManager.Instance.PlayFeedback(14, controller.CastingPivot, controller.CastingPivot.transform.rotation);
                spellVFX.transform.parent = controller.CastingPivot;
                //go.GetComponent<FireBoltProjectile>().SetOwner(controller.GetGameObject());
                controller.SkillColldowns[controller.SkillIndex] = Time.time;
                castingVFX.transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
                Destroy(castingVFX, 133);
            }
            if (tickRateTimer < Time.time)
            {
                tickRateTimer = Time.time+ tickrate;
                DamageObject hitbox = GameManager.Instance.SpawnHitbox(controller.GetGameObject().transform, 1).GetComponent<DamageObject>();
                hitbox.source = controller.StatusManager;
                hitbox.lifeTime = tickrate/2;
            }
        }
        if (!controller.HoldingSkill)
        {
            controller.SkillColldowns[controller.SkillIndex] = Time.time - skillCooldown + 1;
            castingVFX.transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
            spellVFX.transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
            Destroy(spellVFX, 3);
            Destroy(castingVFX, 3);
            controller.SwitchState(PlayerState.Controlling);
        }
    }



    public override void OnExit(GameObject source)
    {
        controller.GetAnimator().SetBool("chrageStaff", false);
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

    private bool CastingFinsihed()
    {
        return Time.time - onEnterTime > castTime;
    }
}
