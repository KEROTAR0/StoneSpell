using UnityEngine;

public class SpellController : MonoBehaviour
{
    // Dữ liệu TĨNH (Khuôn đúc)
    public D_Spell baseSpellData; 
    public Transform visualHolder;

    // Dữ liệu ĐỘNG (Bản sao dùng cho viên đạn này)
    public RuntimeSpellStats currentStats { get; private set; } 
    private float timeActivated;
    private float auraTimer = 0f; 

    // SỬA HÀM INIT: Xóa toàn bộ các tham số LayerMask
    public void Initialize(Vector2 direction)
    {
        // 1. TẠO BẢN SAO DỮ LIỆU ĐỘNG
        currentStats = new RuntimeSpellStats(baseSpellData);

        // 2. TRẠM KIỂM DUYỆT (INTERCEPTOR)
        if (RunManager.Instance != null)
        {
            RunManager.Instance.InterceptSpell(this);
        }

        timeActivated = Time.time;
        auraTimer = currentStats.auraTickRate; 

        // 3. KHỞI TẠO LOGIC
        if (baseSpellData.isSelfTargeted && PlayerController.Instance != null)
        {
            transform.SetParent(PlayerController.Instance.transform);
            transform.localPosition = Vector3.zero;
            
            // XÓA LỆNH TẮT COLLIDER VÀ RIGIDBODY, CHỈ DỪNG VẬN TỐC
            if (TryGetComponent(out Rigidbody2D rb))
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
            
            // BẮT BUỘC PHẢI KHỞI TẠO COLLISION ĐỂ NÓ BẮT ĐẠN ĐỊCH
            if (TryGetComponent(out SpellCollision collision)) collision.Init();
            if (TryGetComponent(out SpellAura aura)) aura.Init();
        }
        else
        {
            if (TryGetComponent(out SpellMovement movement)) movement.Init(direction);
            if (TryGetComponent(out SpellCollision collision)) collision.Init();
            if (TryGetComponent(out SpellLaser laser)) laser.Init(direction);
            if (TryGetComponent(out SpellAura aura)) aura.Init();
        }
    }

    private void Update()
    {
        if (baseSpellData.isSelfTargeted && PlayerController.Instance != null && currentStats.auraTickRate > 0)
        {
            auraTimer += Time.deltaTime;
            if (auraTimer >= currentStats.auraTickRate)
            {
                auraTimer = 0f;
                ApplyEffectsToTarget(PlayerController.Instance.gameObject, true);
            }
        }

        if (Time.time >= timeActivated + currentStats.lifeTime)
        {
            Despawn();
        }
    }

    public void ApplyEffectsToTarget(GameObject target, bool isSelfCast = false)
    {
        if (target.TryGetComponent(out EffectReceiver receiver))
        {
            foreach (var effect in currentStats.activeEffects)
            {
                if (isSelfCast && !effect.isBuff) continue;

                // 2. Đánh trúng mục tiêu khác: CHỈ gây Sát thương/Khống chế, KHÔNG lỡ tay buff máu cho quái vật
                if (!isSelfCast && effect.isBuff) continue;

                receiver.ApplyEffect(effect, transform.position);
            }
        }
    }

    public void SpawnImpactVFXAtPosition(Vector2 exactPosition)
    {
        foreach (var effect in currentStats.activeEffects)
        {
            if (effect.tickRate <= 0 && effect.vfxPrefab != null)
            {
                ObjectPoolManager.Instance.GetFromPool(effect.vfxPrefab, exactPosition, Quaternion.identity);
            }
        }
    }
    public void Despawn()
    {
        if (baseSpellData.isSelfTargeted && PlayerController.Instance != null)
        {
            if (PlayerController.Instance.TryGetComponent(out EffectReceiver receiver))
            {
                foreach (var effect in currentStats.activeEffects)
                    receiver.RemoveEffect(effect.type);
            }
        }

        transform.SetParent(null);
        if (TryGetComponent(out Rigidbody2D rb)) 
        {
            rb.simulated = true;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        if (ObjectPoolManager.Instance != null)
            ObjectPoolManager.Instance.AddToPool(gameObject);
        else
            Destroy(gameObject);
    }
}