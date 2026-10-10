using UnityEngine;

public class PlayerAttackState : PlayerState
{
    protected bool isAnimationFinished;
    protected Vector2 shootDirection;
    protected Vector3 mouseWorldPos;
    public PlayerAttackState(Player player, PlayerStateMachine stateMachine, D_Player playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName)
    {
    }
    public override void Enter()
    {
        base.Enter();
        isAnimationFinished = false;
        
        // giảm tốc độ nhân vật lại khi đang vận chiêu
        player.Slow(playerData.attackSlowRate, playerData.attackSlowDuration);
    }
    public override void DoCheck()
    {
        base.DoCheck();

        mouseWorldPos = Camera.main.ScreenToWorldPoint(player.InputHandler.MousePosition);
        mouseWorldPos.z = 0; 
        shootDirection = (mouseWorldPos - player.attackPosition.position).normalized;

        player.CheckIfShouldFlip(shootDirection.x);
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (isAnimationFinished)
        {
            if (input.sqrMagnitude > 0.01f)
            {
                stateMachine.ChangeState(player.MoveState);
            }
            else
            {
                stateMachine.ChangeState(player.IdleState);
            }
        }
    }
    public override void PhysicUpdate()
    {
        base.PhysicUpdate();
    }
    public virtual void FinishAttack()
    {
        isAnimationFinished = true;
    }
    public override void Exit()
    {
        base.Exit();
    }
}
