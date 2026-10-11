using UnityEngine;

public class Dummy : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 500f;
    private float currentHealth;

    // Component để hiển thị hiệu ứng giật lùi (Knockback)
    private Rigidbody2D rb;

    private void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
    }

    public void TakeDamage(AttackDetails attackDetails)
    {
        currentHealth -= attackDetails.damageAmount;
        Debug.Log($"Ouch - HP LEFT: {currentHealth}");

        // Xử lý Knockback 
        if (rb != null)
        {
            Vector2 knockbackDirection = (transform.position - (Vector3)attackDetails.position).normalized;
            rb.AddForce(knockbackDirection * attackDetails.knockbackForce, ForceMode2D.Impulse);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    public void ApplyAilment(StatusAilment ailmentType, float duration, float value)
    {

    }
    private void Die()
    {
        // gọi từ hệ thống Pool
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SpawnMana(transform.position);
        }
        
        Destroy(gameObject);
    }
}