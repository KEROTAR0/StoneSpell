using UnityEngine;
using System.Collections.Generic;

public class Trap : MonoBehaviour
{
    [Header("Life Time Settings")]
    [Tooltip("Tick nếu đây là bẫy tạm thời (như vũng lầy, vũng độc sinh ra khi quái chết). Bỏ tick nếu bẫy gắn sẵn trong Map.")]
    public bool hasLifeTime = false;
    public float lifeTime = 5f;
    private float spawnTime;

    [Header("Damage Settings")]
    public float playerDamageAmount = 1f;
    public float enemyDamageAmount = 10f;
    [Tooltip("Khoảng cách giữa các lần gây sát thương/hiệu ứng lên CÙNG 1 mục tiêu")]
    public float damageCooldown = 1f; 
    
    // (Lực đẩy vật lý tức thì - Có thể dùng nếu bẫy là bẫy lò xo. Nếu là vũng lầy thì set = 0)
    public float knockbackForce = 0.1f; 

    [Header("Trap Effects")]
    public List<EffectData> trapEffects = new List<EffectData>();

    // Bộ nhớ đệm để theo dõi thời điểm nạn nhân dẫm vào bẫy
    private Dictionary<GameObject, float> nextDamageTime = new Dictionary<GameObject, float>();

    // Sử dụng OnEnable thay vì Start để tương thích với Object Pool (Khi bẫy được sinh ra từ quái)
    private void OnEnable()
    {
        spawnTime = Time.time;
        nextDamageTime.Clear(); // Dọn dẹp danh sách nạn nhân cũ nếu bẫy được tái sử dụng
    }

    private void Update()
    {
        // Tự động hủy bẫy nếu nó là bẫy tạm thời và đã hết thời gian
        if (hasLifeTime && Time.time >= spawnTime + lifeTime)
        {
            Despawn();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        GameObject target = collision.gameObject;
        int colLayer = target.layer;

        // 1. SỬ DỤNG GAMELAYERS ĐỂ LỌC ĐỐI TƯỢNG (Chỉ tác dụng lên Player, Enemy hoặc Damageable)
        bool isPlayer = (GameLayers.PlayerMask.value & (1 << colLayer)) > 0;
        bool isEnemy = (GameLayers.EnemyMask.value & (1 << colLayer)) > 0;
        bool isDamageable = (GameLayers.DamageableMask.value & (1 << colLayer)) > 0;

        if (!isPlayer && !isEnemy && !isDamageable) return;

        // 2. LOGIC TÍNH TOÁN COOLDOWN SÁT THƯƠNG
        if (!nextDamageTime.ContainsKey(target))
        {
            nextDamageTime[target] = 0f;
        }

        if (Time.time >= nextDamageTime[target])
        {
            // A. GÂY SÁT THƯƠNG CƠ BẢN
            if (target.TryGetComponent(out IDamageable damageable))
            {
                // Ưu tiên: Nếu là Player thì dùng sát thương Player, ngược lại dùng sát thương Enemy
                float damage = isPlayer ? playerDamageAmount : enemyDamageAmount;

                AttackDetails attackDetails = new AttackDetails
                {
                    position = transform.position,
                    damageAmount = damage,
                    knockbackForce = knockbackForce 
                };

                damageable.TakeDamage(attackDetails);
            }
            
            // B. ĐỔ EFFECT (SLOW, POISON...) TỪ BẪY LÊN MỤC TIÊU
            if (target.TryGetComponent(out EffectReceiver receiver))
            {
                if (trapEffects != null && trapEffects.Count > 0)
                {
                    foreach (var effect in trapEffects)
                    {
                        receiver.ApplyEffect(effect, transform.position);
                    }
                }
            }
            
            // Cập nhật mốc thời gian giật sát thương tiếp theo cho nạn nhân này
            nextDamageTime[target] = Time.time + damageCooldown;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (nextDamageTime.ContainsKey(collision.gameObject))
        {
            nextDamageTime.Remove(collision.gameObject);
        }
    }

    private void Despawn()
    {
        // Trả bẫy về Object Pool thay vì Destroy để tối ưu hiệu năng
        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.AddToPool(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}