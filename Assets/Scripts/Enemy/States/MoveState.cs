using UnityEngine;

public class MoveState : State
{
    protected bool isDetectingObstacle;
    protected float moveTime;
    protected bool isMoveTimeOver;
    private State idleState;
    public MoveState(Entity entity, FiniteStateMachine stateMachine, string animBoolName) 
        : base(entity, stateMachine, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        entity.ChooseRandom4Direction();
        isDetectingObstacle = false;
        isMoveTimeOver = false;
        moveTime = Random.Range(entity.entityData.minMoveTime, entity.entityData.maxMoveTime);
    }
    public void SetTransitions(State idleState)
    {
        this.idleState = idleState;
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
        entity.SetVelocity(entity.facingDirection, entity.entityData.moveSpeed);

        // Kiểm tra báo động toàn phòng
        /*
        if (!entity.isAggroed && entity.CheckPlayerInAggroRange())
        {
            if (entity.currentRoom != null) entity.currentRoom.TriggerRoomBattle();
            else entity.ForceAggro();
            return; 
        }

        if (entity.isAggroed)
        {
            entity.ReturnToDefaultState();
            return;
        }*/
        
        if (Time.time >= startTime + moveTime)
        {
            isMoveTimeOver = true;
        }
    }
    public override void DoCheck()
    {
        base.DoCheck();
    }
}