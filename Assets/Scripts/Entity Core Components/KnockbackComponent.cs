using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using Pathfinding;

[RequireComponent(typeof(Rigidbody2D))]
public class KnockbackComponent : MonoBehaviour
{
    private Rigidbody2D rb;
    private AIPath aiPath;
    private CoreCCComponent ccComponent; 
    public UnityEvent<float> OnPullTriggered;
    private bool isKnockedBack = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ccComponent = GetComponent<CoreCCComponent>();
        aiPath = GetComponent<AIPath>();
    }

    // 1. XỬ LÝ ĐẨY LÙI VÀ ÉP TƯỜNG (KNOCKBACK)
    public void ApplyKnockback(Vector2 sourcePosition, float force, float duration = 0.2f)
    {
        if (!gameObject.activeInHierarchy) return;
        Vector2 direction = ((Vector2)transform.position - sourcePosition).normalized;
        
        // Dừng các hiệu ứng đẩy/hút cũ để không bị xung đột
        StopAllCoroutines(); 
        
        StartCoroutine(KnockbackRoutine(direction, force, duration));
    }
    private IEnumerator KnockbackRoutine(Vector2 direction, float force, float duration)
    {
        // 1. NGẮT QUYỀN ĐIỀU KHIỂN CỦA AIPATH
        isKnockedBack = true;
        if (aiPath != null) aiPath.canMove = false;

        // 2. ÁP DỤNG LỰC ĐẨY
        rb.linearVelocity = Vector2.zero;
        Vector2 appliedForce = direction * force;
        rb.AddForce(appliedForce, ForceMode2D.Impulse);

        // 3. VÒNG LẶP KIỂM TRA WALL SLAM
        float timer = 0f;
        bool hasSlammed = false;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            // Bắn tia quét xem có chạm tường không. 
            // Quét theo hướng đang bay, dài khoảng 0.5f (tùy thuộc vào độ lớn của quái)
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 0.5f, GameLayers.WallAndHighObstacleMask);
            
            if (hit.collider != null)
            {
                hasSlammed = true;
                break; // Thoát vòng lặp ngay khi chạm tường
            }
            
            yield return null;
        }

        // 4. NẾU BỊ ÉP TƯỜNG -> GÂY CHOÁNG (STUN)
        if (hasSlammed)
        {
            // Ép dừng ngay lập tức để không bị trượt dọc theo tường
            rb.linearVelocity = Vector2.zero; 
            
            if (ccComponent != null)
            {
                // Choáng 1.5 giây khi đập tường
                ccComponent.ApplyStun(1.5f, false); 
                Debug.Log("Wall Slammed!");
            }
            
            // Tùy chọn: Spawn VFX đập tường ở đây (hit.point)
        }

        // 5. TRẢ LẠI QUYỀN ĐIỀU KHIỂN (NẾU KHÔNG BỊ CHOÁNG)
        isKnockedBack = false;
        
        // Luôn xóa vận tốc thừa khi kết thúc đẩy
        rb.linearVelocity = Vector2.zero; 

        if (aiPath != null && ccComponent != null && !ccComponent.isStunned)
        {
            aiPath.canMove = true;
        }
    }
    private IEnumerator CheckWallSlamRoutine(Vector2 dir)
    {
        float timer = 0.2f; 
        bool hasSlammed = false;

        while (timer > 0)
        {
            timer -= Time.deltaTime;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, 0.5f, GameLayers.WallAndHighObstacleMask);
            if (hit.collider != null)
            {
                hasSlammed = true;
                break;
            }
            yield return null;
        }

        if (hasSlammed && ccComponent != null)
        {
            ccComponent.ApplyStun(1.5f, false); // Ép tường gây choáng
        }
    }

    // 2. XỬ LÝ LỰC HÚT
    // public void ApplyPull(Vector2 center, float speed, float duration)
    // {
    //     StopCoroutine("PullRoutine");
    //     StartCoroutine(PullRoutine(center, speed, duration));
    // }
    // --- CƠ CHẾ HÚT 1 LẦN (IMPULSE PULL) ---
    public void ApplyPull(Vector2 targetPosition, float force, float duration = 0.2f)
    {
        if (!gameObject.activeInHierarchy) return;
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        StartCoroutine(KnockbackRoutine(direction * force, force, duration));
    }

    private IEnumerator PullRoutine(Vector2 center, float speed, float duration)
    {
        float timer = 0.2f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            // Kéo mục tiêu về phía tâm Hố đen
            if (Vector2.Distance(transform.position, center) > 0.1f)
            {
                transform.position = Vector2.MoveTowards(transform.position, center, speed * Time.deltaTime);
            }
            yield return null;
        }
    }
}