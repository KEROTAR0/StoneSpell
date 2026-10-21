using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class CoreCCComponent : MonoBehaviour
{
    [Header("CC States")]
    public bool isStunned { get; private set; }
    public bool isSleeping { get; private set; }
    
    // Tốc độ di chuyển có thể bị sửa đổi (ví dụ: Slow)
    public float moveSpeedMultiplier { get; set; } = 1f;

    [Header("Events")]
    [Tooltip("Phát tín hiệu khi bị khống chế để Controller ngắt hành động (ví dụ: hủy vận phép)")]
    public UnityEvent OnInterrupt; 

    /// <summary>
    /// Gây choáng hoặc ru ngủ mục tiêu
    /// </summary>
    /// <param name="duration">Thời gian hiệu lực</param>
    /// <param name="isSleep">Bật true nếu là trạng thái Sleep (ngủ, sẽ tỉnh khi bị đánh)</param>
    public void ApplyStun(float duration, bool isSleep = false)
    {
        if (!gameObject.activeInHierarchy) return;
        isSleeping = isSleep;
        isStunned = true;
        
        // Hét lên cho Controller biết để nó tự ép FSM vào HurtState hoặc StunState
        OnInterrupt?.Invoke(); 

        // Nếu đang bị choáng mà dính thêm chiêu choáng khác, reset lại thời gian
        StopAllCoroutines();
        StartCoroutine(StunRoutine(duration));
    }

    /// <summary>
    /// Hàm gọi khẩn cấp để đánh thức mục tiêu (Dùng khi bị nhận sát thương lúc đang Sleep)
    /// </summary>
    public void WakeUp()
    {
        if (isSleeping)
        {
            isSleeping = false;
            isStunned = false;
            StopAllCoroutines();
            
            // Xóa Buff hiển thị trên UI (Nếu có EffectReceiver)
            if (TryGetComponent(out EffectReceiver receiver))
            {
                receiver.RemoveEffect(EffectType.Sleep);
            }
            
            Debug.Log($"<color=orange>{gameObject.name} BỊ ĐÁNH THỨC!</color>");
        }
    }

    private IEnumerator StunRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        isStunned = false;
        isSleeping = false;
    }
}