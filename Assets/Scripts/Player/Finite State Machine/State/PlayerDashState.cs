using UnityEngine;

public class PlayerDashState : PlayerState
{
    private Vector2 dashDirection;
    private float lastAITime;
    private float distanceBetweenImages = 0.1f;
    public PlayerDashState(Player player, PlayerStateMachine stateMachine, D_Player playerData, string animBoolName) : base(player, stateMachine, playerData, animBoolName)
    {
    }
    public override void Enter()
    {
        base.Enter();
        player.isInvincible = true;
        player.InputHandler.UsedDashInput();
        Vector2 rawInput = player.InputHandler.movementInput;

        ObjectPoolManager.Instance.GetFromPool(playerData.afterImagePrefab, player.transform.position, player.transform.rotation);
        lastAITime = Time.time;

        if (rawInput.sqrMagnitude > 0.01f)
        {
            // 2. Làm tròn toạ độ để ép lướt chuẩn 4 hướng hoặc 8 hướng (tránh lỗi Joystick drift)
            dashDirection = new Vector2(Mathf.Round(rawInput.x), Mathf.Round(rawInput.y)).normalized;
            
            // Cập nhật lại hướng nhìn để đồng bộ
            player.facingDirection = dashDirection;
        }
        else
        {
            // 3. Nếu đang đứng yên, lướt theo hướng mặt hiện tại
            dashDirection = player.facingDirection.normalized; 
        }
    }
    public override void DoCheck()
    {
        base.DoCheck();
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (Time.time >= (lastAITime + distanceBetweenImages))
        {
            ObjectPoolManager.Instance.GetFromPool(playerData.afterImagePrefab, player.transform.position, player.transform.rotation);
            lastAITime = Time.time;
        }

        if (Time.time >= startTime + playerData.dashTime)
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

        player.rb.linearVelocity = dashDirection * playerData.dashVelocity;
    }
    public override void Exit()
    {
        base.Exit();

        player.dashCooldownTimer = playerData.dashCooldown;
        
        player.Stop();
        player.isInvincible = false;
    }
}
