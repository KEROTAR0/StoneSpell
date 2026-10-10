using UnityEngine;
using System.Collections.Generic;

public class Trap : MonoBehaviour
{
    [Header("Damage Settings")]
    public float playerDamageAmount = 1f;
    public float enemyDamageAmount = 10f;
    public float knockbackForce = 0.1f;
    public float damageCooldown = 1f; 

    [Header("Ailment Settings")]
    public StatusAilment ailmentType = StatusAilment.None;
    public float ailmentDuration = 2f;
    public float ailmentValue = 0f;

    // Bộ nhớ theo dõi thời gian hồi sát thương độc lập cho từng nạn nhân
    private Dictionary<GameObject, float> nextDamageTime = new Dictionary<GameObject, float>();

    private void OnTriggerStay2D(Collider2D collision)
    {
        GameObject target = collision.gameObject;

        // Khởi tạo bộ đếm nếu đây là mục tiêu mới dẫm vào bẫy
        if (!nextDamageTime.ContainsKey(target))
        {
            nextDamageTime[target] = 0f;
        }

        // Kiểm tra xem mục tiêu NÀY đã hết thời gian chờ chưa
        if (Time.time >= nextDamageTime[target])
        {
            IDamageable damageable = collision.GetComponent<IDamageable>();
            
            if (damageable != null)
            {
                // Phân biệt nạn nhân là Player hay Enemy
                float damage = target.CompareTag("Player") ? playerDamageAmount : enemyDamageAmount;

                AttackDetails attackDetails = new AttackDetails
                {
                    position = transform.position,
                    damageAmount = damage,
                    knockbackForce = knockbackForce
                };

                damageable.TakeDamage(attackDetails);
                
                // Áp dụng trạng thái bất lợi (như Slow, Poison...)
                damageable.ApplyAilment(ailmentType, ailmentDuration, ailmentValue);

                // Cập nhật mốc thời gian nhận sát thương tiếp theo cho RIÊNG mục tiêu này
                nextDamageTime[target] = Time.time + damageCooldown;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (nextDamageTime.ContainsKey(collision.gameObject))
        {
            nextDamageTime.Remove(collision.gameObject);
        }
    }
}