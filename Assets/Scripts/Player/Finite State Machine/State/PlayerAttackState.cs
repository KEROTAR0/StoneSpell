using UnityEngine;

public class PlayerAttackState : PlayerState
{
    protected bool isAnimationFinished;
    protected Vector2 shootDirection;
    protected Vector3 mouseWorldPos;

    public PlayerAttackState(PlayerController player, PlayerStateMachine stateMachine, D_Player playerData, int animBoolHash) 
        : base(player, stateMachine, playerData, animBoolHash)
    {
    }

    public override void Enter()
    {
        base.Enter();
        isAnimationFinished = false;
        // Đã xóa player.Slow() ở đây, chuyển logic hãm phanh xuống PhysicUpdate
    }

    public override void DoCheck()
    {
        base.DoCheck();
        mouseWorldPos = Camera.main.ScreenToWorldPoint(player.InputHandler.MousePosition);
        mouseWorldPos.z = 0; 
        
        // Đảm bảo PlayerController đã được khai báo biến attackPosition
        shootDirection = (mouseWorldPos - player.attackPosition.position).normalized;
        player.CheckIfShouldFlip(shootDirection.x);
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // KHÓA STATE: Nếu bị Choáng/Ngủ thì không xử lý đổi state ở đây nữa (HurtState sẽ lo)
        if (player.CC != null && player.CC.isStunned) return;

        if (isAnimationFinished)
        {
            if (input.sqrMagnitude > 0.01f) stateMachine.ChangeState(player.MoveState);
            else stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void PhysicUpdate()
    {
        base.PhysicUpdate();
        
        // CHUẨN FSM: Nội suy giảm dần vận tốc mỗi frame vật lý thay vì dùng Coroutine
        player.rb.linearVelocity = Vector2.Lerp(player.rb.linearVelocity, Vector2.zero, playerData.attackSlowRate * Time.fixedDeltaTime);
    }

    public virtual void FinishAttack()
    {
        isAnimationFinished = true;
    }
}