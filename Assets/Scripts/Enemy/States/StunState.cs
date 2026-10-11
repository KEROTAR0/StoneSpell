using UnityEngine;

public class StunnedState : State
{
    protected float stunDuration;
    protected bool isStunFinished;
    protected GameObject stunVfxPrefab;
    protected GameObject currentVfxInstance;
    public StunnedState(Entity entity, FiniteStateMachine stateMachine, int animBoolHash) 
        : base(entity, stateMachine, animBoolHash)
    {
    }

    public void SetupStun(float duration, GameObject vfxPrefab)
    {
        this.stunDuration = duration;
        this.stunVfxPrefab = vfxPrefab;
    }

    public override void Enter()
    {
        base.Enter();
        isStunFinished = false;
        entity.StopMovement();
        entity.SetStunStatus(true);

        if (stunVfxPrefab != null)
        {
            Vector3 vfxPos = entity.transform.position + Vector3.up * entity.entityData.stunVFXPrefabHeight; 
            currentVfxInstance = ObjectPoolManager.Instance.GetFromPool(stunVfxPrefab, vfxPos, Quaternion.identity);
            
            currentVfxInstance.transform.SetParent(entity.transform);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (Time.time >= startTime + stunDuration)
        {
            entity.ReturnToDefaultState();
        }
    }

    public override void Exit()
    {
        base.Exit();
        entity.SetLastStunEndTime(); 
        entity.SetStunStatus(false);

        if (currentVfxInstance != null)
        {
            currentVfxInstance.transform.SetParent(null);
            ObjectPoolManager.Instance.AddToPool(currentVfxInstance);
        }
    }
}