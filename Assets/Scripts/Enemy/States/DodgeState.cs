using UnityEngine;

public class DodgeState : State
{
    private Vector2 dodgeDirection;
    private float dodgeEndTime;

    public DodgeState(Entity entity, FiniteStateMachine stateMachine, int animBoolHash) 
        : base(entity, stateMachine, animBoolHash)
    {
    }

    // Hàm này được gọi TRƯỚC KHI stateMachine.ChangeState() để cài đặt hướng
    public void SetDodgeDirection(Vector2 direction)
    {
        this.dodgeDirection = direction.normalized;
    }

    public override void Enter()
    {
        base.Enter();
        dodgeEndTime = Time.time + entity.entityData.dodgeDuration;
        
        // (Tùy chọn) Bật hiệu ứng khói lướt hoặc Ghost trail ở đây
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (entity.rb != null)
        {
            entity.rb.linearVelocity = dodgeDirection * entity.entityData.dodgeSpeed;
        }

        if (Time.time >= dodgeEndTime)
        {
            entity.ReturnToDefaultState(); 
        }
    }

    public override void Exit()
    {
        base.Exit();
        entity.SetLastDodgeTime(); // Bắt đầu tính Cooldown
        entity.StopMovement(); // Dừng lại sau khi lướt xong
    }
}