using UnityEngine;

public class PlayerSuctionState : PlayerState
{
    private float suctionDuration;

    public PlayerSuctionState(PlayerController player, PlayerStateMachine stateMachine, D_Player playerData, int animBoolHash) 
        : base(player, stateMachine, playerData, animBoolHash)
    {
    }

    public void SetDuration(float duration)
    {
        // Có thể gán tối thiểu 0.2s để tránh tình trạng chưa kịp hút đã thoát State
        this.suctionDuration = Mathf.Max(duration, 0.2f); 
    }

    public override void Enter()
    {
        base.Enter();
        // Không gọi player.StopVelocity() ở đây vì ta đang mượn lực đẩy của KnockbackComponent
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // Ưu tiên 1: Bị choáng khi đang bị kéo
        if (player.CC != null && player.CC.isStunned)
        {
            stateMachine.ChangeState(player.StunnedState);
            return;
        }

        // Hết thời gian mất kiểm soát -> Trả lại quyền điều khiển
        if (Time.time >= startTime + suctionDuration)
        {
            if (input.sqrMagnitude > 0.01f) stateMachine.ChangeState(player.MoveState);
            else stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void PhysicUpdate()
    {
        base.PhysicUpdate();
        // Lực ma sát: Giảm dần tốc độ kéo để Player không bị trượt đi quá xa
        player.rb.linearVelocity = Vector2.Lerp(player.rb.linearVelocity, Vector2.zero, 5f * Time.fixedDeltaTime);
    }
}