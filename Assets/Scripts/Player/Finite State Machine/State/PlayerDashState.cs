using UnityEngine;

public class PlayerDashState : PlayerState
{
    private Vector2 dashDirection;
    private float lastAITime;
    private float distanceBetweenImages = 0.1f;

    public PlayerDashState(PlayerController player, PlayerStateMachine stateMachine, D_Player playerData, int animBoolHash) 
        : base(player, stateMachine, playerData, animBoolHash)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        // Cấp khung hình vô địch (I-Frame) nếu HealthComponent có hỗ trợ
        // Tạm thời giữ biến isInvincible của PlayerController
        player.isInvincible = true; 
        
        player.InputHandler.UsedDashInput();
        Vector2 rawInput = player.InputHandler.movementInput;

        SpawnAfterImage();

        if (rawInput.sqrMagnitude > 0.01f)
        {
            dashDirection = new Vector2(Mathf.Round(rawInput.x), Mathf.Round(rawInput.y)).normalized;
            player.facingDirection = dashDirection;
            player.CheckIfShouldFlip(dashDirection.x); // Ép lật mặt khi lướt chéo
        }
        else
        {
            dashDirection = player.facingDirection.normalized; 
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (Time.time >= (lastAITime + distanceBetweenImages))
        {
            SpawnAfterImage();
        }

        if (Time.time >= startTime + playerData.dashTime)
        {
            if (input.sqrMagnitude > 0.01f)
                stateMachine.ChangeState(player.MoveState);
            else
                stateMachine.ChangeState(player.IdleState);
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
        player.StopVelocity();
        player.isInvincible = false;
    }

    private void SpawnAfterImage()
    {
        if (ObjectPoolManager.Instance != null && playerData.afterImagePrefab != null)
        {
            ObjectPoolManager.Instance.GetFromPool(playerData.afterImagePrefab, player.transform.position, player.transform.rotation);
            lastAITime = Time.time;
        }
    }
}