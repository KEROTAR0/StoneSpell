using UnityEngine;

public class PlayerNormalAttackState : PlayerAttackState
{
    private bool hasShot;
    public PlayerNormalAttackState(Player player, PlayerStateMachine stateMachine, D_Player playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName)
    {
    }
    public override void Enter()
    {
        base.Enter();
        player.InputHandler.UseAttackInput();
        player.ResetNormalAttackCooldown();

        hasShot = false;
    }
    public override void DoCheck()
    {
        base.DoCheck();
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        if (!hasShot && Time.time >= (startTime + playerData.normalAttackCastingTime))
        {
            Shoot();
            hasShot = true;
            FinishAttack();
        }

        if (isAnimationFinished)
        {
            if (input.sqrMagnitude > 0.01f)
            {
                stateMachine.ChangeState(player.MoveState);
            }
            else
            {
                stateMachine.ChangeState(player.IdleState);
            }
        }
    }
    public override void PhysicUpdate()
    {
        base.PhysicUpdate();
    }
    public override void FinishAttack()
    {
        base.FinishAttack();
    }
    public override void Exit()
    {
        base.Exit();
    }
    private void Shoot()
    {
        // lấy đạn từ Pool
        GameObject bullet = ProjectilePool.Instance.GetFromPool(player.attackPosition.position, Quaternion.identity);

        // logic bay và sát thương
        Projectile projectileScript = bullet.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.Fire(shootDirection);
        }
    }
}
