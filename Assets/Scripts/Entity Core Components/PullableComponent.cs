using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PullableComponent : MonoBehaviour
{
    public bool isBeingVacuumed { get; private set; }
    
    private Vector2 vacuumCenter;
    private float vacuumSpeed;
    private float vacuumTimer;
    private Rigidbody2D rb;
    private EnemyController enemyController;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyController = GetComponent<EnemyController>(); // Lấy sẵn để tối ưu
    }

    public void ReceiveContinuousVacuum(Vector2 center, float speed)
    {
        vacuumCenter = center;
        vacuumSpeed = speed;
        vacuumTimer = 0.2f; // Làm mới thời gian mỗi khi bị Aura quét trúng

        if (!isBeingVacuumed)
        {
            isBeingVacuumed = true;
            
            // Gửi tín hiệu đổi State
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
                isBeingVacuumed = false; // Hết giờ -> Tự động nhả ra
            }
            else
            {
                // Kéo rê mục tiêu về phía tâm
                Vector2 direction = (vacuumCenter - (Vector2)transform.position).normalized;
                rb.linearVelocity = direction * vacuumSpeed;
            }
        }
    }
}