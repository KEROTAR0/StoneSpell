// using UnityEngine;
// using System.Collections;
// using Unity.Android.Gradle.Manifest;

// public class Player : MonoBehaviour, IDamageable
// {
//     public static Player Instance { get; private set; }
//     [Header("Data")]
//     public D_Player playerData;
//     public PlayerStateMachine StateMachine {get; private set;}
//     public PlayerInputHandler InputHandler {get; private set;}
    
//     [Header("Components")]
//     public Rigidbody2D rb { get; private set; }
//     public Animator anim { get; private set; }
//     public GameObject playerGO { get; private set; }
//     [Header("Dash")]
//     public float dashCooldownTimer;
//     private Coroutine slowCoroutine;
//     [Header("Runtime Buff Modifiers")]
//     public float moveSpeedMultiplier = 1f;
//     public float damageMultiplier = 1f;

//     public float manaGainMultiplier = 1f;
//     [Header("Mana On Hit Buff")]
//     public float manaOnHitChance = 0f;
//     public float manaOnHitAmount = 0f;
//     [Header("Combat")]
//     public Transform attackPosition;
//     [Header("Status Ailments")]
//     public bool isStunned;
//     public bool isSlowed;
//     public bool isStopped;
//     protected Vector2 velocityWorkSpace;
//     protected float currentHealth;
//     protected float currentStunResistance;
//     protected float lastDamageTime;
//     protected bool isDead;
//     public bool isInvincible { get; set; }
//     public Vector2 facingDirection { get; set; } 
//     public int FacingDirX { get; protected set; } = 1;
//     public float normalAttackCooldownTimer { get; private set; }
//     public float currentMana { get; private set; }
//     private int normalHitCount = 0;
//     public int hitCountNeedToSpawnMana = 5;
//     // Cờ mở rộng: Bật true thì mảnh mana sẽ tự bay về người chơi dù ở bất kỳ đâu
//     public bool isGlobalVacuum { get; set; } = false;
//     public int currentSpellSlot { get; set; }

//     [Header("State")]
//     public PlayerIdleState IdleState  { get; private set; }
//     public PlayerMoveState MoveState { get; private set; }
//     public PlayerDashState DashState { get; private set; }
//     public PlayerNormalAttackState NormalAttackState { get; private set; }
//     public PlayerHurtState HurtState { get; private set; }
//     public PlayerSpellController SpellController { get; private set; }
//     public PlayerCastingSpellState CastingSpellState { get; private set; }

//     private void Awake()
//     {
//         Instance = this;

//         StateMachine = new PlayerStateMachine();

//         IdleState = new PlayerIdleState(this, StateMachine, playerData, AnimHashes.Idle);
//         MoveState = new PlayerMoveState(this, StateMachine, playerData, AnimHashes.Move);
//         DashState = new PlayerDashState(this, StateMachine, playerData, AnimHashes.Dodge);
//         NormalAttackState = new PlayerNormalAttackState(this, StateMachine, playerData, AnimHashes.Attack);
//         HurtState = new PlayerHurtState(this, StateMachine, playerData, AnimHashes.Hurt);
//         CastingSpellState = new PlayerCastingSpellState(this, StateMachine, playerData, AnimHashes.Attack);
//     }
//     private void Start()
//     {
//         currentMana = 120;
//         currentHealth = playerData.maxHealth;
//         facingDirection = Vector2.down;
//         playerGO = gameObject;
//         anim = GetComponent<Animator>();
//         rb = GetComponent<Rigidbody2D>();
//         SpellController = GetComponent<PlayerSpellController>();

//         StateMachine.Initialize(IdleState);
//         InputHandler = GetComponent<PlayerInputHandler>();
//         if (UIManager.Instance != null)
//         {
//             UIManager.Instance.UpdateHealthUI(currentHealth, playerData.maxHealth);
//             UIManager.Instance.UpdateManaUI(currentMana, playerData.maxMana);
//             UIManager.Instance.UpdateCooldownUI(0, 0, playerData.normalAttackCooldown);
//             UIManager.Instance.UpdateCooldownUI(6, 0, playerData.dashCooldown);
//         }
//     }

//     // Update is called once per frame
//     private void Update()
//     {
//         StateMachine.currentState.LogicUpdate();

//         if (dashCooldownTimer > 0)
//         {
//             dashCooldownTimer -= Time.deltaTime;
//         }
//         if (normalAttackCooldownTimer > 0)
//         {
//             normalAttackCooldownTimer -= Time.deltaTime;
//         }
//         if (UIManager.Instance != null)
//         {
//             UIManager.Instance.UpdateCooldownUI(0, normalAttackCooldownTimer, playerData.normalAttackCooldown);
//             UIManager.Instance.UpdateCooldownUI(6, dashCooldownTimer, playerData.dashCooldown);
//         }
//         if (StateMachine.currentState == IdleState || StateMachine.currentState == MoveState)
//         {
//             CheckSpellInput();
//         }
//     }
//     private void FixedUpdate()
//     {
//         StateMachine.currentState.PhysicUpdate();
//     }
//     //MANA
//     public void AddMana(float amount)
//     {
//         currentMana = Mathf.Clamp(currentMana + (amount * manaGainMultiplier), 0, playerData.maxMana);
//         if (UIManager.Instance != null)
//         {
//             UIManager.Instance.UpdateManaUI(currentMana, playerData.maxMana);
//         }
//     }
//     //MOVEMENT
//     public bool CanDash()
//     {
//         return dashCooldownTimer <= 0;
//     }
//     private void Flip()
//     {
//         FacingDirX *= -1;
//         facingDirection = new Vector2(FacingDirX, facingDirection.y); 
//         transform.Rotate(0.0f, 180.0f, 0.0f);
//     }
//     public void CheckIfShouldFlip(float xInput)
//     {
//         if (xInput > 0.01f && FacingDirX == -1)
//         {
//             Flip();
//         }
//         else if (xInput < -0.01f && FacingDirX == 1)
//         {
//             Flip();
//         }
//     }
//     //UI
    
//     public void Stop() =>   rb.linearVelocity = Vector2.zero;

//     public void Slow(float slowRate, float slowDuration)
//     {
//         // Hủy tiến trình làm chậm cũ nếu có
//         if (slowCoroutine != null) 
//         {
//             StopCoroutine(slowCoroutine);
//         }
//         slowCoroutine = StartCoroutine(SlowRoutine(slowRate, slowDuration));
//     }
//     private IEnumerator SlowRoutine(float slowRate, float duration)
//     {
//         float timer = 0f;
//         while (timer < duration)
//         {
//             timer += Time.deltaTime;
            
//             // Giảm dần vận tốc hiện tại về 0. slowRate càng lớn thì phanh càng gấp.
//             rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, slowRate * Time.deltaTime);
            
//             // Đợi đến frame tiếp theo
//             yield return null; 
//         }
//         Stop();
//     }
//     //COMBAT
//     public void ResetNormalAttackCooldown() => normalAttackCooldownTimer = playerData.normalAttackCooldown;

//     public void RecordNormalHit(Vector2 hitPosition)
//     {
//         normalHitCount++;
//         Debug.Log("<color=green>>>> RECORD NORMAL HIT!</color>");
//         if (normalHitCount >= hitCountNeedToSpawnMana)
//         {
//             normalHitCount = 0;
//             GameManager.Instance.SpawnMana(transform.position);
//             Debug.Log("<color=blue>>>> RECORD NORMAL HIT SPAWN MANA!</color>");
//         }
//     }
//     private void CheckSpellInput()
//     {
//         int requestedSlot = -1;

//         if (InputHandler.Spell1Input) requestedSlot = 0;
//         else if (InputHandler.Spell2Input) requestedSlot = 1;
//         else if (InputHandler.Spell3Input) requestedSlot = 2;
//         else if (InputHandler.Spell4Input) requestedSlot = 3;
//         else if (InputHandler.Spell5Input) requestedSlot = 4;

//         if (requestedSlot != -1)
//         {
//             InputHandler.UseSpellInputByIndex(requestedSlot);

//             if (SpellController.CanCastSpell(requestedSlot))
//             {
//                 currentSpellSlot = requestedSlot;
//                 StateMachine.ChangeState(CastingSpellState);
//             }
//         }
//     }
//     public void TakeDamage(AttackDetails attackDetails)
//     {
//         if (isDead || isInvincible) return;

//         currentHealth -= attackDetails.damageAmount;
        
//         if (UIManager.Instance != null)
//         {
//             UIManager.Instance.UpdateHealthUI(currentHealth, playerData.maxHealth);
//         }

//         if (rb != null)
//         {
//             Vector2 knockbackDir = (transform.position - (Vector3)attackDetails.position).normalized;
//             rb.linearVelocity = Vector2.zero; // Xóa vận tốc cũ để đẩy chính xác hơn
//             rb.AddForce(knockbackDir * attackDetails.knockbackForce, ForceMode2D.Impulse);
//         }

//         if (currentHealth <= 0)
//         {
//             currentHealth = 0;
//             isDead = true;
//             // Gọi hàm chết ở đây
//         }
//         else
//         {
//             // NGẮT QUÃNG (INTERRUPT): Chuyển ngay sang HurtState bất kể đang làm gì (kể cả casting)
//             StateMachine.ChangeState(HurtState);
//         }
//     }
// }
