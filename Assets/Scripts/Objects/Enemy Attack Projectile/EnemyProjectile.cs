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

        if (isReflected && (GameLayers.EnemyAndDamageableMask.value & (1 << colLayer)) > 0)
        {
            // 1. Gây sát thương
            if (collision.TryGetComponent(out IDamageable damageable))
            {
                AttackDetails details = new AttackDetails 
                { 
                    damageAmount = Mathf.RoundToInt(attackData.damage * 10), // Đạn bật lại giữ nguyên sát thương (hoặc bạn có thể nhân 2 ở đây)
                    position = transform.position 
                };
                damageable.TakeDamage(details);
            }
            // 2. Trả lại Effect cho quái
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

        if (!isReflected && (GameLayers.PlayerAndDamageableMask.value & (1 << colLayer)) > 0)
        {
            // 1. Gây sát thương cơ bản (Base Damage)
            if (collision.TryGetComponent(out IDamageable damageable))
            {
                AttackDetails details = new AttackDetails 
                { 
                    damageAmount = attackData.damage, 
                    position = transform.position 
                };
                damageable.TakeDamage(details);
            }

            // 2. NHÉT TOÀN BỘ EFFECT VÀO EFFECT RECEIVER (MỚI)
            if (collision.TryGetComponent(out EffectReceiver receiver) && attackData.hitEffects != null)
            {
                foreach (var effect in attackData.hitEffects) receiver.ApplyEffect(effect, transform.position);
            }
            Despawn();
        }
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