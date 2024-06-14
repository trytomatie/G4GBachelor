using UnityEngine;
using static PlayerController;
using UnityEngine.InputSystem;
using static Unity.VisualScripting.Member;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "Skill_FieldOfFlowers", menuName = "ScriptableObjects/Skills/Skill_FieldOfFlowers", order = 1)]
public class Skill_FieldOfFlowers : Skill
{
    private IEntityControlls controller;
    public GameObject[] flowerPrefabs;
    public List<GameObject> spawnedObjects;
    private GameObject castingVFX;
    public int flowerAmountPerTick = 5;
    public int ticks = 5;
    private int itterations = 1;
    private float tickTimer = 0;
    public int lifeTime = 10;
    public LayerMask layerMask;


    public override void OnEnter(GameObject source)
    {
        controller = source.GetComponent<IEntityControlls>();
        onEnterTime = Time.time;
        itterations = 1;
        controller.SkillColldowns[controller.SkillIndex] = Time.time;
        spawnedObjects = new List<GameObject>();
        castingVFX = VFXManager.Instance.PlayFeedback(9, controller.GetGameObject().transform,controller.GetGameObject().transform.rotation);

    }
    public override void OnUpdate(GameObject source)
    {
        if(itterations-1 < ticks)
        {
            tickTimer = Time.time;
            for (int i = 0; i < flowerAmountPerTick + itterations*3; i++)
            {
                int randomIndex = Random.Range(0, flowerPrefabs.Length);
                Vector2 rnd = Random.onUnitSphere * (itterations/2f);
                RaycastHit hit;
                if(Physics.Raycast(new Vector3(rnd.x, controller.GetGameObject().transform.position.y + 2, rnd.y) + controller.GetGameObject().transform.position, Vector3.down, out hit,4,layerMask))
                {
                    Vector3 randomPosition = hit.point;
                    GameObject spawnedObject = Instantiate(flowerPrefabs[randomIndex], randomPosition, Quaternion.Euler(0,Random.Range(0,360),0));
                    spawnedObject.SetActive(false);
                    spawnedObjects.Add(spawnedObject);
                    Destroy(spawnedObject, lifeTime);
                }
            }
            itterations++;
        }
        if (Time.time > onEnterTime + castTime-0.25f)
        {
            castingVFX.transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
            Destroy(castingVFX, 3);
        }
        if (Time.time > onEnterTime + castTime)
        {
            spawnedObjects = spawnedObjects.OrderBy(e => Vector3.Distance(e.transform.position, controller.GetGameObject().transform.position)).ToList();
            GameManager.Instance.SpawnFlowersDelayed(spawnedObjects,8,0.4f);
            GameObject flowerVfx = VFXManager.Instance.PlayFeedback(10, controller.GetGameObject().transform, controller.GetGameObject().transform.rotation);
            GameManager.Instance.StopParticleSystemAfterTime(flowerVfx.transform.GetChild(0).GetComponent<ParticleSystem>(), 3);
            Destroy(flowerVfx, 6);
            controller.SwitchState(PlayerState.Controlling);
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

                return true;

        }
        return false;
    }
}
