using UnityEngine;

[RequireComponent(typeof(SpellController))]
public class SpellAura : MonoBehaviour
{
    private SpellController controller;
    private float auraTimer;
    private float chargeDelayTimer;

    private void Awake()
    {
        controller = GetComponent<SpellController>();
    }

    public void Init()
    {
        if (controller == null) controller = GetComponent<SpellController>();
        
        // Đọc thời gian tick từ currentStats
        auraTimer = controller.currentStats.auraTickRate;
        
        // Nếu phép có thời gian gồng (castingTime) và dùng MultiStateAnimation thì chờ gồng xong mới bật Aura
        chargeDelayTimer = (controller.useMultiStateAnimation && controller.baseSpellData.castingTime > 0) 
            ? controller.currentStats.castingTime 
            : 0f;
    }

    private void Update()
    {
        if (controller == null || controller.currentStats == null) return;

        if (chargeDelayTimer > 0f)
        {
            chargeDelayTimer -= Time.deltaTime;
            return;
        }

        auraTimer += Time.deltaTime;
        
        // Đảm bảo TickRate không bao giờ bằng 0 (Gây treo máy)
        float tickRate = controller.currentStats.auraTickRate > 0f ? controller.currentStats.auraTickRate : 0.2f;

        if (auraTimer >= tickRate)
        {
            auraTimer = 0f;
            
            float currentRadius = Mathf.Max(0.1f, controller.currentStats.auraCheckRadius);
            Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, currentRadius, GameLayers.EnemyAndDamageableMask);

            foreach (Collider2D target in targets)
            {
                EffectReceiver receiver = target.GetComponentInParent<EffectReceiver>();
                
                // NẾU TÌM THẤY QUÁI VẬT (Có EffectReceiver)
                if (receiver != null)
                {
                    controller.ApplyEffectsToTarget(receiver.gameObject);
                }
                // ====================================================
                // ĐÃ THÊM: NẾU TÌM THẤY THÙNG GỖ (Không có EffectReceiver nhưng có IDamageable)
                // ====================================================
                else if (target.TryGetComponent(out IDamageable damageable))
                {
                    // Lọc tìm Sát thương (DoT) từ danh sách Effect của Aura
                    float totalDmg = 0f;
                    foreach(var eff in controller.currentStats.activeEffects)
                    {
                        if (eff.type == EffectType.DoT) totalDmg += eff.value;
                    }

                    if (totalDmg > 0)
                    {
                        // Gửi thẳng cục sát thương vào thùng gỗ
                        damageable.TakeDamage(new AttackDetails { 
                            damageAmount = totalDmg, 
                            position = transform.position, 
                            isDoT = false 
                        });
                    }
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (controller != null && controller.currentStats != null && controller.currentStats.auraCheckRadius > 0)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, controller.currentStats.auraCheckRadius);
        }
        else if (TryGetComponent(out SpellController ctrl) && ctrl.baseSpellData != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, ctrl.baseSpellData.auraCheckRadius);
        }
    }
}