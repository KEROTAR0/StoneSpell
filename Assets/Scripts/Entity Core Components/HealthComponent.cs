using UnityEngine;
using UnityEngine.Events;

public class HealthComponent : MonoBehaviour, IDamageable
{
    // Dữ liệu gốc lấy từ ScriptableObject hoặc khởi tạo mặc định
    [SerializeField] private float initialMaxHealth = 100f;
    
    // Dữ liệu Runtime (Có thể bị thay đổi bởi đồ đạc/buff)
    public int maxHealth { get; private set; }
    public int currentHealth { get; private set; }
    public float damageTakenMultiplier = 1f;

    [Header("Events")]
    public UnityEvent OnTakeDamage;
    public UnityEvent OnDeath;

    public void Init(float templateMaxHealth)
    {
        // Nhận maxHealth từ D_Player hoặc D_Entity lúc Start()
        this.maxHealth = Mathf.RoundToInt(templateMaxHealth);
        this.currentHealth = this.maxHealth;
        UpdateUI();
    }

    public void RestoreHealth(int amount)
    {
        if (currentHealth <= 0) return;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UpdateUI();
    }

    public void TakeDamage(AttackDetails attackDetails)
    {
        if (currentHealth <= 0) return; // Đã chết thì thôi

        // TÍNH TOÁN (LUÔN LÀM TRÒN THÀNH SỐ NGUYÊN)
        float rawDamage = attackDetails.damageAmount * damageTakenMultiplier;
        int finalDamage = Mathf.RoundToInt(rawDamage);
        
        currentHealth -= finalDamage;
        UpdateUI();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnDeath?.Invoke(); // Hét lên cho Controller biết "Tôi chết rồi!"
        }
        else
        {
            OnTakeDamage?.Invoke(); // Báo hiệu bị thương (để Controller giật khựng hoặc nháy đỏ)
        }
    }

    private void UpdateUI()
    {   
        if ((GameLayers.PlayerMask.value & (1 << gameObject.layer)) > 0)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateHealthUI(currentHealth, maxHealth);
            }
        }
    }
}