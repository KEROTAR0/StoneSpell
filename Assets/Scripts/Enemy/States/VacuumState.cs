using UnityEngine;

public class VacuumState : State
{
    protected Vector2 pullCenter;
    protected float pullSpeed;
    protected float pullDuration;

    public VacuumState(EnemyController entity, FiniteStateMachine stateMachine, int animBoolHash) 
        : base(entity, stateMachine, animBoolHash)
    {
    }

    public void SetupPull(Vector2 center, float speed, float duration)
    {
        this.pullCenter = center;
        this.pullSpeed = speed;
        this.pullDuration = duration;
    }

    public override void Enter()
    {
        base.Enter();
        entity.StopMovement();

        if (entity.aiPath != null)
        {
            entity.aiPath.enabled = false;
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (Time.time >= startTime + pullDuration)
        {
            entity.ReturnToDefaultState();
        }
        else
        {
            // Ép tọa độ trượt về tâm Hố đen mỗi frame
            if (Vector2.Distance(entity.transform.position, pullCenter) > 0.1f)
            {
                entity.transform.position = Vector2.MoveTowards(entity.transform.position, pullCenter, pullSpeed * Time.deltaTime);
            }
        }
    }
    public override void Exit()
    {
        base.Exit();

        if (entity.aiPath != null)
        {
            entity.aiPath.enabled = true;
        }
    }
}
