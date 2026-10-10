using UnityEngine;

public class PatrolState : State
{
    private State observeState;
    private Vector2 destination;

    public PatrolState(Entity entity, FiniteStateMachine stateMachine, string animBoolName) 
        : base(entity, stateMachine, animBoolName)
    {
    }

    public void SetTransitions(State observeState)
    {
        this.observeState = observeState;
    }

    public override void Enter()
    {
        base.Enter();
        
        destination = entity.GetRandomWalkablePoint();
        Debug.Log("<color=cyan>Bắt đầu đi tuần điểm mới: " + destination + "</color>");

        if (entity.aiPath != null)
        {
            // SỬA LỖI 1: Mở khóa phanh xe và Xóa đường cũ
            entity.aiPath.SetPath(null);
            entity.aiPath.canMove = true;
            entity.aiPath.isStopped = false; 
            
            // Gán đích đến và ép tính đường ngay
            entity.aiPath.destination = destination;
            entity.aiPath.SearchPath();
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (entity.CheckPlayerInAggroRange())
        {
            entity.ForceAggro();
            return;
        }

        if (entity.GetNearestSpell() != null)
        {
            Debug.Log("<color=yellow>Phát hiện đạn khi đi tuần -> Chuyển sang Cảnh giác!</color>");
            
            if (observeState is ObserveState obs) obs.SetAlertMode(true);
            stateMachine.ChangeState(observeState);
            
            return; // Dừng logic di chuyển
        }

        if (entity.aiPath != null)
        {
            entity.UpdateFacingFromPathfinding(entity.aiPath.desiredVelocity);

            if (!entity.aiPath.pathPending && (entity.aiPath.reachedDestination || entity.aiPath.isStopped))
            {
                if (observeState is ObserveState obs) obs.SetAlertMode(false);
                stateMachine.ChangeState(observeState);
            }
        }
    }
    
    public override void Exit()
    {
        base.Exit();
        // Không gọi StopMovement ở đây vì quái có thể cần giữ trớn chạy. 
        // ObserveState hoặc ChaseState sẽ tự lo việc dừng lại khi cần.
    }
}