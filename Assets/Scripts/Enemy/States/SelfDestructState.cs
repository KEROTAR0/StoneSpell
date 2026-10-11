using UnityEngine;

public class SelfDestructState : State
{
    public SelfDestructState(Entity entity, FiniteStateMachine stateMachine, int animBoolHash) 
        : base(entity, stateMachine, animBoolHash)
    {
    }

    public override void Enter()
    {
        base.Enter();
        entity.StopMovement();
        Explode();
    }

    private void Explode()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(entity.transform.position, entity.entityData.shortRangeAction, GameLayers.PlayerAndDamageableMask);
        
        foreach (var col in colliders)
        {
            if (col.gameObject == entity.gameObject) continue;

            IDamageable damageable = col.GetComponent<IDamageable>();
            if (damageable != null)
            {
                AttackDetails details = new AttackDetails 
                { 
                    damageAmount = entity.entityData.selfDestructDamage, 
                    position = entity.transform.position, 
                    knockbackForce = entity.entityData.knockbackForce 
                };
                damageable.TakeDamage(details);
            }
        }

        if (entity.entityData.selfDestructVFX != null)
        {
            ObjectPoolManager.Instance.GetFromPool(entity.entityData.selfDestructVFX, entity.transform.position, Quaternion.identity);
        }

        entity.Die();
    }
}