using UnityEngine;

public class DeadState : State
{
    public DeadState(Entity entity, FiniteStateMachine stateMachine, int animBoolHash) 
        : base(entity, stateMachine, animBoolHash)
    {
    }

    public override void Enter()
    {
        base.Enter();
        entity.StopMovement(); 

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SpawnMana(entity.transform.position);
        }
        if (entity.entityData.deathAoEPrefab != null)
        {
            ObjectPoolManager.Instance.GetFromPool(entity.entityData.deathAoEPrefab, entity.transform.position, Quaternion.identity);
        }

        Collider2D col = entity.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }
}