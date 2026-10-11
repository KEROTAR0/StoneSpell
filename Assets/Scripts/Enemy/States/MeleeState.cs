using UnityEngine;

public class MeleeState : AttackState
{
protected GameObject meleePrefab;

    public MeleeState(Entity entity, FiniteStateMachine stateMachine, string animBoolName, GameObject meleePrefab) 
        : base(entity, stateMachine, animBoolName)
    {
        this.meleePrefab = meleePrefab;
    }
    public override void Exit()
    {
        base.Exit();
        entity.SetLastMeleeTime();
    }
    public override void TriggerAttack()
    {
        if (isAttacking) return;
        base.TriggerAttack();

        // Spawn tại meleeAttackPosition
        Vector2 spawnPos = entity.meleeAttackPosition != null ? entity.meleeAttackPosition.position : entity.transform.position;
        
        GameObject meleeObj = ObjectPoolManager.Instance.GetFromPool(meleePrefab, spawnPos, Quaternion.identity);
        EnemyProjectile projScript = meleeObj.GetComponent<EnemyProjectile>();

        if (projScript != null)
        {
            projScript.Initialize(entity.facingDirection);
        }
    }
}
