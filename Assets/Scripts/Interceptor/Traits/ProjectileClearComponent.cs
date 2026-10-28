using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpellController))]
public class ProjectileClearComponent : MonoBehaviour
{
    private SpellController controller;
    private WaitForSeconds tickWait;

    private void Awake()
    {
        controller = GetComponent<SpellController>();
    }

    private void OnEnable()
    {
        // Nếu là Aura (Như khiên bao bọc), quét liên tục theo nhịp
        if (controller.baseSpellData.hasAura)
        {
            float tickRate = controller.currentStats.auraTickRate > 0f ? controller.currentStats.auraTickRate : 0.5f;
            tickWait = new WaitForSeconds(tickRate);
            StartCoroutine(ClearRoutine());
        }
        else
        {
            // Nếu là đạn bắn ra nổ 1 lần, chỉ quét 1 lần khi đạn vừa xuất hiện
            ClearEnemyProjectiles();
        }
    }

    private IEnumerator ClearRoutine()
    {
        while (true)
        {
            ClearEnemyProjectiles();
            yield return tickWait;
        }
    }

    private void ClearEnemyProjectiles()
    {
        // Bán kính quét có thể dùng chung với pushPullRadius hoặc explosionRadius tùy thiết kế
        float clearRadius = controller.currentStats.pushPullRadius > 0f ? controller.currentStats.pushPullRadius : controller.currentStats.explosionRadius;
        
        if (clearRadius <= 0f) return;

        // Quét tìm tất cả các vật thể nằm trong Layer đạn địch (Giả định layer là EnemyAttackMask)
        Collider2D[] enemyBullets = Physics2D.OverlapCircleAll(transform.position, clearRadius, GameLayers.EnemyAttackMask);
        
        foreach (Collider2D bulletCol in enemyBullets)
        {
            if (bulletCol.TryGetComponent(out EnemyProjectile enemyProj))
            {
                // Xóa xổ đạn địch
                enemyProj.Despawn();
                
                // Tùy chọn: Spawn một hiệu ứng nổ nhỏ tại vị trí viên đạn bị xóa
                // ObjectPoolManager.Instance.GetFromPool(miniExplosionPrefab, enemyProj.transform.position, Quaternion.identity);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (controller != null && controller.currentStats != null)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.5f); // Màu Vàng báo hiệu vùng xóa đạn
            float r = controller.currentStats.pushPullRadius > 0f ? controller.currentStats.pushPullRadius : controller.currentStats.explosionRadius;
            Gizmos.DrawWireSphere(transform.position, r);
        }
    }
}