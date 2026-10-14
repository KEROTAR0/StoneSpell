using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class KnockbackComponent : MonoBehaviour
{
    private Rigidbody2D rb;
    private CoreCCComponent ccComponent; 
    public UnityEvent<float> OnPullTriggered;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ccComponent = GetComponent<CoreCCComponent>();
    }

    // 1. XỬ LÝ ĐẨY LÙI VÀ ÉP TƯỜNG (KNOCKBACK)
    public void ApplyKnockback(Vector2 sourcePosition, float force)
    {
        if (rb == null) return;

        Vector2 knockbackDir = ((Vector2)transform.position - sourcePosition).normalized;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockbackDir * force, ForceMode2D.Impulse);

        StartCoroutine(CheckWallSlamRoutine(knockbackDir));
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
    public void ApplyPull(Vector2 center, float pullForce, float duration)
    {
        if (rb == null) return;

        // Tính hướng từ nhân vật TỚI tâm (ngược lại với knockback)
        Vector2 pullDir = (center - (Vector2)transform.position).normalized;
        
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(pullDir * pullForce, ForceMode2D.Impulse);

        // Báo tín hiệu kèm theo thời gian bị mất kiểm soát (khựng lại)
        OnPullTriggered?.Invoke(duration);
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