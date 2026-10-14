using UnityEngine;

public class PlayerHurtState : PlayerState
{
    public PlayerHurtState(PlayerController player, PlayerStateMachine stateMachine, D_Player playerData, int animBoolHash) 
        : base(player, stateMachine, playerData, animBoolHash)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.isInvincible = true; 
        
        // Vận tốc (Knockback) đã được cấp bởi KnockbackComponent 
        // Nên ta không can thiệp vào rb.linearVelocity.
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 1. KIỂM TRA STUN
        if (player.CC != null && player.CC.isStunned)
        {
            // Bị khóa cứng -> Kẹt ở HurtState, không được ra ngoài
            return; 
        }

        // 2. NẾU KHÔNG STUN -> XÀI THỜI GIAN GIẬT KHỰNG (HURT DURATION)
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