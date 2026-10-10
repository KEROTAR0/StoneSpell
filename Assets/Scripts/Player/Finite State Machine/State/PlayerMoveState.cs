using UnityEngine;

public class PlayerMoveState : PlayerState
{
    public PlayerMoveState(Player player, PlayerStateMachine stateMachine, D_Player playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName)
    {
    }
    public override void Enter()
    {
        base.Enter();
    }
    public override void DoCheck()
    {
        base.DoCheck();
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        player.CheckIfShouldFlip(input.x);
        UpdateAnimatorParameters();

        if (input.sqrMagnitude > 0.01f)
        {
            player.facingDirection = new Vector2(Mathf.Round(input.x), Mathf.Round(input.y)).normalized;
        }
        if (player.InputHandler.DashInput && player.CanDash())
        {
            stateMachine.ChangeState(player.DashState);
            return;
        }
        if (player.InputHandler.NormalAttackInput && player.normalAttackCooldownTimer <= 0)
        {
            stateMachine.ChangeState(player.NormalAttackState);
        }
        if (input.sqrMagnitude <= 0.01f)
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }
    public override void PhysicUpdate()
    {
        base.PhysicUpdate();

        player.rb.linearVelocity = input.normalized * (playerData.movementSpeed * player.moveSpeedMultiplier);
        
    }
    public override void Exit()
    {
        base.Exit();
    }
}
