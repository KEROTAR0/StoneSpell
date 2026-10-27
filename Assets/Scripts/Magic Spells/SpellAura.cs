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
        if (auraTimer >= controller.currentStats.auraTickRate)
        {
            auraTimer = 0f;
            
            // ĐÃ SỬA: Dùng currentStats.auraCheckRadius thay vì baseSpellData.auraCheckRadius
            float currentRadius = Mathf.Max(0.1f, controller.currentStats.auraCheckRadius);
            Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, currentRadius, GameLayers.EnemyAndDamageableMask);

            foreach (Collider2D target in targets)
            {
                EffectReceiver receiver = target.GetComponentInParent<EffectReceiver>();
                GameObject rootObj = receiver != null ? receiver.gameObject : target.gameObject;
                controller.ApplyEffectsToTarget(rootObj);
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