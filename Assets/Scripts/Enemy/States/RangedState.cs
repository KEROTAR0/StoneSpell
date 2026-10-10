using UnityEngine;

public class RangedState : AttackState
{
protected GameObject projectilePrefab;

    public RangedState(Entity entity, FiniteStateMachine stateMachine, string animBoolName, GameObject projectilePrefab) 
        : base(entity, stateMachine, animBoolName)
    {
        this.projectilePrefab = projectilePrefab;
    }
    public override void Exit()
    {
        base.Exit();
        entity.SetLastRangedTime();
    }
    public override void TriggerAttack()
    {
        if (isAttacking) return;
        base.TriggerAttack();

        Vector2 spawnPos = entity.spawnBulletPosition.position;
        Vector2 shootDir = entity.facingDirection;

        if (Player.Instance != null)
        {
            shootDir = ((Vector2)Player.Instance.transform.position - spawnPos).normalized;
        }

        GameObject spellObj = EnemyAttackPool.Instance.GetFromPool(projectilePrefab, spawnPos, Quaternion.identity);
        EnemyProjectile projScript = spellObj.GetComponent<EnemyProjectile>();

        if (projScript != null)
        {
            projScript.Initialize(shootDir);
        }
    }
}
