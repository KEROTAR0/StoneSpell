using UnityEngine;

public class PlayerStunnedState : PlayerState
{
    public PlayerStunnedState(PlayerController player, PlayerStateMachine stateMachine, D_Player playerData, int animBoolHash) 
        : base(player, stateMachine, playerData, animBoolHash)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.StopVelocity();
        // Gợi ý: Bật icon sao chớp chớp trên đầu Player thông qua UIManager ở đây
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // Chỉ thoát State khi CoreCCComponent báo là đã hết choáng
        if (player.CC != null && !player.CC.isStunned)
        {
            if (input.sqrMagnitude > 0.01f) stateMachine.ChangeState(player.MoveState);
            else stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void PhysicUpdate()
    {
        base.PhysicUpdate();
        player.StopVelocity(); // Ép đứng im tuyệt đối
    }
}