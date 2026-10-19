using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SpellController : MonoBehaviour
{
    // Dữ liệu TĨNH (Khuôn đúc)
    public D_Spell baseSpellData; 
    public Transform visualHolder;

    [Header("Animation Control")]
    [Tooltip("Tick nếu dùng 3 state Charge -> Fire -> Fade cho Toggle. Bỏ tick nếu gộp chung 1 Animation.")]
    public bool useMultiStateAnimation = false; 
    private Animator anim;
    private bool isFadingOut = false;

    // Dữ liệu ĐỘNG (Bản sao dùng cho viên đạn này)
    public RuntimeSpellStats currentStats { get; private set; } 
    public Vector2 initialDirection { get; private set; }
    private float timeActivated;
    private float auraTimer = 0f; 
    private float orbitAngle = 0f;
    private bool isOrbitingOrb = false;

    public void Initialize(Vector2 direction, int index = 0, int totalCount = 1)
    {
        this.initialDirection = direction;
        // 1. TẠO BẢN SAO DỮ LIỆU ĐỘNG
        currentStats = new RuntimeSpellStats(baseSpellData);
    
        // 2. TRẠM KIỂM DUYỆT (INTERCEPTOR)
        if (RunManager.Instance != null)
        {
            RunManager.Instance.InterceptSpell(this);
        }
        anim = GetComponent<Animator>();
        isFadingOut = false;
        timeActivated = Time.time;
        auraTimer = currentStats.auraTickRate; 
        Vector2 finalDirection = direction;
        if (currentStats.inaccuracy > 0)
        {
            float randomAngle = Random.Range(-currentStats.inaccuracy, currentStats.inaccuracy);
            finalDirection = Quaternion.Euler(0, 0, randomAngle) * direction;
        }

        // 3. KHỞI TẠO LOGIC
        if (baseSpellData.isSelfTargeted && PlayerController.Instance != null)
        {
            // NẾU LÀ ORBITING ORB (Đạn bay quanh người)
            if (baseSpellData.isProjectile) 
            {
                isOrbitingOrb = true;
                orbitAngle = (360f / totalCount) * index; // Chia đều góc cho các viên
                
                // Cập nhật vị trí ban đầu ngay lập tức
                UpdateOrbitPosition();
                
                // Đạn bay quanh người thì không có va chạm vật lý nảy/chặn tường
                if (TryGetComponent(out Rigidbody2D rb))
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.bodyType = RigidbodyType2D.Kinematic;
                }
            }
            // NẾU LÀ BÙA LỢI BÁM VÀO NGƯỜI (Aura/Khiên tự thân)
            else
            {
                transform.SetParent(PlayerController.Instance.transform);
                transform.localPosition = Vector3.zero;
                
                if (TryGetComponent(out Rigidbody2D rb))
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.bodyType = RigidbodyType2D.Kinematic;
                }
            }
            
            // CHỈ cho phép Va chạm khi đạn đang xoay, HOẶC Aura.
            if (TryGetComponent(out SpellCollision collision)) collision.Init();
            if (baseSpellData.hasAura && TryGetComponent(out SpellAura aura)) aura.Init();
        }
        else
        {
            // ĐẠN BẮN RA HOẶC ĐẶT MÌN TRÊN MẶT ĐẤT
            // Nếu speed = 0, nó sẽ tự động trở thành bãi mìn tại tọa độ con trỏ chuột
            if (TryGetComponent(out SpellMovement movement)) movement.Init(finalDirection);
            if (TryGetComponent(out SpellCollision collision)) collision.Init();
            if (TryGetComponent(out SpellLaser laser)) laser.Init(direction);
            if (baseSpellData.hasAura && TryGetComponent(out SpellAura aura)) aura.Init();
        }
    }

    private void Update()
    {
        if (baseSpellData.isSelfTargeted && baseSpellData.isRotating)
        {
            Transform targetRotate = visualHolder != null ? visualHolder : transform;
            targetRotate.Rotate(Vector3.forward, currentStats.rotationSpeed * Time.deltaTime);
        }
        if (isOrbitingOrb && PlayerController.Instance != null)
        {
            orbitAngle += baseSpellData.orbRotationSpeed * Time.deltaTime;
            UpdateOrbitPosition();
        }
        if (baseSpellData.isSelfTargeted && PlayerController.Instance != null && currentStats.auraTickRate > 0)
        {
            auraTimer += Time.deltaTime;
            if (auraTimer >= currentStats.auraTickRate)
            {
                auraTimer = 0f;
                ApplyEffectsToTarget(PlayerController.Instance.gameObject, true);
            }
        }

        // ĐÃ XÓA MỘT ĐOẠN IF BỊ TRÙNG LẶP GÂY LỖI TỰ SÁT Ở ĐÂY!
        
        // CHỈ KIỂM TRA LIFETIME NẾU KHÔNG PHẢI LÀ PHÉP TOGGLE
        if (!baseSpellData.isManualToggle && Time.time >= timeActivated + currentStats.lifeTime)
        {
            if (!isFadingOut) Despawn(); 
        }
    }

    public void ApplyEffectsToTarget(GameObject target, bool isSelfCast = false)
    {
        bool isTargetingPlayer = target.TryGetComponent(out PlayerController playerTarget);
        if (isTargetingPlayer && !isSelfCast) return; 
        
        bool isEnemyAttack = (GameLayers.EnemyAttackMask.value & (1 << gameObject.layer)) > 0;

        if (isTargetingPlayer)
        {
            if (isEnemyAttack) 
            {
                // Để trống: Cho phép luồng chạy tiếp xuống dưới để nhận sát thương
            }
            else if (!isSelfCast) 
            {
                return; 
            }
        }

        if (target.TryGetComponent(out EffectReceiver receiver))
        {
            foreach (var effect in currentStats.activeEffects)
            {
                if (isTargetingPlayer)
                {
                    // Từ chối các loại Buff tốt (Trừ ManaRestore âm/dương tùy hệ thống của bạn)
                    if (!effect.isBuff && effect.type != EffectType.ManaRestore) continue; 
                }
                // Nếu đánh vào Quái:
                else 
                {
                    // Bỏ qua các hiệu ứng Buff tốt để không lỡ tay hồi máu cho địch
                    if (effect.isBuff) continue;
                }

                receiver.ApplyEffect(effect, transform.position);
            }
        }
        
        // CHỈ SÁT THƯƠNG FLAT CHO QUÁI (Không bao giờ sát thương flat lên Player)
        if (!isTargetingPlayer && target.TryGetComponent(out IDamageable damageable))
        {
            float totalFlatDamage = 0f;

            foreach (var effect in currentStats.activeEffects)
            {
                if (effect.type == EffectType.DoT && effect.tickRate <= 0)
                {
                    totalFlatDamage += effect.value;
                }
            }

            if (totalFlatDamage > 0)
            {
                AttackDetails details = new AttackDetails 
                { 
                    damageAmount = totalFlatDamage, 
                    position = transform.position,
                };
                damageable.TakeDamage(details);
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
    private void TriggerExplosion()
    {
        // 1. SPAWN VFX ĐỒNG BỘ: Sinh ra ngay tại tâm viên đạn trước khi nó biến mất
        if (baseSpellData.explosionVFX != null)
        {
            ObjectPoolManager.Instance.GetFromPool(baseSpellData.explosionVFX, transform.position, Quaternion.identity);
        }

        // 2. GÂY SÁT THƯƠNG AOE
        // Quét tất cả Quái và Thùng gỗ trong bán kính
        Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, baseSpellData.explosionRadius, GameLayers.EnemyAndDamageableMask);
        
        foreach (Collider2D target in targets)
        {
            // Dùng lại hàm ApplyEffectsToTarget thần thánh để truyền Độc, Cháy, Choáng, Sát thương... vào vùng nổ
            ApplyEffectsToTarget(target.gameObject);
        }
    }
    private void UpdateOrbitPosition()
    {
        float rad = orbitAngle * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * baseSpellData.orbitRadius;
        transform.position = (Vector2)PlayerController.Instance.transform.position + offset;
    }
    public void Despawn()
    {
        // KÍCH NỔ (Nếu có)
        if (baseSpellData.explodeOnDespawn && !isFadingOut)
        {
            TriggerExplosion();
        }

        // NẾU DÙNG MULTI-STATE ANIMATION (Bao gồm cả Toggle) VÀ CHƯA CHẠY FADE
        if (useMultiStateAnimation && anim != null && !isFadingOut)
        {
            isFadingOut = true;
            
            // Gửi lệnh kích hoạt Animation FadeOut
            anim.SetTrigger("FadeOut");
            
            // Lấy thời gian của clip Spell_Fade
            float fadeDuration = GetAnimationClipLength(anim, "Spell_Fade");
            
            // Nếu không tìm thấy clip, mặc định chờ 0.5s
            if (fadeDuration <= 0) fadeDuration = 0.5f; 
            
            // Trì hoãn việc thu hồi (cất vào kho) cho đến khi Fade xong
            Invoke(nameof(FinalizeDespawn), fadeDuration);
            return; 
        }

        // Nếu không có Animation nhiều bước, thu hồi ngay lập tức
        FinalizeDespawn();
    }
    private void FinalizeDespawn()
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
    
    // HÀM TIỆN ÍCH: Lấy độ dài của một Animation Clip
    private float GetAnimationClipLength(Animator animator, string clipName)
    {
        if (animator == null || animator.runtimeAnimatorController == null) return 0f;
        
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == clipName) return clip.length;
        }
        return 0f;
    }
}