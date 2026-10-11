using UnityEngine;

public class EntityTrapReactions : MonoBehaviour
{
    private Entity entity;

    private void Start()
    {
        entity = GetComponent<Entity>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (entity == null || entity.isDead) return;

        // Nếu chạm vào bẫy (Kiểm tra layer)
        if ((GameLayers.TrapLayerIndex & (1 << collision.gameObject.layer)) > 0)
        {
            // Nếu quái đang có quyền di chuyển (không bị Stun, Suction...)
            if (entity.stateMachine.currentState != ((EntitySlime)entity).stunnedState &&
                entity.stateMachine.currentState != ((EntitySlime)entity).suctionState)
            {
                // Ép quái vật phải di chuyển ra khỏi vùng tâm bẫy
                Vector2 escapeDir = (transform.position - collision.transform.position).normalized;
                
                // Mở khóa AIPath nếu nó đang bị tắt
                if (entity.aiPath != null && !entity.aiPath.canMove)
                {
                    entity.aiPath.canMove = true;
                    entity.aiPath.isStopped = false;
                }
                
                // Cấp một vận tốc nhỏ để trượt ra ngoài
                entity.SetVelocity(escapeDir, entity.entityData.moveSpeed * 0.8f);
            }
        }
    }
}