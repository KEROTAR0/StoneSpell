using System;
using UnityEngine;

public class PlayerState
{
    protected PlayerStateMachine stateMachine;
    protected PlayerController player;
    protected D_Player playerData;
    protected Vector2 input;

    public float startTime {get; protected set;}
    protected int animBoolHash;

    public PlayerState(PlayerController player, PlayerStateMachine stateMachine, D_Player playerData, int animBoolHash)
    {
        this.player = player;
        this.stateMachine = stateMachine;
        this.playerData = playerData;
        this.animBoolHash = animBoolHash;
    }

    public virtual void Enter()
    {
        startTime = Time.time;
        player.anim.SetBool(animBoolHash, true);
        DoCheck();
    }
    public virtual void Exit()
    {
        player.anim.SetBool(animBoolHash, false);
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
            player.anim.SetFloat("dirX", input.x);
            player.anim.SetFloat("dirY", input.y);
        }
    }
    public virtual void DoCheck()
    {
        
    }
}
