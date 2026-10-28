using UnityEngine;
using Pathfinding; // Thêm thư viện A* để kiểm soát AI

[RequireComponent(typeof(Rigidbody2D))]
public class PullableComponent : MonoBehaviour
{
    public bool isBeingVacuumed { get; private set; }
    
    private Vector2 vacuumCenter;
    private float vacuumSpeed;
    private float vacuumTimer;
    private Rigidbody2D rb;
    private EnemyController enemyController;
    private AIPath aiPath; // Thêm biến để trói chân AI

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyController = GetComponent<EnemyController>(); 
        aiPath = GetComponent<AIPath>(); // Lấy AIPath
    }

    // ĐÃ SỬA: Yêu cầu truyền thêm duration (thường là bằng với auraTickRate)
    public void ReceiveContinuousVacuum(Vector2 center, float speed, float duration)
    {
        vacuumCenter = center;
        vacuumSpeed = speed;
        // Trừ hao thêm 0.05s để lực hút mượt mà, không bị ngắt quãng giữa 2 lần quét của Aura
        vacuumTimer = duration + 0.05f; 

        if (!isBeingVacuumed)
        {
            isBeingVacuumed = true;
            
            // 1. NGẮT QUYỀN ĐIỀU KHIỂN CỦA AI (Giống hệt Knockback)
            if (aiPath != null) aiPath.canMove = false;
            
            // 2. Gửi tín hiệu đổi State
            if (TryGetComponent(out PlayerController player))
            {
                player.HandleVacuumStart();
            }
            else if (enemyController != null)
            {
                enemyController.HandleInterrupt();
            }
        }
    }

    private void FixedUpdate()
    {
        if (isBeingVacuumed)
        {
            vacuumTimer -= Time.fixedDeltaTime;
            
            if (vacuumTimer <= 0)
            {
                isBeingVacuumed = false; 
                
                // HẾT GIỜ HÚT -> TRẢ LẠI QUYỀN DI CHUYỂN CHO AI
                if (aiPath != null && TryGetComponent(out CoreCCComponent cc) && !cc.isStunned)
                {
                    aiPath.canMove = true;
                }
            }
            else
            {
                // Kéo rê mục tiêu thẳng về phía tâm Hố đen
                Vector2 direction = (vacuumCenter - (Vector2)transform.position).normalized;
                rb.linearVelocity = direction * vacuumSpeed;
            }
        }
    }
}