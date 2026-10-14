using UnityEngine;

public class AttackState : State
{
    protected bool isAnimationFinished;
    protected bool isAttacking;

    public AttackState(EnemyController entity, FiniteStateMachine stateMachine, int animBoolHash) 
        : base(entity, stateMachine, animBoolHash)
    {
    }

    public override void Enter()
    {
        base.Enter();
        isAnimationFinished = false;
        isAttacking = false;
        
        entity.StopMovement(); 
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
        if (isAnimationFinished)
        {
            entity.ReturnToDefaultState();
        }
    }
    public override void Exit()
    {
        base.Exit();  
    }
    // Animation Event
    public override void TriggerAttack()
    {
        base.TriggerAttack();
        isAttacking = true;
    }

    public override void FinishAttack()
    {
        base.FinishAttack();
        isAnimationFinished = true;
    }
}