using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(Player player, PlayerStateMachine stateMachine, D_Player playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName)
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

        if(input.sqrMagnitude > 0.01f)
        {
            stateMachine.ChangeState(player.MoveState);
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
    }
    public override void PhysicUpdate()
    {
        base.PhysicUpdate();

        player.Stop();
    }
    public override void Exit()
    {
        base.Exit();
    }
}
