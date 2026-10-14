using UnityEngine;

public class RangedState : AttackState
{
protected GameObject projectilePrefab;

    public RangedState(EnemyController entity, FiniteStateMachine stateMachine, int animBoolHash, GameObject projectilePrefab) 
        : base(entity, stateMachine, animBoolHash)
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

        if (PlayerController.Instance != null)
        {
            shootDir = ((Vector2)PlayerController.Instance.transform.position - spawnPos).normalized;
        }

        GameObject spellObj = ObjectPoolManager.Instance.GetFromPool(projectilePrefab, spawnPos, Quaternion.identity);
        EnemyProjectile projScript = spellObj.GetComponent<EnemyProjectile>();

        if (projScript != null)
        {
            projScript.Initialize(shootDir);
        }
    }
}
