using UnityEditor.AnimatedValues;
using UnityEngine;

public class State
{
    protected FiniteStateMachine stateMachine;
    protected EnemyController entity;

    public float startTime {get; protected set;}
    protected int animBoolHash;

    public State(EnemyController entity, FiniteStateMachine stateMachine, int animBoolHash)
    {
        this.entity = entity;
        this.stateMachine = stateMachine;
        this.animBoolHash = animBoolHash;
    }

    public virtual void Enter()
    {
        startTime = Time.time;
        entity.anim.SetBool(animBoolHash, true);
        DoCheck();
    }
    public virtual void Exit()
    {
        entity.anim.SetBool(animBoolHash, false);
    }
    public virtual void LogicUpdate(){}
    public virtual void PhysicUpdate()
    {
        DoCheck();
        //UpdateEntityAnimatorParameters();
    }
    public virtual void DoCheck(){}
    public virtual void TriggerAttack() { }
    public virtual void FinishAttack() { }
}
