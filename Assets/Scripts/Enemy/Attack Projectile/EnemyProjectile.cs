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
        gameObject.layer = LayerMask.NameToLayer("EnemyAttack");

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
        // Nếu đạn đã bị Player phản lại, nó sẽ nhắm vào Enemy
        if (isReflected && collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            // Trừ máu quái vật...
            Despawn();
            return;
        }

        // Đạn đánh trúng Player
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                AttackDetails details = new AttackDetails 
                { 
                    damageAmount = attackData.damage, 
                    position = transform.position 
                };
                damageable.TakeDamage(details);

                // Áp dụng Status Ailment lên Player (Cần một hàm ApplyAilment bên Player)
                // Player.Instance.ApplyAilment(attackData.ailmentType, attackData.ailmentDuration, attackData.ailmentValue);
            }
            Despawn();
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
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
        gameObject.layer = LayerMask.NameToLayer("PlayerSpell"); 
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