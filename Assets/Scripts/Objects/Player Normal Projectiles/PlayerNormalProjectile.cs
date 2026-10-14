using UnityEngine;

public class PlayerNormalProjectile : MonoBehaviour
{
    [SerializeField] private float lifeTime = 3f; 
    
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 15f;      
    [SerializeField] private bool autoRotate = true; 
    [SerializeField] private GameObject impactVFX; 

    [Header("Spin Settings")]
    [SerializeField] private bool isSpinning = true; 
    [SerializeField] private float spinSpeed = 720f; 

    [Header("Damage Settings")]
    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private float stunDamageAmount = 1f;
    [SerializeField] private float knockbackForce = 0f;

    private float timeActivated;

    private void OnEnable()
    {
        timeActivated = Time.time;
    }

    private void Update()
    {
        if (isSpinning) transform.Rotate(0, 0, spinSpeed * Time.deltaTime);

        if (Time.time >= (timeActivated + lifeTime))
        {
            SpawnImpactVFX();
            ReturnToPool();
        }
    }

    public void Fire(Vector2 direction)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = direction * speed;

        if (autoRotate)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        int colLayer = collision.gameObject.layer;

        // 1. Kiểm tra va chạm với Kẻ địch hoặc Vật thể phá hủy được
        if ((GameLayers.EnemyAndDamageableMask.value & (1 << colLayer)) > 0)
        {
            if (collision.TryGetComponent(out IDamageable damageable))
            {
                AttackDetails attackDetails = new AttackDetails
                {
                    position = transform.position, 
                    damageAmount = this.damageAmount,
                    stunDamageAmount = this.stunDamageAmount,
                    knockbackForce = this.knockbackForce
                };
                damageable.TakeDamage(attackDetails);
                
                SpawnImpactVFX();
                
                if (PlayerController.Instance != null && PlayerController.Instance.Mana != null)
                {
                    PlayerController.Instance.Mana.RecordNormalHit(collision.transform.position);
                }
                
                ReturnToPool(); 
                return; 
            }
        }

        // 2. Kiểm tra va chạm với Tường (Obstacle)
        // Dùng WallAndHighObstacleMask để đạn thường bị chặn bởi tường cao, không bị chặn bởi vũng nước (LowObstacle)
        if ((GameLayers.WallAndHighObstacleMask.value & (1 << colLayer)) > 0)
        {
            SpawnImpactVFX();
            ReturnToPool(); 
        }
    }

    private void SpawnImpactVFX()
    {
        if (impactVFX != null)
            ObjectPoolManager.Instance.GetFromPool(impactVFX, transform.position, Quaternion.identity);
    }

    private void ReturnToPool()
    {
        ObjectPoolManager.Instance.AddToPool(gameObject);
    }
}