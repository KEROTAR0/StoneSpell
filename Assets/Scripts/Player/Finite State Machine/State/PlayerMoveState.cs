using UnityEngine;

public class PlayerMoveState : PlayerState
{
    public PlayerMoveState(PlayerController player, PlayerStateMachine stateMachine, D_Player playerData, int animBoolHash) 
        : base(player, stateMachine, playerData, animBoolHash)
    {
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
        // 1. NẾU ĐANG BỊ KHỐNG CHẾ -> VỀ IDLE ĐỂ ĐỨNG IM
        if (player.CC != null && player.CC.isStunned)
        {
            stateMachine.ChangeState(player.IdleState);
            return;
        }

        // Cập nhật hướng mặt và Animator
        player.CheckIfShouldFlip(input.x);
        UpdateAnimatorParameters(); // (Hàm của lớp cha PlayerState)

        if (input.sqrMagnitude > 0.01f)
        {
            player.facingDirection = new Vector2(Mathf.Round(input.x), Mathf.Round(input.y)).normalized;
        }

        // Ưu tiên Dash
        if (player.InputHandler.DashInput && player.CanDash())
        {
            stateMachine.ChangeState(player.DashState);
            return;
        }

        // Ưu tiên Chém
        if (player.InputHandler.NormalAttackInput && player.normalAttackCooldownTimer <= 0)
        {
            stateMachine.ChangeState(player.NormalAttackState);
            return; // Nhớ thêm return để thoát LogicUpdate
        }

        // Hết bấm nút -> Về Idle
        if (input.sqrMagnitude <= 0.01f)
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void PhysicUpdate()
    {
        base.PhysicUpdate();

        // 2. LẤY HỆ SỐ TỐC ĐỘ TỪ CORE CC COMPONENT (Thay vì biến nội bộ của Player)
        float currentSpeedMult = player.CC != null ? player.CC.moveSpeedMultiplier : player.moveSpeedMultiplier;
        
        player.rb.linearVelocity = input.normalized * (playerData.movementSpeed * currentSpeedMult);
    }
}