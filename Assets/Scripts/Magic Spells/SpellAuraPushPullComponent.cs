using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpellController))]
public class SpellAuraPushPullComponent : MonoBehaviour
{
    private SpellController controller;
    private WaitForSeconds tickWait;

    private void Awake()
    {
        controller = GetComponent<SpellController>();
    }

    private void OnEnable()
    {
        // KHÔNG đọc currentStats ở đây nữa để tránh NullReferenceException
        // Chỉ việc gọi Coroutine chạy
        StartCoroutine(AuraRoutine());
    }

    private IEnumerator AuraRoutine()
    {
        // =============================================================
        // ĐÃ SỬA: Chờ đến khi SpellController khởi tạo xong currentStats 
        // (thông qua hàm Initialize) thì mới bắt đầu chạy logic
        // =============================================================
        yield return new WaitUntil(() => controller != null && controller.currentStats != null);

        // Lúc này currentStats đã có dữ liệu, đọc an toàn 100%
        float tickRate = controller.currentStats.auraTickRate > 0f ? controller.currentStats.auraTickRate : 0.5f;
        tickWait = new WaitForSeconds(tickRate);
        
        while (true)
        {
            float radius = controller.currentStats.pushPullRadius;
            float pullF = controller.currentStats.pullForce;
            float pushF = controller.currentStats.knockbackForce;

            if (radius > 0f && (pullF > 0f || pushF > 0f))
            {
                Collider2D[] hitTargets = Physics2D.OverlapCircleAll(transform.position, radius, GameLayers.EnemyAndDamageableMask);
                
                foreach (Collider2D hit in hitTargets)
                {
                    EffectReceiver receiver = hit.GetComponentInParent<EffectReceiver>();
                    if (receiver == null) continue;

                    // A. KÍCH HOẠT LỰC ĐẨY LÙI (KNOCKBACK)
                    if (pushF > 0f && receiver.TryGetComponent(out KnockbackComponent kbPush))
                    {
                        kbPush.ApplyKnockback(transform.position, pushF);
                    }
                    
                    // B. KÍCH HOẠT LỰC HÚT (PULL / VACUUM)
                    if (pullF > 0f && receiver.TryGetComponent(out PullableComponent pullable))
                    {
                        pullable.ReceiveContinuousVacuum(transform.position, pullF, tickRate); 
                    }
                }
            }
            
            yield return tickWait;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (controller != null && controller.currentStats != null)
        {
            Gizmos.color = new Color(1f, 1f, 1f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, controller.currentStats.pushPullRadius);
        }
    }
}