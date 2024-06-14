using UnityEngine;
using static PlayerController;
using UnityEngine.InputSystem;
using static Unity.VisualScripting.Member;

[CreateAssetMenu(fileName = "Skill_SummonKnightLight", menuName = "ScriptableObjects/Skills/Skill_SummonKnightLight", order = 1)]
public class Skill_SummonKnightLight : Skill
{
    private IEntityControlls controller;
    public GameObject knightLightPrefab;
    public GameObject spawnedObject;
    public int lifeTime = 30;

    public override void OnEnter(GameObject source)
    {
        controller = source.GetComponent<IEntityControlls>();
        spawnedObject = Instantiate(knightLightPrefab,controller.GetGameObject().transform.position,Quaternion.identity);
        //spawnedObject.GetComponent<FollowTargertAI>().target = controller.GetGameObject().transform;
        //spawnedObject.GetComponent<FollowTargertAI>().speed = 1;
        Destroy(spawnedObject, 30);
        controller.SkillColldowns[controller.SkillIndex] = Time.time;

    }
    public override void OnUpdate(GameObject source)
    {
        controller.SwitchState(PlayerState.Controlling);
    }

    public override void OnExit(GameObject source)
    {

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