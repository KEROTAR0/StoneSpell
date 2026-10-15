using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(SpellController))]
public class SpellCollision : MonoBehaviour
{
    private SpellController controller;
    private SpellMovement movement;
    
    public bool isHitboxActive { get; private set; } = false;
    private HashSet<GameObject> hitTargets = new HashSet<GameObject>();
    private int currentBounces;
    private int currentBlocks;

    private void Awake()
    {
        controller = GetComponent<SpellController>();
        movement = GetComponent<SpellMovement>();
    }

    public void Init()
    {
        currentBounces = controller.currentStats.maxBounces;
        currentBlocks = controller.currentStats.maxBlocks;
        isHitboxActive = false;
        hitTargets.Clear();
        
        GetComponent<Collider2D>().enabled = true;
        if (TryGetComponent(out Animator animChild))
        {
            animChild.Rebind();
            animChild.Update(0f);
        }
    }

    public void AnimationEvent_EnableHitbox()
    {
        isHitboxActive = true;
        hitTargets.Clear(); 
    }

    public void AnimationEvent_DisableHitbox()
    {
        isHitboxActive = false;
        if (movement != null) movement.SetFinishedAttackFrame(true);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        D_Spell baseData = controller.baseSpellData;
        int colLayer = collision.gameObject.layer;

        if (isHitboxActive && (baseData.isPenetrating || baseData.isLaser))
        {
            // Xuyên thấu trúng Quái hoặc Đồ vật
            if ((GameLayers.EnemyAndDamageableMask.value & (1 << colLayer)) > 0)
            {
                if (!hitTargets.Contains(collision.gameObject))
                {
                    Vector2 hitPoint = collision.ClosestPoint(transform.position);
                    controller.ApplyEffectsToTarget(collision.gameObject);
                    hitTargets.Add(collision.gameObject);
                    controller.SpawnImpactVFXAtPosition(hitPoint);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!gameObject.activeInHierarchy || !collision.gameObject.activeInHierarchy) return;
        D_Spell baseData = controller.baseSpellData;
        int colLayer = collision.gameObject.layer;

        // 1. Kiểm tra Va chạm Tường
        if ((GameLayers.WallMask.value & (1 << colLayer)) > 0 || 
            ((GameLayers.HighObstacleMask.value & (1 << colLayer)) > 0 && !baseData.isPassThroughtHighObstacle))
        {
            if (baseData.isPenetrating) return; 

            if (currentBounces > 0 && movement != null)
            {
                currentBounces--;
                movement.PerformBounce();
            }
            else
            {
                if (TryGetComponent(out Rigidbody2D rb)) rb.linearVelocity = Vector2.zero; 
                Vector2 hitPoint = collision.ClosestPoint(transform.position);
                controller.SpawnImpactVFXAtPosition(hitPoint);
                controller.Despawn();
            }
            return; 
        }

        // 2. Tương tác với Đạn địch
        if ((GameLayers.EnemyAttackMask.value & (1 << colLayer)) > 0)
        {
            if (collision.TryGetComponent(out EnemyProjectile enemyProj))
            {
                // BỎ QUA TelegraphedAoE (Sét đánh, Cột lửa từ dưới đất...)
                // Không thể Block hay Reflect những loại này
                if (enemyProj.attackData.behavior == AttackBehavior.TelegraphedAoE) return;

                Vector2 hitPoint = collision.ClosestPoint(transform.position);
                controller.SpawnImpactVFXAtPosition(hitPoint);

                // REFLECT
                if (baseData.canReflect)
                {
                    // A. Phản đòn Cận chiến (Parry Melee)
                    if (enemyProj.attackData.behavior == AttackBehavior.StationaryMelee)
                    {
                        EnemyController enemy = enemyProj.GetComponentInParent<EnemyController>();
                        if (enemy != null && enemy.TryGetComponent(out IDamageable damageable))
                        {
                            AttackDetails details = new AttackDetails 
                            { 
                                damageAmount = enemyProj.attackData.damage * 10f, 
                                position = transform.position,
                                knockbackForce = 5f 
                            };
                            damageable.TakeDamage(details);
                        }
                        enemyProj.Despawn();
                    }
                    // B. Phản Đạn bay / Bãi lầy (Reflect Projectiles)
                    else 
                    {
                        enemyProj.ReflectedByPlayer();
                    }

                    if (baseData.canBlock)
                    {
                        currentBlocks--;
                        if (currentBlocks <= 0) controller.Despawn();
                    }
                }
                // BLOCK
                else if (baseData.canBlock)
                {
                    enemyProj.Despawn(); 

                    currentBlocks--;
                    if (currentBlocks <= 0) controller.Despawn();
                }
            }
            return;
        }

        // 3. Trúng Mục Tiêu (Quái hoặc Đồ vật)
        if (!baseData.isPenetrating && !baseData.isLaser && (GameLayers.EnemyAndDamageableMask.value & (1 << colLayer)) > 0)
        {
            Vector2 hitPoint = (Vector2)transform.position + ((Vector2)collision.transform.position - (Vector2)transform.position) * 0.5f;
            controller.ApplyEffectsToTarget(collision.gameObject);
            controller.SpawnImpactVFXAtPosition(hitPoint);
            
            if (baseData.isProjectile && !baseData.hasAura) controller.Despawn();
        }
    }
}