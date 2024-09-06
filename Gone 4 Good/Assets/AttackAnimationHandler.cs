using UnityEngine;

public class AttackAnimationHandler : MonoBehaviour
{
    public ZombieAI zombieAI;
    public void SpawnHitBox(float lingerTime)
    {
        zombieAI.spawnHitBox = lingerTime;
    }
}
