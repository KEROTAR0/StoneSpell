using UnityEngine;

public class StunnedState : State
{
    protected GameObject currentVfxInstance;
    private bool isSleepState; // Dùng để chọn đúng loại VFX
    
    public StunnedState(EnemyController entity, FiniteStateMachine stateMachine, int animBoolHash) 
        : base(entity, stateMachine, animBoolHash)
    {
    }

    // Hàm gọi từ EnemyController khi bị ngắt
    public void SetupStun(bool isSleep)
    {
        this.isSleepState = isSleep;
    }

    public override void Enter()
    {
        base.Enter();
        entity.StopMovement();

        // Xin VFX từ GameManager thay vì từ Entity Data
        if (GameManager.Instance != null)
        {
            if (isSleepState)
                currentVfxInstance = GameManager.Instance.SpawnSleepVFX(entity.transform, entity.entityData.stunVFXPrefabHeight);
            else
                currentVfxInstance = GameManager.Instance.SpawnStunVFX(entity.transform, entity.entityData.stunVFXPrefabHeight);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
        // Thoát State khi CoreCCComponent báo hết giờ
        if (entity.CC != null && !entity.CC.isStunned)
        {
            entity.ReturnToDefaultState();
        }
    }

    public override void Exit()
    {
        base.Exit();

        if (currentVfxInstance != null)
        {
            currentVfxInstance.transform.SetParent(null);
            ObjectPoolManager.Instance.AddToPool(currentVfxInstance);
        }
    }
}