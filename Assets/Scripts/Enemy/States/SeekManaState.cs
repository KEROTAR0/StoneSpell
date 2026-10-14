using UnityEngine;

public class SeekManaState : State
{
    private Transform targetMana;

    public SeekManaState(EnemyController entity, FiniteStateMachine stateMachine, int animBoolHash) 
        : base(entity, stateMachine, animBoolHash)
    {
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        targetMana = entity.GetNearestManaShard();

        if (targetMana == null)
        {
            entity.ReturnToDefaultState();
            return;
        }

        if (entity.aiPath != null)
        {
            entity.aiPath.destination = targetMana.position;
            entity.aiPath.canMove = true;
            entity.aiPath.isStopped = false;
            entity.UpdateFacingFromPathfinding(entity.aiPath.desiredVelocity);
        }

        // KHI ĐẾN GẦN MANA -> BÀN GIAO CHO STATE ĐẠP NỔ
        float distanceToMana = Vector2.Distance(entity.transform.position, targetMana.position);
        if (distanceToMana <= entity.entityData.shortRangeAction)
        {
            if (targetMana.TryGetComponent(out ManaShard shard))
            {
                // Truyền viên Mana vào và đổi State
                entity.detonateManaState.SetTargetMana(shard);
                stateMachine.ChangeState(entity.detonateManaState);
            }
        }
    }
    
    public override void Exit()
    {
        base.Exit();
        entity.StopMovement();
    }
}