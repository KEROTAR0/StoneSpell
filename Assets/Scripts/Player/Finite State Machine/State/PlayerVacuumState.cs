using UnityEngine;

public class PlayerVacuumState : PlayerState
{
    public PlayerVacuumState(PlayerController player, PlayerStateMachine stateMachine, D_Player playerData, int animBoolHash) 
        : base(player, stateMachine, playerData, animBoolHash)
    {
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 1. Ưu tiên: Bị choáng đè lên -> Văng khỏi Hố đen vào StunnedState
        if (player.CC != null && player.CC.isStunned)
        {
            stateMachine.ChangeState(player.StunnedState);
            return;
        }
        
        // 2. Dash để thoát khỏi Hố đen (Tùy chọn: Có cho phép người chơi Dash thoát ra không?)
        if (player.InputHandler.DashInput && player.CanDash())
        {
            stateMachine.ChangeState(player.DashState);
            return;
        }
        
        // Cố tình khóa đứng ở LogicUpdate này, không tự động thoát ra bằng Time.time.
        // Việc thoát ra sẽ do lệnh HandleVacuumEnd() của PlayerController gọi.
    }

    public override void PhysicUpdate()
    {
        base.PhysicUpdate();
        // Không cấp linearVelocity ở đây. Việc di chuyển đã được PullableComponent lo.
    }
}