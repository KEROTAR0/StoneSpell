using UnityEngine;

public class PlayerHurtState : PlayerState
{
    public PlayerHurtState(Player player, PlayerStateMachine stateMachine, D_Player playerData, string animBoolName) 
        : base(player, stateMachine, playerData, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.isInvincible = true;
        // Vận tốc (Knockback) đã được cấp ở hàm TakeDamage của Player, 
        // nên ở đây ta không can thiệp vào rb.linearVelocity nữa.
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // Đợi hết thời gian bị choáng thì tự động trả về trạng thái bình thường
        if (Time.time >= startTime + playerData.hurtDuration)
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
    public override void Exit()
    {
        base.Exit();
        player.isInvincible = false;
    }
}