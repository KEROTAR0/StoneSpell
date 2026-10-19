using UnityEngine;

[RequireComponent(typeof(SpellController))]
public class SpellAura : MonoBehaviour
{
    private SpellController controller;
    private float auraTimer;
    private float chargeDelayTimer;

    private void Awake() => controller = GetComponent<SpellController>();

    public void Init() 
    {
        auraTimer = controller.currentStats.auraTickRate;

        // KIỂM TRA ĐỘ TRỄ GỒNG CHIÊU
        if (controller.useMultiStateAnimation && controller.baseSpellData.castingTime > 0)
        {
            chargeDelayTimer = controller.baseSpellData.castingTime;
        }
        else
        {
            chargeDelayTimer = 0f;
        }
    }

    private void Update()
    {
        // 1. NẾU ĐANG CHARGE (GỒNG) -> CHỜ ĐẾM NGƯỢC XONG MỚI CHO CHẠY TIẾP
        if (chargeDelayTimer > 0)
        {
            chargeDelayTimer -= Time.deltaTime;
            return; 
        }

        // 2. LOGIC QUÉT SÁT THƯƠNG BÌNH THƯỜNG (Bắt đầu chạy khi vào Animation Fire)
        if (controller.currentStats.auraTickRate <= 0) return; 
        
        auraTimer += Time.deltaTime;
        if (auraTimer >= controller.currentStats.auraTickRate)
        {
            auraTimer = 0;
            Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, controller.baseSpellData.auraCheckRadius, GameLayers.EnemyAndDamageableMask);
            foreach (var target in targets) 
            {
                if(target != null)
                {
                    controller.ApplyEffectsToTarget(target.gameObject);
                    Vector2 hitPoint = target.ClosestPoint(transform.position);
                    controller.SpawnImpactVFXAtPosition(hitPoint);
                }
            }
        }
    }
}