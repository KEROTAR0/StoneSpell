using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DestructibleObject : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 50f;
    private float currentHealth;
    private Rigidbody2D rb;

    [Header("On Death: Drops")]
    public bool dropMana = true;

    [Header("On Death: Explosion")]
    public bool explodeOnDeath = false;
    public float explosionRadius = 2.5f;
    public float explosionDamage = 20f;
    public float explosionKnockback = 5f;
    public GameObject explosionVFX;
    
    // Tùy chọn: Nhét thêm hiệu ứng bất lợi vào vụ nổ
    public List<EffectData> explosionEffects = new List<EffectData>();

    [Header("On Death: Shrapnel (Văng mảnh đạn)")]
    public bool spawnShrapnel = false;
    public GameObject shrapnelPrefab; // Prefab đạn của Quái hoặc Player đều được
    public int shrapnelCount = 8;
    public float shrapnelSpeed = 10f;

    private void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        
        gameObject.layer = GameLayers.DamageableLayerIndex;
    }

    public void TakeDamage(AttackDetails attackDetails)
    {
        currentHealth -= attackDetails.damageAmount;

        if (rb != null)
        {
            Vector2 knockbackDirection = (transform.position - (Vector3)attackDetails.position).normalized;
            rb.AddForce(knockbackDirection * attackDetails.knockbackForce, ForceMode2D.Impulse);
        }

        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        if (dropMana && GameManager.Instance != null)
            GameManager.Instance.SpawnMana(transform.position);

        if (explodeOnDeath) Detonate();
        if (spawnShrapnel) SpawnShrapnels();

        // Thay vì Destroy, nếu thùng gỗ là tĩnh thì Destroy cũng được, 
        // nhưng nếu spawn nhiều thì nên đưa về Pool.
        Destroy(gameObject); 
    }

    private void Detonate()
    {
        if (explosionVFX != null)
            ObjectPoolManager.Instance.GetFromPool(explosionVFX, transform.position, Quaternion.identity);

        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, explosionRadius, GameLayers.PlayerAndDamageableMask | GameLayers.EnemyAndDamageableMask);
        
        foreach (Collider2D target in targets)
        {
            if (target.gameObject == gameObject) continue;

            if (target.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(new AttackDetails { 
                    damageAmount = explosionDamage, 
                    position = transform.position, 
                    knockbackForce = explosionKnockback 
                });
            }

            if (target.TryGetComponent(out EffectReceiver receiver) && explosionEffects.Count > 0)
            {
                foreach (var effect in explosionEffects) receiver.ApplyEffect(effect, transform.position);
            }
        }
    }

    private void SpawnShrapnels()
    {
        if (shrapnelPrefab == null) return;
        
        float angleStep = 360f / shrapnelCount;
        for (int i = 0; i < shrapnelCount; i++)
        {
            // Tính góc bắn tỏa ra 360 độ
            float angle = i * angleStep;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            
            // Lấy đạn từ Pool
            GameObject shrapnel = ObjectPoolManager.Instance.GetFromPool(shrapnelPrefab, transform.position, Quaternion.identity);
            
            // Bắn đi bằng hàm Fire() nếu là đạn Player, hoặc Initialize() nếu là đạn Enemy
            if (shrapnel.TryGetComponent(out PlayerNormalProjectile proj))
                proj.Fire(dir); // Gợi ý: Sửa hàm Fire() để nhận thêm tham số Speed nếu muốn
            else if (shrapnel.TryGetComponent(out EnemyProjectile enemyProj))
                enemyProj.Initialize(dir); 
        }
    }
}