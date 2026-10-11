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
        currentBounces = controller.spellData.maxBounces;
        currentBlocks = controller.spellData.maxBlocks;
        isHitboxActive = false;
        hitTargets.Clear();
        
        GetComponent<Collider2D>().enabled = true;
        if (TryGetComponent(out Animator animChild))
        {
            animChild.Rebind();
            animChild.Update(0f);
        }
    }

    // Gắn vào Animation Event
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
        D_Spell data = controller.spellData;
        if (isHitboxActive && (data.isPenetrating || data.isLaser))
        {
            if ((controller.targetLayer.value & (1 << collision.gameObject.layer)) > 0)
            {
                if (!hitTargets.Contains(collision.gameObject))
                {
                    controller.ApplyEffectsToTarget(collision.gameObject);
                    hitTargets.Add(collision.gameObject);
                    controller.SpawnImpactVFX();
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        D_Spell data = controller.spellData;
        int colLayer = collision.gameObject.layer;

        // 1. CHẠM TƯỜNG
        if ((controller.wallLayer.value & (1 << colLayer)) > 0 || 
            ((controller.highObstacleLayer.value & (1 << colLayer)) > 0 && !data.isPassThroughtHighObstacle))
        {
            if (data.isPenetrating) return; // Nếu có thuộc tính xuyên thấu thì bỏ qua

            if (currentBounces > 0 && movement != null)
            {
                currentBounces--;
                movement.PerformBounce();
            }
            else
            {
                // SỬA LỖI XUYÊN TƯỜNG: Hủy ngay vận tốc vật lý trước khi Despawn
                if (TryGetComponent(out Rigidbody2D rb))
                {
                    rb.linearVelocity = Vector2.zero; 
                }
                
                controller.SpawnImpactVFX();
                controller.Despawn();
            }
            return; // Đảm bảo thoát khỏi hàm ngay lập tức
        }

        // 2. CHẠM ĐẠN ĐỊCH (BLOCK / REFLECT)
        if ((controller.enemyProjectileLayer.value & (1 << colLayer)) > 0)
        {
            if (collision.TryGetComponent(out EnemyProjectile enemyProj))
            {
                if (enemyProj.attackData.behavior == AttackBehavior.NormalProjectile || enemyProj.attackData.behavior == AttackBehavior.SpawnAoEOnHit)
                {
                    if (data.canReflect) enemyProj.ReflectedByPlayer();
                    else if (data.canBlock || data.isProjectile)
                    {
                        controller.SpawnImpactVFX();
                        if (data.canBlock)
                        {
                            currentBlocks--;
                            if (currentBlocks <= 0) controller.Despawn();
                        }
                        else controller.Despawn();
                        
                        enemyProj.Despawn(); 
                    }
                }
            }
            return;
        }

        // 3. CHẠM MỤC TIÊU BÌNH THƯỜNG
        if (!data.isPenetrating && !data.isLaser && (controller.targetLayer.value & (1 << colLayer)) > 0)
        {
            controller.ApplyEffectsToTarget(collision.gameObject);
            controller.SpawnImpactVFX();
            
            if (data.isProjectile && !data.hasAura) controller.Despawn();
        }
    }
}