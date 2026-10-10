using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class SpellBase : MonoBehaviour
{
    public D_Spell spellData;
    public GameObject prefabOriginal; // Tham chiếu về prefab gốc để SpellPool nhận diện

    [Header("Visuals")]
    [Tooltip("Kéo object con chứa SpriteRenderer và Animator vào đây")]
    public Transform visualHolder;

    // Ma trận Layer
    private LayerMask targetLayer;
    private LayerMask wallLayer;
    private LayerMask highObstacleLayer;
    private LayerMask EnemyProjectileLayer;

    private int currentBounces;
    private int currentBlocks;
    private float auraTimer;
    private float timeActivated;
    
    private Rigidbody2D rb;
    private Vector2 currentDirection;
    private Animator animChild;
    private List<SpellEffect> activeEffects = new List<SpellEffect>();

    private bool isHitboxActive = false;
    private bool hasFinishedAttackFrame = false; 
    private float currentSpeed = 0f;
    private HashSet<GameObject> hitTargets = new HashSet<GameObject>(); // Bộ lọc chống sát thương lặp

    public void Initialize(GameObject originalPrefab, LayerMask target, LayerMask wall, LayerMask highObs, LayerMask enemyProjectile, Vector2 direction)
    {
        if (spellData == null)
        {
            Debug.LogError($"Prefab {gameObject.name} chưa được gắn D_Spell!");
            return;
        }

        this.prefabOriginal = originalPrefab;
        this.targetLayer = target;
        this.wallLayer = wall;
        this.highObstacleLayer = highObs;
        this.EnemyProjectileLayer = enemyProjectile;

        rb = GetComponent<Rigidbody2D>();
        currentBounces = this.spellData.maxBounces;
        currentBlocks = this.spellData.maxBlocks;
        currentDirection = direction.normalized;
        animChild = GetComponentInChildren<Animator>();

        timeActivated = Time.time;
        auraTimer = this.spellData.auraTickRate;

        isHitboxActive = false;
        hasFinishedAttackFrame = false;
        hitTargets.Clear();
        currentSpeed = spellData.isPenetrating ? 0f : spellData.moveSpeed; 

        activeEffects.Clear();
        if (spellData.hitEffects != null)
        {
            activeEffects.AddRange(spellData.hitEffects);
        }

        // --- ĐÂY LÀ LÚC TRẠM KIỂM DUYỆT (INTERCEPTOR) HOẠT ĐỘNG ---
        // Ví dụ: ItemManager.Instance.ApplyModifiers(this);
        // Trạm kiểm duyệt sẽ cộng thêm activeEffects.Add(poisonEffect) hoặc sửa currentBounces
        // ----------------------------------------------------------

        GetComponent<Collider2D>().enabled = true;
        if (animChild != null)
        {
            animChild.Rebind();
            animChild.Update(0f);
        }
        // THIẾT LẬP SPELLTYPE
        if (spellData.isLaser && Player.Instance != null)
        {
            transform.SetParent(Player.Instance.transform);
            transform.localPosition = Vector3.zero;
            rb.linearVelocity = Vector2.zero;
            
            float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            float actualLength = spellData.laserMaxLength;
            if (!spellData.isPenetrating)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, currentDirection, spellData.laserMaxLength, wallLayer);
                if (hit.collider != null) actualLength = hit.distance;
            }

            // Kéo dài BoxCollider2D cho Laser
            BoxCollider2D boxCol = GetComponent<BoxCollider2D>();
            if (boxCol != null)
            {
                boxCol.size = new Vector2(actualLength, spellData.laserWidth);
                boxCol.offset = new Vector2(actualLength / 2f, 0);
            }
            if (visualHolder != null) visualHolder.localPosition = new Vector3(actualLength / 2f, 0, 0);
        }
        else if (spellData.isProjectile || spellData.isPenetrating)
        {
            SetVelocity(currentDirection);
        }
        else if (Player.Instance != null && spellData.isSelfTargeted)
        {
            transform.SetParent(Player.Instance.transform);
            transform.localPosition = Vector3.zero;
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
            GetComponent<Collider2D>().enabled = false;
            ApplyEffectsToTarget(Player.Instance.gameObject);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void Update()
    {
        if (spellData.hasAura) ProcessAura();

        if (spellData.isPenetrating && !spellData.isLaser)
        {
            if (!isHitboxActive && !hasFinishedAttackFrame) 
                currentSpeed += spellData.acceleration * Time.deltaTime;
            else if (hasFinishedAttackFrame) 
                currentSpeed -= spellData.acceleration * Time.deltaTime;

            currentSpeed = Mathf.Max(0, currentSpeed);
            SetVelocity(currentDirection * currentSpeed);
        }

        if (spellData.isRotating)
        {
            Transform targetRotate = visualHolder != null ? visualHolder : transform;
            targetRotate.Rotate(Vector3.forward, spellData.rotationSpeed * Time.deltaTime);
        }
        if (Time.time >= timeActivated + spellData.lifeTime)
        {
            Despawn();
        }
    }
    // ====================================================
    // CÁC HÀM GỌI TỪ ANIMATION EVENT TRÊN PREFAB KỸ NĂNG
    // ====================================================
    public void AnimationEvent_EnableHitbox()
    {
        isHitboxActive = true;
        
        hitTargets.Clear(); 
    }

    public void AnimationEvent_DisableHitbox()
    {
        isHitboxActive = false;
        hasFinishedAttackFrame = true; // Bắt đầu giảm tốc cho Penetrate
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // GÂY SÁT THƯƠNG THEO ATTACK FRAME (Laser & Penetrate)
        if (isHitboxActive && (spellData.isPenetrating || spellData.isLaser))
        {
            int colLayer = collision.gameObject.layer;
            if ((targetLayer.value & (1 << colLayer)) > 0)
            {
                // Chỉ gây sát thương nếu kẻ địch chưa nằm trong danh sách bị đánh ở nhịp này
                if (!hitTargets.Contains(collision.gameObject))
                {
                    ApplyEffectsToTarget(collision.gameObject);
                    hitTargets.Add(collision.gameObject);
                    SpawnImpactVFX();
                }
            }
        }
    }
    // ====================================================
    // 
    // ====================================================

    private void ProcessAura()
    {
        if (spellData.auraTickRate <= 0) return; 
        auraTimer += Time.deltaTime;
        if (auraTimer >= spellData.auraTickRate)
        {
            auraTimer = 0;
            Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, spellData.auraCheckRadius, targetLayer);
            foreach (var target in targets) 
            {
                ApplyEffectsToTarget(target.gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        int colLayer = collision.gameObject.layer;

        if ((wallLayer.value & (1 << colLayer)) > 0)
        {
            if (spellData.isPenetrating) return; // Penetrate bỏ qua vật cản
            HandleObstacleCollision(collision);
            return;
        }

        if ((highObstacleLayer.value & (1 << colLayer)) > 0)
        {
            if (spellData.isPassThroughtHighObstacle || spellData.isPenetrating) return; 
            HandleObstacleCollision(collision);
            return;
        }

        // DÀNH CHO PROJECTILE THÔNG THƯỜNG (Không dùng Attack Frame)
        if (!spellData.isPenetrating && !spellData.isLaser && (targetLayer.value & (1 << colLayer)) > 0)
        {
            ApplyEffectsToTarget(collision.gameObject);
            SpawnImpactVFX();
            
            if (spellData.isProjectile && !spellData.hasAura) Despawn();
            return;
        }

        // TƯƠNG TÁC ĐẠN KẺ ĐỊCH
        if ((EnemyProjectileLayer.value & (1 << colLayer)) > 0)
        {
            EnemyProjectile enemyProj = collision.GetComponent<EnemyProjectile>();
            if (enemyProj != null)
            {
                if (enemyProj.attackData.behavior == AttackBehavior.NormalProjectile || enemyProj.attackData.behavior == AttackBehavior.SpawnAoEOnHit)
                {
                    if (spellData.canReflect) enemyProj.ReflectedByPlayer();
                    else if (spellData.canBlock || spellData.isProjectile)
                    {
                        SpawnImpactVFX();
                        if (spellData.canBlock)
                        {
                            currentBlocks--;
                            if (currentBlocks <= 0) Despawn();
                        }
                        else Despawn();
                        
                        enemyProj.Despawn(); 
                    }
                }
            }
        }
    }
    // --- CÁC HÀM XỬ LÝ VẬT LÝ VÀ LOGIC ---

    private void HandleObstacleCollision(Collider2D collision)
    {
        if (currentBounces > 0)
        {
            currentBounces--;
            PerformBounce();
        }
        else
        {
            SpawnImpactVFX();
            Despawn();
        }
    }

    private void PerformBounce()
    {
        // Bắn Raycast ngược lại một chút để tìm bề mặt vừa va chạm
        RaycastHit2D hit = Physics2D.Raycast(transform.position - (Vector3)currentDirection * 0.5f, currentDirection, 1.5f, wallLayer | highObstacleLayer);
        SpawnImpactVFX();
        if (hit.collider != null)
        {
            // Tính toán hướng dội lại bằng Vector2.Reflect
            currentDirection = Vector2.Reflect(currentDirection, hit.normal).normalized;
            SetVelocity(currentDirection);
        }
        else
        {
            // Fallback nếu Raycast trượt: Đảo ngược hướng hoàn toàn
            currentDirection = -currentDirection;
            SetVelocity(currentDirection);
        }
    }

    private void SetVelocity(Vector2 dir)
    {
        float speedToUse = (spellData.isPenetrating && !spellData.isLaser) ? currentSpeed : spellData.moveSpeed;
        rb.linearVelocity = dir * speedToUse;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // Hàm gọi khi bị dội lại bởi phép Reflect của đối phương
    public void ReflectedByPlayer()
    {
        currentDirection = -currentDirection;
        SetVelocity(currentDirection);
        
        // Đảo layer từ Đạn Địch thành Đạn Người Chơi
        gameObject.layer = LayerMask.NameToLayer("PlayerSpell");
        this.targetLayer = LayerMask.GetMask("Enemy", "Damageable"); // Đổi mục tiêu thành quái
    }

    private void ApplyEffectsToTarget(GameObject target)
    {
        if (activeEffects == null) return;
        
        foreach (var effect in activeEffects)
        {
            effect.ApplyEffect(target, transform.position);
        }
    }

    private void SpawnImpactVFX()
    {
        if (spellData.impactVFX != null)
        {
            Instantiate(spellData.impactVFX, transform.position, Quaternion.identity);
        }
    }

    public void Despawn()
    {
        if (spellData.isSelfTargeted && Player.Instance != null)
        {
            foreach (var effect in activeEffects)
            {
                effect.RemoveEffect(Player.Instance.gameObject);
            }
        }

        if (rb != null) rb.simulated = true;
        transform.SetParent(null);

        // Trả về Pool
        if (SpellPool.Instance != null && prefabOriginal != null)
        {
            SpellPool.Instance.AddToPool(gameObject, prefabOriginal);
        }
        else
        {
            Destroy(gameObject); // Fallback
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (spellData != null && spellData.hasAura)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, spellData.auraCheckRadius);
        }
        if (spellData.hitEffects != null)
        {
            foreach (var effect in spellData.hitEffects)
            {
                if (effect is PullEffect pullEffect)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(transform.position, pullEffect.pullRadius);
                }
                if (effect is DoTEffect dotEffect)
                {
                    Gizmos.color = Color.purple;
                    Gizmos.DrawWireSphere(transform.position, dotEffect.damageRadius);
                }
            }
        }
    }
}