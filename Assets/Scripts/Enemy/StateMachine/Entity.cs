using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using Pathfinding;

public class Entity : MonoBehaviour
{
    //public
    [Header("Data")]
    public D_Entity entityData;
    public FiniteStateMachine stateMachine { get; private set; } 
    public AIPath aiPath { get; private set; }
    [Header("Hit Flash Effect")]
    public Material whiteFlashMat;
    public float flashDuration = 0.1f;
    private SpriteRenderer sr;
    private Material originalMat;

    [Header("Components")]
    public Rigidbody2D rb { get; private set; }
    public Animator anim { get; private set; }
    public bool isAggroed { get; protected set; } = false;

    [Header("Pos")]
    [SerializeField] protected Transform playerCheck;
    public Transform spawnBulletPosition;

    [Tooltip("Tâm quét kiểm tra Player (Nếu để trống sẽ tự lấy tâm quái vật)")]
    public Transform shortRangeActionCenter; 
    public Transform meleeAttackPosition;
    public float lastMeleeTime { get; protected set; } = -Mathf.Infinity;
    public float lastRangedTime { get; protected set; } = -Mathf.Infinity;
    public float lastDodgeTime { get; protected set; } = -Mathf.Infinity;
    protected Vector2 velocityWorkSpace;
    public Vector2 facingDirection { get; protected set; } 
    public int FacingDirX { get; protected set; } = 1;

    private int dirXHash;
    private int dirYHash;
    private int moveHash;
    private int idleHash;
    private int speedHash;

    public float moveSpeedMultiplier = 1f;
    public float damageMultiplier = 1f;
    public float damageTakenMultiplier = 1f;
    protected float currentHealth;
    protected float currentStunResistance;
    protected float lastDamageTime;
    public bool isStunned {get; private set;}
    public float lastStunEndTime { get; protected set; } = -Mathf.Infinity;
    public bool isKamikazeMode { get; protected set; } = false;
    public bool isDead;
    private AnimationToStateMachine ASTM;
    public virtual void Start()
    {
        stateMachine = new FiniteStateMachine();
        aiPath = GetComponent<AIPath>();
        currentHealth = entityData.maxHealth;
        currentStunResistance = entityData.stunResistance;

        facingDirection = Vector2.down;

        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        ASTM = GetComponent<AnimationToStateMachine>();
        sr = GetComponent<SpriteRenderer>();

        dirXHash = Animator.StringToHash("dirX");
        dirYHash = Animator.StringToHash("dirY");
        moveHash = Animator.StringToHash("move");
        idleHash = Animator.StringToHash("idle");
        speedHash = Animator.StringToHash("speed");

        if (sr != null)
        {
            originalMat = sr.material;
        }
    }
    public virtual void Update()
    {
        stateMachine.currentState.LogicUpdate();
        UpdateAnimation();
    }
    public virtual void FixedUpdate()
    {
        stateMachine.currentState.PhysicUpdate();
        
    }
    public virtual void ForceAggro()
    {
        if (isAggroed)
        {
            Debug.Log("already aggro");
            return;
        }
        
        isAggroed = true;
        Debug.Log("aggro");
        ReturnToDefaultState();
    }
    //==================================================
    // MOVEMENT
    //==================================================
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
    
    public virtual void UpdateFacingFromPathfinding(Vector2 moveDirection)
    {
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            // Tận dụng biến facingDirection
            facingDirection = moveDirection.normalized;

            // Nếu quái chỉ có anim trái/phải thì lật mặt
            if (entityData.hasNoAnimLeftOrRight)
            {
                CheckIfShouldFlip(facingDirection.x);
            }
        }
    }
    public virtual void MoveTowardsPlayer()
    {
        if (Player.Instance == null) return;

        if (aiPath != null)
        {
            aiPath.canMove = true;
            aiPath.isStopped = false;
            aiPath.destination = Player.Instance.transform.position;

            // Cập nhật hướng mặt theo vận tốc thực tế của A*
            UpdateFacingFromPathfinding(aiPath.desiredVelocity);
        }
    }
    public virtual void SetVelocity(Vector2 direction, float speed)
    {
        if (direction.sqrMagnitude > 0.01f)
        {
            facingDirection = direction.normalized;
        }

        velocityWorkSpace = direction.normalized * speed;

        rb.linearVelocity = velocityWorkSpace;
    }
    public virtual void StopMovement()
    {
        // 1. Dừng triệt để AIPath để nó không tiếp tục ghi đè vận tốc
        if (aiPath != null)
        {
            aiPath.canMove = false;
            aiPath.isStopped = true;
        }

        // 2. Triệt tiêu quán tính vật lý
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
    public virtual bool CanDodge()
    {
        return Time.time >= lastDodgeTime + entityData.dodgeCooldown;
    }

    public virtual void SetLastDodgeTime()
    {
        lastDodgeTime = Time.time;
    }
    //==================================================
    // FACING
    //==================================================
    public void CheckIfShouldFlip(float xInput)
    {
        if (xInput > 0.01f && FacingDirX == -1)
        {
            Flip();
        }
        else if (xInput < -0.01f && FacingDirX == 1)
        {
            Flip();
        }
    }
    private void Flip()
    {
        FacingDirX *= -1;
        facingDirection = new Vector2(FacingDirX, facingDirection.y); 
        transform.Rotate(0.0f, 180.0f, 0.0f);
    }
    public virtual bool CheckObstacle()
    {
        return Physics2D.Raycast(transform.position, facingDirection, entityData.obstacleCheckDistance, GameLayers.WallAndHighObstacleMask);
    }
    public virtual void ChooseRandom4Direction()
    {
        int randomDir = Random.Range(0, 4);
        switch (randomDir)
        {
            case 0: facingDirection = Vector2.up; break;
            case 1: facingDirection = Vector2.down; break;
            case 2: facingDirection = Vector2.left; break;
            case 3: facingDirection = Vector2.right; break;
        }
        
        if (facingDirection.x != 0 && entityData.hasNoAnimLeftOrRight)
        {
            CheckIfShouldFlip(facingDirection.x);
        }
    }

    //==================================================
    // DAMAGE
    //==================================================
    public virtual bool CanMelee()
    {
        return Time.time >= lastMeleeTime + entityData.meleeCooldown;
    }
    public virtual bool CanRanged()
    {
        return Time.time >= lastRangedTime + entityData.rangedCooldown;
    }
    public virtual void SetLastMeleeTime()
    {
        lastMeleeTime = Time.time;
    }
    public virtual void SetLastRangedTime()
    {
        lastRangedTime = Time.time;
    }
    public virtual void TakeDamage(AttackDetails attackDetails)
    {
        float finalDamage = attackDetails.damageAmount;
        if (isStunned) finalDamage += attackDetails.stunDamageAmount;

        currentHealth -= finalDamage;

        // CHỈ xử lý Flash và Knockback nếu quái vật vẫn còn sống
        if (currentHealth > 0)
        {
            if (sr != null && whiteFlashMat != null) StartCoroutine(FlashWhiteRoutine());
            
            if (rb != null)
            {
                Vector2 knockbackDirection = (transform.position - (Vector3)attackDetails.position).normalized;
                rb.AddForce(knockbackDirection * attackDetails.knockbackForce, ForceMode2D.Impulse);
            }
        }
    }
    public void ApplyAilment(StatusAilment ailmentType, float duration, float value)
    {

    }
    private IEnumerator FlashWhiteRoutine()
    {
        // Tráo thành Material trắng
        sr.material = whiteFlashMat;
        
        // Chờ một khoảng thời gian ngắn
        yield return new WaitForSeconds(flashDuration);
        
        // Trả lại Material gốc để hiện lại hình ảnh bình thường
        sr.material = originalMat;
    }
    public void Die()
    {
        // Reset Collider, máu, v.v trước khi ném lại vào Pool
        gameObject.SetActive(false);
        // Hoặc EnemyPool.Instance.AddToPool(gameObject);    
    }
    //==================================================
    // CC
    //==================================================
    public virtual void ResetStunResistance()
    {
        isStunned = false;

        currentStunResistance =
            entityData.stunResistance;
    }
    public void SetStunStatus(bool status)
    {
        isStunned = status;
    }
    public virtual void ApplyStun(float duration, GameObject stunVfxPrefab) {}

    public virtual bool CanBeStunned()
    {
        return Time.time >= lastStunEndTime + entityData.stunRecoveryTime;
    }

    public virtual void SetLastStunEndTime()
    {
        lastStunEndTime = Time.time;
    }
    public virtual void ApplyPull(Vector2 center, float speed, float duration)
    {

    }
    public virtual void ReturnToDefaultState()
    {
    }
    //==================================================
    // DETECTION
    //==================================================
    public virtual bool CheckPlayerInAggroRange()
    {
        if (Player.Instance == null) return false;

        Vector2 dirToPlayer = Player.Instance.transform.position - transform.position;
        float distance = dirToPlayer.magnitude;

        // 1. Kiểm tra khoảng cách
        if (distance > entityData.visionRange) return false;

        // 2. Kiểm tra góc nhìn
        float angleToPlayer = Vector2.Angle(facingDirection, dirToPlayer);
        if (angleToPlayer > entityData.visionAngle / 2f) 
        {
            // Nằm ngoài góc quét (đang quay lưng lại)
            return false; 
        }

        // 3. Phóng Raycast
        // Quan trọng: Chỉ quét obstacleLayer. Đảm bảo obstacleLayer trên D_Entity CHỈ tick "Wall/Ground", không tick "Player" hay "Enemy".
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToPlayer.normalized, distance, GameLayers.WallAndHighObstacleMask);
        
        if (hit.collider != null) 
        {
            // BẬT CONSOLE ĐỂ XEM DÒNG NÀY: Nếu nó in ra tên của Player hoặc tên của chính con quái vật, bạn đã set sai LayerMask!
            Debug.Log("Tầm nhìn của quái bị che bởi: " + hit.collider.gameObject.name);
            return false; 
        }

        Debug.Log("PHÁT HIỆN PLAYER -> CHUYỂN STATE!");
        return true; // Hoàn toàn nhìn thấy Player
    }
    public virtual Transform GetNearestSpell()
    {
        if (!entityData.canDodgeSpells) return null;

        // 1. Lấy TẤT CẢ đạn phép nằm trong bán kính nhận diện
        Collider2D[] spellCols = Physics2D.OverlapCircleAll(transform.position, entityData.spellDetectionRadius, GameLayers.PlayerAttackLayerIndex);
        
        Transform nearestSpell = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D spellCol in spellCols)
        {
            Vector2 dirToSpell = spellCol.transform.position - transform.position;
            float distance = dirToSpell.magnitude;

            // 2. Kiểm tra Góc nhìn (Vision Cone)
            // Tận dụng góc nhìn visionAngle của quái vật để check xem đạn có nằm trước mặt không
            float angleToSpell = Vector2.Angle(facingDirection, dirToSpell);
            
            if (angleToSpell <= entityData.visionAngle / 2f)
            {
                // 3. Kiểm tra Tường chắn (Raycast)
                // Đảm bảo đạn không bị khuất sau bức tường
                RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToSpell.normalized, distance, GameLayers.WallAndHighObstacleMask);
                
                if (hit.collider == null) 
                {
                    // 4. Lọc ra viên đạn gần nhất nếu có nhiều viên bay tới cùng lúc
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestSpell = spellCol.transform;
                    }
                }
            }
        }

        return nearestSpell; // Sẽ trả về null nếu đạn ở sau lưng hoặc bị tường che
    }
    public virtual bool CheckPlayerInShortRangeAction()
    {
        Transform center = shortRangeActionCenter != null ? shortRangeActionCenter : transform;
        return Vector2.Distance(Player.Instance.transform.position, center.position) <= entityData.shortRangeAction;
    }

    //==================================================
    // ANIMATION
    //==================================================
    protected virtual void UpdateAnimation()
    {
        if (anim == null)
            return;

        anim.SetFloat(dirXHash, facingDirection.x);
        anim.SetFloat(dirYHash, facingDirection.y);

        float currentSpeed = 0f;
        if (aiPath != null && aiPath.canMove)
        {
            currentSpeed = aiPath.velocity.magnitude; 
        }
        else if (rb != null)
        {
            currentSpeed = rb.linearVelocity.magnitude;
        }
        anim.SetFloat(speedHash, currentSpeed);
    }
    
    //==================================================
    // GIZMOS
    //==================================================
    public virtual void OnDrawGizmos()
    {
        if (entityData == null || playerCheck == null)
            return;

        Gizmos.DrawWireSphere(
            playerCheck.position,
            entityData.shortRangeAction
        );
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)facingDirection * entityData.obstacleCheckDistance);

        Gizmos.color = Color.yellow;
        Vector3 currentPos = transform.position;
        Gizmos.DrawWireSphere(currentPos, entityData.visionRange);
        Vector3 rightVisionDir = Quaternion.Euler(0, 0, entityData.visionAngle / 2f) * facingDirection;
        Vector3 leftVisionDir = Quaternion.Euler(0, 0, -entityData.visionAngle / 2f) * facingDirection;
        Gizmos.DrawLine(currentPos, currentPos + rightVisionDir * entityData.visionRange);
        Gizmos.DrawLine(currentPos, currentPos + leftVisionDir * entityData.visionRange);
    }
}