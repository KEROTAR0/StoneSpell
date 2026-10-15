using UnityEngine;
using Pathfinding;

// TỰ ĐỘNG GẮN CÁC LINH KIỆN LÕI CHO QUÁI VẬT
[RequireComponent(typeof(HealthComponent), typeof(KnockbackComponent), typeof(CoreCCComponent))]
[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(AIPath))]
public class EnemyController : MonoBehaviour
{
    [Header("Data")]
    public D_Entity entityData;
    // =======================================
    // KHAI BÁO CÁC STATE
    // =======================================
    public PatrolState patrolState { get; private set; }
    public ObserveState observeState { get; private set; }
    public ChaseState chaseState { get; private set; }
    public DeadState deadState { get; private set; }
    public MeleeState meleeState { get; private set; }
    public RangedState rangedState { get; private set; }
    public RepositionState repositionState { get; private set; }
    public SelfDestructState selfDestructState { get; private set; }
    public DodgeState dodgeState { get; private set; }
    public StunnedState stunnedState { get; private set; }
    public VacuumState vacuumState { get; private set; }
    public SeekManaState seekManaState { get; private set; }
    public DetonateManaState detonateManaState { get; private set; }
    
    // --- HỆ THỐNG ĐIỀU PHỐI (FSM & AI) ---
    public FiniteStateMachine stateMachine { get; private set; } 
    public AIPath aiPath { get; private set; }

    // --- CÁC LINH KIỆN (COMPONENTS) ---
    public HealthComponent Health { get; private set; }
    public KnockbackComponent Knockback { get; private set; }
    public CoreCCComponent CC { get; private set; }
    public PullableComponent Pullable { get; private set; }

    [Header("Hit Flash Effect (Giao cho Component phụ trách VFX xử lý thì tốt hơn)")]
    public Material whiteFlashMat;
    private Coroutine flashCoroutine;
    public float flashDuration = 0.05f;
    private SpriteRenderer sr;
    private Material originalMat;

    [Header("Components")]
    public Rigidbody2D rb { get; private set; }
    public Animator anim { get; private set; }
    public bool isAggroed { get; protected set; } = false;

    [Header("Pos")]
    [SerializeField] protected Transform playerCheck;
    public Transform spawnBulletPosition;
    [Tooltip("Tâm quét kiểm tra Player")]
    public Transform shortRangeActionCenter; 
    public Transform meleeAttackPosition;

    // Các biến Cooldown
    public float lastMeleeTime { get; protected set; } = -Mathf.Infinity;
    public float lastRangedTime { get; protected set; } = -Mathf.Infinity;
    public float lastDodgeTime { get; protected set; } = -Mathf.Infinity;
    
    // Biến Di chuyển
    protected Vector2 velocityWorkSpace;
    public Vector2 facingDirection { get; protected set; } 
    public int FacingDirX { get; protected set; } = 1;

    public bool isKamikazeMode { get; protected set; } = false;
    private AnimationToStateMachine ASTM;

    public virtual void Start()
    {
        // 1. LẤY CÁC THÀNH PHẦN
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        aiPath = GetComponent<AIPath>();
        ASTM = GetComponent<AnimationToStateMachine>();
        sr = GetComponent<SpriteRenderer>();

        Health = GetComponent<HealthComponent>();
        Knockback = GetComponent<KnockbackComponent>();
        CC = GetComponent<CoreCCComponent>();
        Pullable = GetComponent<PullableComponent>();

        // 2. KHỞI TẠO DỮ LIỆU RUNTIME CHO COMPONENT
        Health.Init(entityData.maxHealth);

        // 3. ĐĂNG KÝ SỰ KIỆN (Lắng nghe khi bị thương / bị khống chế)
        Health.OnTakeDamage.AddListener(HandleHurt);
        Health.OnDeath.AddListener(Die);
        CC.OnInterrupt.AddListener(HandleInterrupt);

        stateMachine = new FiniteStateMachine();
        facingDirection = Vector2.down;
        if (sr != null) originalMat = sr.material;

        // Khởi tạo các State cơ bản (Mọi quái đều có)
        patrolState = new PatrolState(this, stateMachine, AnimHashes.Move);
        observeState = new ObserveState(this, stateMachine, AnimHashes.Idle);
        deadState = new DeadState(this, stateMachine, AnimHashes.Dead);
        stunnedState = new StunnedState(this, stateMachine, AnimHashes.Stun);
        vacuumState = new VacuumState(this, stateMachine, AnimHashes.Hurt);

        // Khởi tạo State chiến đấu (Tùy chọn)
        if (entityData.meleeAttackPrefab != null) 
            meleeState = new MeleeState(this, stateMachine, AnimHashes.Melee, entityData.meleeAttackPrefab);

        if (entityData.rangedAttackPrefab != null)
        {
            rangedState = new RangedState(this, stateMachine, AnimHashes.Ranged, entityData.rangedAttackPrefab);
            repositionState = new RepositionState(this, stateMachine, AnimHashes.Move);
        }

        if (entityData.canSelfDestruct) 
            selfDestructState = new SelfDestructState(this, stateMachine, AnimHashes.Destruct);

        if (entityData.canDodge) 
            dodgeState = new DodgeState(this, stateMachine, AnimHashes.Dodge);

        // ChaseState sẽ tự quản lý các logic chuyển đổi sang Đánh Gần/Bắn Xa/Né/Bom
        chaseState = new ChaseState(this, stateMachine, AnimHashes.Move, meleeState, selfDestructState, rangedState, repositionState, dodgeState);
        if (entityData.isManaHunter)
        {
            seekManaState = new SeekManaState(this, stateMachine, AnimHashes.Move);
            
            // Dùng chung Anim Melee để tiết kiệm, hoặc bạn có thể tạo AnimHashes.Stomp riêng
            detonateManaState = new DetonateManaState(this, stateMachine, AnimHashes.Melee);
        }
        // Nối dây logic đi tuần
        patrolState.SetTransitions(observeState);
        observeState.SetTransitions(patrolState);

        // Bắt đầu nhịp FSM
        stateMachine.Initialize(observeState);
    }

    public virtual void Update()
    {
        // Nếu bị Choáng hoặc Ngủ, ngưng cập nhật Logic của State hiện tại
        if (!CC.isStunned)
        {
            stateMachine.currentState.LogicUpdate();
        }
        UpdateAnimation();
    }

    public virtual void FixedUpdate()
    {
        if (!CC.isStunned)
        {
            stateMachine.currentState.PhysicUpdate();
        }
    }

    // =========================================================
    // EVENT HANDLERS (Giao tiếp với Component)
    // =========================================================
    
    protected virtual void HandleHurt()
    {
        // 1. Kiểm tra trạng thái chết
        if (stateMachine.currentState == deadState) return;

        // Bỏ qua nếu đang Choáng cứng (Stun). KHÔNG bỏ qua nếu đang Ngủ (Sleep) hoặc bình thường.
        if (stateMachine.currentState == stunnedState && CC != null && !CC.isSleeping)
        {
            // Ngay cả khi đang bị Choáng (Stun), ta VẪN cho phép Nhấp nháy trắng khi ăn đòn, chỉ là không đổi State thôi!
            TriggerFlashWhite(); 
            return;
        }

        // Đánh thức nếu đang ngủ
        if (CC != null) CC.WakeUp();
        
        isAggroed = true;

        // ==========================================
        // GỌI HIỆU ỨNG NHÁY TRẮNG (FLASH WHITE)
        // ==========================================
        TriggerFlashWhite();

        // 2. Chuyển đổi State nếu cần thiết (Khi đang đi tuần/quan sát)
        if (stateMachine.currentState == patrolState || 
            stateMachine.currentState == observeState || 
            stateMachine.currentState == stunnedState) 
        {
            ReturnToDefaultState(); 
        }
    }
    private void TriggerFlashWhite()
    {
        if (sr != null && whiteFlashMat != null)
        {
            // NẾU ĐANG NHẤY: Tắt ngay Coroutine cũ để tránh việc nó tự phục hồi Material gốc sớm
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            // Khởi chạy lượt nhấp nháy mới (Thời gian flash sẽ được reset về 0.1s từ đầu)
            flashCoroutine = StartCoroutine(FlashWhiteRoutine());
        }
    }
    public virtual void HandleInterrupt()
    {
        if (stateMachine.currentState == deadState) return;
        StopMovement();
        
        if (CC != null)
        {
            if (Pullable != null && Pullable.isBeingVacuumed)
            {
                stateMachine.ChangeState(vacuumState);
                Debug.Log("vacuum!");
                return;
            }
            if (CC.isStunned || CC.isSleeping)
            {
                stunnedState.SetupStun(CC.isSleeping); 
                stateMachine.ChangeState(stunnedState);
            }
        }
    }

    private System.Collections.IEnumerator FlashWhiteRoutine()
    {
        sr.material = whiteFlashMat;
        yield return new WaitForSeconds(flashDuration);
        sr.material = originalMat;
        flashCoroutine = null; // Giải phóng
    }

    public virtual void Die()
    {
        gameObject.SetActive(false);
    }

    public virtual void ForceAggro()
    {
        if (isAggroed) return;
        isAggroed = true;
        ReturnToDefaultState();
    }

    // =========================================================
    // HỆ THỐNG DI CHUYỂN VÀ NHẬN DIỆN (Giữ nguyên logic cũ)
    // =========================================================
    public virtual Vector2 GetRandomWalkablePoint()
    {
        Vector2 randomPoint = (Vector2)transform.position + Random.insideUnitCircle * entityData.patrolRadius;

        if (AstarPath.active != null)
        {
            Pathfinding.NNInfo nnInfo = AstarPath.active.GetNearest(randomPoint, Pathfinding.NNConstraint.Default);
            return (Vector3)nnInfo.node.position; 
        }
        return randomPoint;
    }
    public virtual void StopMovement()
    {
        if (aiPath != null)
        {
            aiPath.canMove = false;
            aiPath.isStopped = true;
        }
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    public virtual void MoveTowardsPlayer()
    {
        if (PlayerController.Instance == null) return;

        if (aiPath != null)
        {
            // Ép tốc độ AI Path bằng đúng tốc độ quy định nhân với hệ số Slow (nếu có)
            aiPath.maxSpeed = entityData.movementSpeed * CC.moveSpeedMultiplier;
            
            aiPath.canMove = true;
            aiPath.isStopped = false;
            aiPath.destination = PlayerController.Instance.transform.position;
            UpdateFacingFromPathfinding(aiPath.desiredVelocity);
        }
    }

    public virtual void SetVelocity(Vector2 direction, float speed)
    {
        if (direction.sqrMagnitude > 0.01f) facingDirection = direction.normalized;
        
        // Nhận hệ số giảm tốc từ Component CC
        float finalSpeed = speed * CC.moveSpeedMultiplier;
        
        velocityWorkSpace = direction.normalized * finalSpeed;
        rb.linearVelocity = velocityWorkSpace;
    }

    public virtual void UpdateFacingFromPathfinding(Vector2 moveDirection)
    {
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            facingDirection = moveDirection.normalized;
            if (entityData.hasNoAnimLeftOrRight) CheckIfShouldFlip(facingDirection.x);
        }
    }

    public void CheckIfShouldFlip(float xInput)
    {
        if ((xInput > 0.01f && FacingDirX == -1) || (xInput < -0.01f && FacingDirX == 1))
        {
            FacingDirX *= -1;
            facingDirection = new Vector2(FacingDirX, facingDirection.y); 
            transform.Rotate(0.0f, 180.0f, 0.0f);
        }
    }

    public virtual bool CheckObstacle()
    {
        return Physics2D.Raycast(transform.position, facingDirection, entityData.obstacleCheckDistance, GameLayers.WallAndHighObstacleMask);
    }

    public virtual bool CheckPlayerInAggroRange()
    {
        if (PlayerController.Instance == null) return false;

        Vector2 dirToPlayer = PlayerController.Instance.transform.position - transform.position;
        float distance = dirToPlayer.magnitude;

        if (distance > entityData.visionRange) return false;
        
        float angleToPlayer = Vector2.Angle(facingDirection, dirToPlayer);
        if (angleToPlayer > entityData.visionAngle / 2f) return false; 

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToPlayer.normalized, distance, GameLayers.WallAndHighObstacleMask);
        if (hit.collider != null) return false; 

        return true;
    }

    public virtual bool CheckPlayerInShortRangeAction()
    {
        Transform center = shortRangeActionCenter != null ? shortRangeActionCenter : transform;
        return Vector2.Distance(PlayerController.Instance.transform.position, center.position) <= entityData.shortRangeAction;
    }

    public virtual Transform GetNearestSpell()
    {
        if (!entityData.canDodgeSpells) return null;

        Collider2D[] spellCols = Physics2D.OverlapCircleAll(transform.position, entityData.spellDetectionRadius, GameLayers.PlayerAttackMask);        
        Transform nearestSpell = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D spellCol in spellCols)
        {
            Vector2 dirToSpell = spellCol.transform.position - transform.position;
            float distance = dirToSpell.magnitude;
            float angleToSpell = Vector2.Angle(facingDirection, dirToSpell);
            
            if (angleToSpell <= entityData.visionAngle / 2f)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToSpell.normalized, distance, GameLayers.WallAndHighObstacleMask);
                if (hit.collider == null && distance < minDistance)
                {
                    minDistance = distance;
                    nearestSpell = spellCol.transform;
                }
            }
        }
        return nearestSpell; 
    }
    public Transform GetNearestManaShard()
    {
        // Quét Layer Item/Mana
        Collider2D[] shards = Physics2D.OverlapCircleAll(transform.position, entityData.visionRange, GameLayers.ManaMask);        
        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D col in shards)
        {
            float distance = Vector2.Distance(transform.position, col.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = col.transform;
            }
        }
        return nearest;
    }
    // =========================================================
    // COOLDOWNS & ANIMATION
    // =========================================================
    
    public virtual bool CanDodge() => Time.time >= lastDodgeTime + entityData.dodgeCooldown;
    public virtual void SetLastDodgeTime() => lastDodgeTime = Time.time;
    public virtual bool CanMelee() => Time.time >= lastMeleeTime + entityData.meleeCooldown;
    public virtual bool CanRanged() => Time.time >= lastRangedTime + entityData.rangedCooldown;
    public virtual void SetLastMeleeTime() => lastMeleeTime = Time.time;
    public virtual void SetLastRangedTime() => lastRangedTime = Time.time;
    public void ReturnToDefaultState()
    {
        if (isAggroed)
        {
            if (entityData.isManaHunter) 
                stateMachine.ChangeState(seekManaState);
            else 
                stateMachine.ChangeState(chaseState);
        }
        else
        {
            stateMachine.ChangeState(patrolState);
        }
    }
    
    // (LƯU Ý: Khung gầm AnimHashes đã được chuyển về chuẩn số nguyên)
    protected virtual void UpdateAnimation()
    {
        if (anim == null) return;
        anim.SetFloat(AnimHashes.DirX, facingDirection.x);
        anim.SetFloat(AnimHashes.DirY, facingDirection.y);

        float currentSpeed = 0f;
        if (aiPath != null && aiPath.canMove) currentSpeed = aiPath.velocity.magnitude; 
        else if (rb != null) currentSpeed = rb.linearVelocity.magnitude;
        
        anim.SetFloat(AnimHashes.Speed, currentSpeed);
    }
}