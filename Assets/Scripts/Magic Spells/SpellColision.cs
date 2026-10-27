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
        
        hitTargets.Clear();
        
        if (TryGetComponent(out Animator animChild))
        {
            animChild.Rebind();
            animChild.Update(0f);
        }

        Collider2D col = GetComponent<Collider2D>();

        // ĐÃ SỬA: Bắt buộc phải có dấu tick UseMultiStateAnimation thì mới được phép tắt Hitbox
        if (controller.useMultiStateAnimation && controller.baseSpellData.castingTime > 0)
        {
            isHitboxActive = false; 
            if (col != null) col.enabled = false; 
        }
        else
        {
            isHitboxActive = true; 
            if (col != null) col.enabled = true; 
        }
    }

    private void EnableHitboxDelayed()
    {
        isHitboxActive = true;
        hitTargets.Clear(); 
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = true; // BẬT LẠI KHI ĐÃ GỒNG XONG
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
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        
        if (movement != null) movement.SetFinishedAttackFrame(true);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isHitboxActive) TryDamageTarget(collision);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!gameObject.activeInHierarchy || !collision.gameObject.activeInHierarchy) return;
        int colLayer = collision.gameObject.layer;

        // =======================================================
        // 1. ƯU TIÊN SỐ 1: TƯƠNG TÁC VỚI ĐẠN VÀ ĐÒN ĐÁNH CỦA ĐỊCH
        // =======================================================
        if ((GameLayers.EnemyAttackMask.value & (1 << colLayer)) > 0)
        {
            if (collision.TryGetComponent(out EnemyProjectile enemyProj))
            {
                Vector2 hitPoint = collision.ClosestPoint(transform.position);
                controller.SpawnImpactVFXAtPosition(hitPoint);

                // REFLECT (PHẢN ĐÒN)
                if (controller.currentStats.canReflect)
                {
                    enemyProj.ReflectedByPlayer();

                    if (currentBlocks > 0)
                    {
                        currentBlocks--;
                        if (currentBlocks <= 0) controller.Despawn();
                    }
                }
                // BLOCK (ĐỠ ĐÒN)
                else if (currentBlocks > 0)
                {
                    enemyProj.Despawn(); 

                    currentBlocks--;
                    if (currentBlocks <= 0) controller.Despawn();
                }
            }
            return; // Đã xử lý đạn địch xong thì thoát luôn, không cho chạy xuống dưới
        }

        // =======================================================
        // 2. KIỂM TRA VA CHẠM TƯỜNG (Sau khi đã loại trừ đạn địch)
        // =======================================================
        if ((GameLayers.WallMask.value & (1 << colLayer)) > 0 || 
            ((GameLayers.HighObstacleMask.value & (1 << colLayer)) > 0 && !controller.currentStats.isPassThroughtHighObstacle))
        {
            if (controller.currentStats.isPenetrating || controller.currentStats.isSelfTargeted) return;

            if (currentBounces > 0 && movement != null)
            {
                if (Time.time < lastBounceTime + 0.05f) return; 
                
                lastBounceTime = Time.time;
                currentBounces--;
                
                Vector2 currentDir = Vector2.zero;
                if (TryGetComponent(out Rigidbody2D rbBounce)) currentDir = rbBounce.linearVelocity.normalized;

                Vector2 hitPoint = collision.ClosestPoint(transform.position);
                Vector2 surfaceNormal = ((Vector2)transform.position - hitPoint).normalized;
                
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

        if ((GameLayers.EnemyAndDamageableMask.value & (1 << colLayer)) > 0)
        {
            // ==============================================================
            // ĐÃ SỬA: TÌM ROOT OBJECT (ĐỐI TƯỢNG GỐC)
            // Thay vì lưu Collider con, ta tìm gốc chứa EffectReceiver/Health
            // Điều này đảm bảo dù quái có 10 cái Collider, nó chỉ nhận 1 lần sát thương!
            // ==============================================================
            EffectReceiver receiver = collision.GetComponentInParent<EffectReceiver>();
            
            // Nếu tìm thấy gốc, lấy gốc. Nếu không, lấy chính nó.
            GameObject rootObject = receiver != null ? receiver.gameObject : collision.gameObject;

            if (!hitTargets.Contains(rootObject))
            {
                Vector2 hitPoint = collision.ClosestPoint(transform.position);
                
                // Truyền rootObject vào hàm Apply thay vì truyền Collider con
                controller.ApplyEffectsToTarget(rootObject);
                controller.SpawnImpactVFXAtPosition(hitPoint);
                
                // Đánh dấu Root Object đã bị đánh trúng
                hitTargets.Add(rootObject);

                if (!baseData.isPenetrating && !baseData.isLaser && baseData.isProjectile && !baseData.hasAura)
                {
                    controller.Despawn();
                }
            }
        }
    }
}