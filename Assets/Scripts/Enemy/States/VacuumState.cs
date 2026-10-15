using UnityEngine;

public class VacuumState : State
{
    public VacuumState(EnemyController entity, FiniteStateMachine stateMachine, int animBoolHash) 
        : base(entity, stateMachine, animBoolHash)
    {
    }

    public override void Enter()
    {
        base.Enter();
        entity.StopMovement();

        if (entity.aiPath != null) entity.aiPath.enabled = false;
    }

    public override void PhysicUpdate()
    {
        base.PhysicUpdate();
        // KHÔNG LÀM GÌ CẢ. Việc kéo Rigidbody đã được PullableComponent tự động xử lý.
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // Nếu Component báo cáo đã thoát khỏi hố đen -> Chuyển về trạng thái cũ
        if (entity.Pullable != null && !entity.Pullable.isBeingVacuumed)
        {
            entity.ReturnToDefaultState();
        }
    }

    public override void Exit()
    {
        base.Exit();
        if (entity.aiPath != null) entity.aiPath.enabled = true;
    }
}