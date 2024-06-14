
using UnityEngine;

public interface IEntityControlls
{
    public Animator GetAnimator();
    public GameObject GetGameObject();
    public StatusManager StatusManager { get; set; }
    public Transform VfxTransform { get; set; }
    public Vector3 GetMovmentDirection();
    public bool HoldingSkill { get;}
    public void Movement(Vector3 movement);
    public void CastRotation();
    public void ManualMovement();
    public Transform CastingPivot { get; }
    public float[] SkillColldowns { get; set; }
    public int SkillIndex { get; set; }
    void SwitchState(PlayerController.PlayerState controlling);
}
