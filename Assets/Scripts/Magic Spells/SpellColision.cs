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
    private float lastBounceTime = 0f;

    private void Awake()
    {
        controller = GetComponent<SpellController>();
        movement = GetComponent<SpellMovement>();
    }

    public void Init()
    {
        currentBounces = controller.currentStats.maxBounces;
        currentBlocks = controller.currentStats.maxBlocks;
        
        if (controller.baseSpellData.isProjectile)
        {
            isHitboxActive = true; 
        }
        else
        {
            isHitboxActive = false;
        }
        
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
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
            col.enabled = true;
        }
    }

    public void AnimationEvent_DisableHitbox()
    {
        isHitboxActive = false;
        if (movement != null) movement.SetFinishedAttackFrame(true);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isHitboxActive) TryDamageTarget(collision);
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
            if (baseData.isPenetrating || baseData.isSelfTargeted) return;

            if (currentBounces > 0 && movement != null)
            {
                // CHỐNG VA CHẠM KÉP Ở GÓC VUÔNG: Chỉ cho phép nảy tối đa 1 lần mỗi 0.05 giây
                if (Time.time < lastBounceTime + 0.05f) return; 
                
                lastBounceTime = Time.time;
                currentBounces--;
                
                Vector2 currentDir = Vector2.zero;
                if (TryGetComponent(out Rigidbody2D rbBounce)) currentDir = rbBounce.linearVelocity.normalized;

                // CÁCH TÍNH GÓC MẶT PHẲNG (NORMAL) CHUẨN NHẤT CHO VẬT THỂ TRÒN:
                // Lấy vector nối từ tâm vật thể đập vuông góc ra mép va chạm
                Vector2 hitPoint = collision.ClosestPoint(transform.position);
                Vector2 surfaceNormal = ((Vector2)transform.position - hitPoint).normalized;
                
                // Nếu đạn bay quá nhanh lún thẳng vào tâm, phép trừ bị bằng 0 -> Dùng Raycast cứu viện
                if (surfaceNormal == Vector2.zero) 
                {
                    RaycastHit2D hit = Physics2D.Raycast((Vector2)transform.position - currentDir * 0.5f, currentDir, 2f, GameLayers.WallMask.value | GameLayers.HighObstacleMask.value);
                    surfaceNormal = hit.collider != null ? hit.normal : -currentDir;
                }

                movement.PerformBounce(surfaceNormal);
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
                // if (enemyProj.attackData.behavior == AttackBehavior.TelegraphedAoE) return;

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
        if (isHitboxActive) 
        {
            TryDamageTarget(collision);
        }
    }
    private void TryDamageTarget(Collider2D collision)
    {
        D_Spell baseData = controller.baseSpellData;
        int colLayer = collision.gameObject.layer;

        // Nếu chạm trúng Quái hoặc Đồ vật phá được
        if ((GameLayers.EnemyAndDamageableMask.value & (1 << colLayer)) > 0)
        {
            // Tránh việc 1 viên đạn giật cùng 1 mục tiêu 2 lần
            if (!hitTargets.Contains(collision.gameObject))
            {
                Vector2 hitPoint = (Vector2)transform.position + ((Vector2)collision.transform.position - (Vector2)transform.position) * 0.5f;
                controller.ApplyEffectsToTarget(collision.gameObject);
                controller.SpawnImpactVFXAtPosition(hitPoint);
                
                hitTargets.Add(collision.gameObject);

                // NẾU KHÔNG PHẢI XUYÊN THẤU -> HỦY VIÊN ĐẠN NGAY LẬP TỨC
                if (!baseData.isPenetrating && !baseData.isLaser && baseData.isProjectile && !baseData.hasAura)
                {
                    controller.Despawn();
                }
            }
        }
    }
}