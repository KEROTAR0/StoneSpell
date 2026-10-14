using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Không cần HealthComponent nữa
[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class ManaShard : MonoBehaviour
{
    [SerializeField] private float manaAmount = 30f;
    [SerializeField] private float spinSpeed = 360f; 
    
    [Header("Life Time & Despawn")]
    [SerializeField] private float lifeTime = 10f; 
    [SerializeField] private float blinkTime = 3f; 
    
    [Header("Detonation Settings")]
    public float playerExplosionDamage = 1f;
    public float enemyExplosionDamage = 50f;
    public float explosionRadius = 2.5f;
    public float explosionKnockback = 3f;
    public GameObject explosionVFX;
    
    public List<EffectData> explosionEffects = new List<EffectData>();

    private bool isVacuuming = false;
    private bool isCollected = false;
    
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private float spawnTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        isVacuuming = false;
        isCollected = false;
        transform.localScale = Vector3.one;
        spawnTime = Time.time;
        
        Color c = sr.color;
        c.a = 1f;
        sr.color = c;
        
        if (rb != null)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            rb.AddForce(randomDir * 3f, ForceMode2D.Impulse);
        }
    }

    private void Update()
    {
        if (isCollected) return;

        transform.Rotate(0, 0, spinSpeed * Time.deltaTime);

        // Hút về Player
        if (PlayerController.Instance != null && !isVacuuming)
        {
            float distance = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
            if (distance <= PlayerController.Instance.playerData.manaCollectionRadius) 
            {
                isVacuuming = true;
                if (rb != null) rb.linearVelocity = Vector2.zero; 
            }
        }

        if (isVacuuming)
        {
            transform.position = Vector3.MoveTowards(transform.position, PlayerController.Instance.transform.position, 15f * Time.deltaTime);
            return; 
        }

        // Tự biến mất theo thời gian
        float timeAlive = Time.time - spawnTime;
        if (timeAlive >= lifeTime) ReturnToPool();
        else if (timeAlive >= lifeTime - blinkTime)
        {
            float alpha = Mathf.PingPong(Time.time * 10f, 0.7f) + 0.3f;
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((GameLayers.PlayerMask.value & (1 << collision.gameObject.layer)) > 0 && !isCollected)
        {
            isCollected = true;
            StartCoroutine(CollectRoutine());
        }
    }

    private IEnumerator CollectRoutine()
    {
        if (PlayerController.Instance.Mana != null)
            PlayerController.Instance.Mana.RestoreMana(manaAmount);
        
        float timer = 0, duration = 0.15f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            spinSpeed = 1500f;
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, timer / duration);
            yield return null;
        }
        ReturnToPool();
    }

    // ==========================================
    // HÀM KÍCH NỔ
    // ==========================================
    public void Detonate()
    {
        if (isCollected) return;
        isCollected = true;

        if (explosionVFX != null)
            ObjectPoolManager.Instance.GetFromPool(explosionVFX, transform.position, Quaternion.identity);

        // Quét bán kính vụ nổ
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, explosionRadius, GameLayers.PlayerAndDamageableMask | GameLayers.EnemyAndDamageableMask);
        
        foreach (Collider2D target in targets)
        {
            if (target.gameObject == gameObject) continue;
            int colLayer = target.gameObject.layer;

            // Xử lý sát thương phân biệt
            if (target.TryGetComponent(out IDamageable damageable))
            {
                bool isPlayer = (GameLayers.PlayerMask.value & (1 << colLayer)) > 0;
                float finalDamage = isPlayer ? playerExplosionDamage : enemyExplosionDamage;

                AttackDetails details = new AttackDetails
                {
                    position = transform.position,
                    damageAmount = finalDamage,
                    knockbackForce = explosionKnockback
                };
                damageable.TakeDamage(details);
            }

            // Đổ thêm Effect (nếu có set trong Editor)
            if (target.TryGetComponent(out EffectReceiver receiver) && explosionEffects.Count > 0)
            {
                foreach (var effect in explosionEffects) receiver.ApplyEffect(effect, transform.position);
            }
        }
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        Color c = sr.color;
        c.a = 1f;
        sr.color = c;
        ObjectPoolManager.Instance.AddToPool(gameObject);
    }
}