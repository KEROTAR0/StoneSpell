using UnityEngine;

public class SelfDestructState : State
{
    public SelfDestructState(Entity entity, FiniteStateMachine stateMachine, string animBoolName) 
        : base(entity, stateMachine, animBoolName)
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
        Collider2D[] colliders = Physics2D.OverlapCircleAll(entity.transform.position, entity.entityData.shortRangeAction, entity.entityData.selfDestructTargetLayer);
        
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
            SpellPool.Instance.GetFromPool(entity.entityData.selfDestructVFX, entity.transform.position, Quaternion.identity);
        }

        entity.Die();
    }
}