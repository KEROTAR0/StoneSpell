using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyProjectile : MonoBehaviour
{
    public D_EnemyAttack attackData;
    
    private Rigidbody2D rb;
    private Vector2 currentDirection;
    private float spawnTime;
    private bool isReflected;
    private Collider2D col;

    public void Initialize(Vector2 direction)
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        currentDirection = direction.normalized;
        spawnTime = Time.time;
        isReflected = false;
        gameObject.layer = GameLayers.EnemyAttackLayerIndex;

        if (attackData.behavior == AttackBehavior.TelegraphedAoE)
        {
            col.enabled = false;
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            col.enabled = true;
            float speed = attackData.behavior == AttackBehavior.StationaryMelee ? 0f : attackData.moveSpeed;
            rb.linearVelocity = currentDirection * speed;
            
            if (speed > 0 || attackData.behavior == AttackBehavior.StationaryMelee)
            {
                if (attackData.rotateToDirection) 
                {
                    float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0, 0, angle);
                }
                else
                {
                    transform.rotation = Quaternion.identity;
                }
            }
        }
    }

    private void Update()
    {
        // Melee sẽ tự hủy qua Animation Event, Đạn bay sẽ tự hủy qua LifeTime
        if (attackData.behavior != AttackBehavior.StationaryMelee && Time.time >= spawnTime + attackData.lifeTime)
        {
            Despawn();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {   
        if (!gameObject.activeInHierarchy || !collision.gameObject.activeInHierarchy) return;
        int colLayer = collision.gameObject.layer;

        // 1. Nếu đã bị phản đòn, Đạn Quái trở thành Đạn Player, chỉ đánh Quái
        if (isReflected && (GameLayers.EnemyAndDamageableMask.value & (1 << colLayer)) > 0)
        {
            if (collision.TryGetComponent(out IDamageable damageable))
            {
                AttackDetails details = new AttackDetails 
                { 
                    damageAmount = Mathf.RoundToInt(attackData.damage * 10), 
                    position = transform.position 
                };
                damageable.TakeDamage(details);
            }
            if (collision.TryGetComponent(out EffectReceiver receiver))
            {
                if (attackData.hitEffects != null && attackData.hitEffects.Count > 0)
                {
                    foreach (var effect in attackData.hitEffects) receiver.ApplyEffect(effect, transform.position);
                }
            }

            Despawn();
            return;
        }

        // 2. Chạm vào Khiên/Phép của Player -> Nhường cho Khiên tự xử lý
        if ((GameLayers.PlayerAttackMask.value & (1 << colLayer)) > 0)
        {
            return; 
        }

        // 3. Nếu Đạn/Đòn Quái chạm vào Player
        if (!isReflected && (GameLayers.PlayerAndDamageableMask.value & (1 << colLayer)) > 0)
        {
            // =========================================================
            // BỘ LỌC CHỐNG XUYÊN KHIÊN DỰA THEO Ý TƯỞNG CỦA BẠN
            // Quái đánh trúng Player, nhưng phải tìm xem quanh Player
            // có đang bọc cái Khiên (isSelfTargeted) nào không!
            // =========================================================
            SpellController[] activeSpells = collision.GetComponentsInChildren<SpellController>();
            foreach (var spell in activeSpells)
            {
                // Kiểm tra xem đó có phải là Khiên có khả năng Đỡ đòn hoặc Phản đòn không
                if (spell.currentStats.isSelfTargeted && (spell.baseSpellData.canReflect || spell.currentStats.maxBlocks > 0))
                {
                    // NẾU CÓ KHIÊN -> Lập tức quay xe, không gây sát thương cho Player!
                    // Quyền xử lý phản đòn sẽ được nhường lại cho hàm OnTriggerEnter2D bên SpellCollision của Khiên.
                    return; 
                }
            }

            // Nếu quét qua không thấy cái Khiên nào, thì Player ăn sát thương bình thường
            if (collision.TryGetComponent(out IDamageable damageable))
            {
                AttackDetails details = new AttackDetails 
                { 
                    damageAmount = attackData.damage, 
                    position = transform.position 
                };
                damageable.TakeDamage(details);
            }

            // Gắn Effect
            if (collision.TryGetComponent(out EffectReceiver receiver) && attackData.hitEffects != null)
            {
                foreach (var effect in attackData.hitEffects) receiver.ApplyEffect(effect, transform.position);
            }
            Despawn();
        }
        
        // 4. Va chạm tường
        else if ((GameLayers.WallAndHighObstacleMask.value & (1 << colLayer)) > 0)
        {
            Despawn();
        }
    }

    // Hàm gọi từ Animation Event ở frame cuối của đòn Melee
    public void AnimationEvent_FinishMelee()
    {
        Despawn();
    }
    public void AnimationEvent_EnableHitbox()
    {
        if (col != null) col.enabled = true;
    }

    // Gọi ở ngay 1-2 frame sau đó để tắt đi (tránh việc Player đi vào vùng khói sau khi sét đã đánh xong mà vẫn mất máu)
    public void AnimationEvent_DisableHitbox()
    {
        if (col != null) col.enabled = false;
    }

    // Khi Player dùng khiên phản đòn
    public void ReflectedByPlayer()
    {
        isReflected = true;
        currentDirection = -currentDirection;
        rb.linearVelocity = currentDirection * attackData.moveSpeed;
        gameObject.layer = GameLayers.PlayerAttackLayerIndex; 
        
        // FIX 1: CẬP NHẬT GÓC XOAY CHO ĐẠN
        if (attackData.rotateToDirection) 
        {
            float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    public void Despawn()
    {
        // Cơ chế sinh vũng lầy (Puddle) hoặc Vụ nổ AoE khi đạn kết thúc
        if (attackData.behavior == AttackBehavior.SpawnAoEOnHit && attackData.spawnOnDeathPrefab != null)
        {
            ObjectPoolManager.Instance.GetFromPool(attackData.spawnOnDeathPrefab, transform.position, Quaternion.identity);
        }

        rb.linearVelocity = Vector2.zero;
        
        ObjectPoolManager.Instance.AddToPool(gameObject); 
    }
}