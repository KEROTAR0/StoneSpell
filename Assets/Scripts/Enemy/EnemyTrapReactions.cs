using UnityEngine;

public class EntityTrapReactions : MonoBehaviour
{
    private EnemyController entity;
    private CoreCCComponent cc; // Thêm biến CC

    private void Start()
    {
        entity = GetComponent<EnemyController>();
        cc = GetComponent<CoreCCComponent>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (entity == null || entity.Health.currentHealth <= 0) return; // Đọc máu từ HealthComponent

        if ((GameLayers.TrapLayerIndex & (1 << collision.gameObject.layer)) > 0)
        {
            // Kiểm tra xem quái có đang bị khống chế không thông qua CC Component
            if (cc != null && !cc.isStunned && !cc.isSleeping)
            {
                Vector2 escapeDir = (transform.position - collision.transform.position).normalized;
                
                if (entity.aiPath != null && !entity.aiPath.canMove)
                {
                    entity.aiPath.canMove = true;
                    entity.aiPath.isStopped = false;
                }
                
                entity.SetVelocity(escapeDir, entity.entityData.movementSpeed * 0.8f);
            }
        }
    }
}