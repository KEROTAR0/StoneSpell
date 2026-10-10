using UnityEngine;

public class RepositionState : State
{
    private Vector2 destination;
    private bool isRepositioning;

    public RepositionState(Entity entity, FiniteStateMachine stateMachine, string animBoolName) 
        : base(entity, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        isRepositioning = true;
        
        // 1. Tính toán điểm chạy lách (Ví dụ: chạy lùi hoặc dạt sang ngang 2-3 mét)
        Vector2 dirAwayFromPlayer = (entity.transform.position - Player.Instance.transform.position).normalized;
        
        // Trộn thêm 1 chút góc ngẫu nhiên để nó không đi thẳng lùi: Range(-45f, 45f);
        float randomAngle = Random.Range(-135f, 135f);
        Vector2 randomDodgeDir = Quaternion.Euler(0, 0, randomAngle) * dirAwayFromPlayer;
        
        // Khoảng cách chạy ngẫu nhiên từ 2 đến 4 mét
        destination = (Vector2)entity.transform.position + randomDodgeDir * Random.Range(2f, 4f);

        // 2. Gửi lệnh cho A*
        if (entity.aiPath != null)
        {
            entity.aiPath.SetPath(null);
            entity.aiPath.canMove = true;
            entity.aiPath.isStopped = false;
            
            // Ép điểm ngẫu nhiên dính vào Navmesh
            var nnInfo = AstarPath.active.GetNearest(destination, Pathfinding.NNConstraint.Default);
            entity.aiPath.destination = (Vector3)nnInfo.node.position;
            entity.aiPath.SearchPath();
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (entity.aiPath != null)
        {
            entity.UpdateFacingFromPathfinding(entity.aiPath.desiredVelocity);

            // 3. Nếu đã tới nơi, hoặc chạy quá 1.5 giây thì dừng lại để bắn
            if (!entity.aiPath.pathPending && (entity.aiPath.reachedDestination || Time.time > startTime + entity.entityData.rangedCooldown))
            {
                isRepositioning = false;
                entity.ReturnToDefaultState();
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        entity.StopMovement();
    }
}