using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PullableComponent : MonoBehaviour
{
    private Rigidbody2D rb;
    private CoreCCComponent ccComponent;

    // Các biến quản lý Hố đen
    public bool isBeingVacuumed { get; private set; }
    private Vector2 blackHoleCenter;
    private float vacuumSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ccComponent = GetComponent<CoreCCComponent>();
    }

    private void Update()
    {
        // XỬ LÝ HÚT LIÊN TỤC MỖI FRAME
        if (isBeingVacuumed)
        {
            // Tắt vận tốc vật lý thông thường để chống cự lại lực hút
            rb.linearVelocity = Vector2.zero;

            // Kéo dần mục tiêu về phía tâm hố đen (dùng MoveTowards để tránh bị giật lố tâm)
            if (Vector2.Distance(transform.position, blackHoleCenter) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, blackHoleCenter, vacuumSpeed * Time.deltaTime);
            }
        }
    }

    // ==========================================
    // CÁC HÀM GIAO TIẾP VỚI HỐ ĐEN (SPELL AURA)
    // ==========================================

    /// <summary>
    /// Gọi hàm này liên tục mỗi Tick (ví dụ: mỗi 0.1s) khi đứng trong vùng Aura của Hố đen
    /// </summary>
    public void ReceiveContinuousVacuum(Vector2 center, float speed)
    {
        blackHoleCenter = center;
        vacuumSpeed = speed;

        if (!isBeingVacuumed)
        {
            isBeingVacuumed = true;
            
            // ÉP CHUYỂN STATE (Nếu là Player)
            if (TryGetComponent(out PlayerController player))
            {
                player.HandleVacuumStart();
            }
            // ÉP CHUYỂN STATE (Nếu là Enemy)
            else if (TryGetComponent(out EnemyController enemy))
            {
                // enemy.HandleVacuumStart(); 
            }
        }

        // Đếm ngược để tự động nhả ra nếu Hố đen biến mất (hoặc nạn nhân đi ra khỏi vùng Aura)
        StopCoroutine(VacuumTimeoutRoutine());
        StartCoroutine(VacuumTimeoutRoutine());
    }

    // Nếu trong 0.2s mà không nhận được thêm tín hiệu hút nào -> Tự hiểu là Hố đen đã biến mất
    private System.Collections.IEnumerator VacuumTimeoutRoutine()
    {
        yield return new WaitForSeconds(0.2f);
        
        isBeingVacuumed = false;

        // BÁO CHO CONTROLLER BIẾT LÀ ĐƯỢC THẢ RA RỒI
        if (TryGetComponent(out PlayerController player)) player.HandleVacuumEnd();
        // else if (TryGetComponent(out EnemyController enemy)) enemy.HandleVacuumEnd();
    }
}