using System;
using UnityEngine;

public class PlayerState
{
    protected PlayerStateMachine stateMachine;
    protected Player player;
    protected D_Player playerData;
    protected Vector2 input;

    public float startTime {get; protected set;}
    protected string animBoolName;

    public PlayerState(Player player, PlayerStateMachine stateMachine, D_Player playerData, string animBoolName)
    {
        this.player = player;
        this.stateMachine = stateMachine;
        this.playerData = playerData;
        this.animBoolName = animBoolName;
    }

    public virtual void Enter()
    {
        startTime = Time.time;
        player.anim.SetBool(animBoolName, true);
        DoCheck();
    }
    public virtual void Exit()
    {
        player.anim.SetBool(animBoolName, false);
    }
    public virtual void LogicUpdate()
    {
        input = player.InputHandler.movementInput;
    }
    public virtual void PhysicUpdate()
    {
        DoCheck();
    }
    public virtual void UpdateAnimatorParameters()
    {
        if (input != Vector2.zero)
        {
            player.anim.SetFloat("MoveX", input.x);
            player.anim.SetFloat("MoveY", input.y);
        }
    }
    public virtual void DoCheck()
    {
        
    }
}
