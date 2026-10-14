using UnityEngine;

public class PlayerIdleState : PlayerState
{
    public PlayerIdleState(PlayerController player, PlayerStateMachine stateMachine, D_Player playerData, int animBoolHash) 
        : base(player, stateMachine, playerData, animBoolHash)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.StopVelocity(); // Dừng cứng khi vào Idle
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 1. NẾU ĐANG BỊ KHỐNG CHẾ (CHOÁNG/NGỦ) -> KHÔNG CHO CHUYỂN STATE
        if (player.CC != null && player.CC.isStunned) return;

        // 2. XỬ LÝ INPUT NHƯ BÌNH THƯỜNG
        if(input.sqrMagnitude > 0.01f)
        {
            stateMachine.ChangeState(player.MoveState);
            return;
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
        player.StopVelocity(); // Ép liên tục để không bị trôi
    }
}