using UnityEngine;

public class IdleState : State
{
    protected float idleTime;
    protected bool isIdleTimeOver;
    private State moveState;

    public IdleState(Entity entity, FiniteStateMachine stateMachine, int animBoolHash) 
        : base(entity, stateMachine, animBoolHash)
    {
    }

    public override void Enter()
    {
        base.Enter();
        entity.StopMovement();
        
        idleTime = Random.Range(entity.entityData.minIdleTime, entity.entityData.maxIdleTime);
        isIdleTimeOver = false;
    }
    public void SetTransitions(State moveState)
    {
        this.moveState = moveState;
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();

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

        if (Time.time >= startTime + idleTime)
        {
            isIdleTimeOver = true;
        }

    }
}