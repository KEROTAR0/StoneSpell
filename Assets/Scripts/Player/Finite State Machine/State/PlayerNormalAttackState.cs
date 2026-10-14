using UnityEngine;

public class PlayerNormalAttackState : PlayerAttackState
{
    private bool hasShot;

    // Sửa Player thành PlayerController
    public PlayerNormalAttackState(PlayerController player, PlayerStateMachine stateMachine, D_Player playerData, int animBoolHash) 
        : base(player, stateMachine, playerData, animBoolHash)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.InputHandler.UseAttackInput();
        player.ResetNormalAttackCooldown();
        hasShot = false;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
        // Chặn bắn nếu đang bị choáng giữa chừng
        if (player.CC != null && player.CC.isStunned) return;

        if (!hasShot && Time.time >= (startTime + playerData.normalAttackCastingTime))
        {
            Shoot();
            hasShot = true;
            FinishAttack();
        }
    }

    private void Shoot()
    {
        float maxSpreadAngle = 5f; 
        float randomAngle = Random.Range(-maxSpreadAngle, maxSpreadAngle);
        Vector2 spreadDirection = Quaternion.Euler(0, 0, randomAngle) * shootDirection;
        
        GameObject bullet = GameManager.Instance.SpawnPlayerNormalAttack(player.attackPosition.position, Quaternion.identity);

        if (bullet.TryGetComponent(out PlayerNormalProjectile projectileScript))
        {
            projectileScript.Fire(spreadDirection);
        }
    }
}