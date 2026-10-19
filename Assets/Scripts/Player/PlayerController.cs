using UnityEngine;

// Tự động gắn các linh kiện khi bạn kéo script này vào Player
[RequireComponent(typeof(HealthComponent), typeof(PlayerManaComponent))]
[RequireComponent(typeof(CoreCCComponent), typeof(KnockbackComponent))] 
[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Data")]
    public D_Player playerData;
    
    [Header("Combat setup")]
    public Transform attackPosition; // <-- Điểm phát bắn phép thuật / chém thường
    
    [Header("Core Systems")]
    public PlayerStateMachine StateMachine { get; private set; }
    public PlayerInputHandler InputHandler { get; private set; }
    public PlayerSpellController SpellController { get; private set; }
    public KnockbackComponent Knockback { get; private set; }

    // --- CÁC LINH KIỆN ĐỘC LẬP (COMPONENTS) ---
    public HealthComponent Health { get; private set; }
    public PlayerManaComponent Mana { get; private set; }
    public PlayerLevelComponent Exp { get; private set; }
    public CoreCCComponent CC { get; private set; } // <-- Đã bổ sung
    public Rigidbody2D rb { get; private set; }
    public Animator anim { get; private set; }

    // --- BIẾN TRẠNG THÁI VÀ FSM ---
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerDashState DashState { get; private set; }
    public PlayerNormalAttackState NormalAttackState { get; private set; }
    public PlayerHurtState HurtState { get; private set; }
    public PlayerCastingSpellState CastingSpellState { get; private set; }
    public PlayerStunnedState StunnedState { get; private set; }
    public PlayerSuctionState SuctionState { get; private set; }
    public PlayerVacuumState VacuumState { get; private set; }

    public float moveSpeedMultiplier = 1f;
    public float damageMultiplier = 1f; 
    public Vector2 facingDirection { get; set; } = Vector2.down;
    public int FacingDirX { get; protected set; } = 1;
    public int currentSpellSlot { get; set; }

    [Header("Runtime Info (Read Only)")]
    public float dashCooldownTimer;
    public float normalAttackCooldownTimer;
    public bool isInvincible;

    private void Awake()
    {
        Instance = this;
        
        // 1. LẤY CÁC THÀNH PHẦN
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        InputHandler = GetComponent<PlayerInputHandler>();
        SpellController = GetComponent<PlayerSpellController>();
        
        Health = GetComponent<HealthComponent>();
        Mana = GetComponent<PlayerManaComponent>();
        Exp = GetComponent<PlayerLevelComponent>();
        CC = GetComponent<CoreCCComponent>();
        Knockback = GetComponent<KnockbackComponent>();

        // 2. KHỞI TẠO STATE MACHINE (Dùng Hash Integer để tối ưu)
        StateMachine = new PlayerStateMachine();
        IdleState = new PlayerIdleState(this, StateMachine, playerData, AnimHashes.Idle);
        MoveState = new PlayerMoveState(this, StateMachine, playerData, AnimHashes.Move);
        DashState = new PlayerDashState(this, StateMachine, playerData, AnimHashes.Dodge);
        NormalAttackState = new PlayerNormalAttackState(this, StateMachine, playerData, AnimHashes.Attack);
        HurtState = new PlayerHurtState(this, StateMachine, playerData, AnimHashes.Hurt);
        CastingSpellState = new PlayerCastingSpellState(this, StateMachine, playerData, AnimHashes.Attack);
        StunnedState = new PlayerStunnedState(this, StateMachine, playerData, AnimHashes.Hurt);
        SuctionState = new PlayerSuctionState(this, StateMachine, playerData, AnimHashes.Hurt);
        VacuumState = new PlayerVacuumState(this, StateMachine, playerData, AnimHashes.Hurt);
        // 3. ĐĂNG KÝ LẮNG NGHE SỰ KIỆN (EVENTS)
        Health.OnTakeDamage.AddListener(HandleHurt);
        Health.OnDeath.AddListener(HandleDeath);
        
        // Quan trọng: Khi CC bị kích hoạt (Trúng Stun), ép PlayerController hủy bỏ hành động hiện tại
        CC.OnInterrupt.AddListener(HandleInterrupt);

        Knockback.OnPullTriggered.AddListener(HandleSuction);
    }

    private void Start()
    {
        // Khởi tạo các Component Runtime (Không sửa vào file Data gốc)
        Health.Init(playerData.maxHealth);
        Mana.Init(playerData.maxMana, 5); 
        Exp.Init(0, playerData.xpFirstLevel, 1);
        StateMachine.Initialize(IdleState);
    }

    private void Update()
    {
        StateMachine.currentState.LogicUpdate();
        UpdateCooldowns();

        // Chỉ cho phép nhập Input phép thuật khi đang rảnh rỗi và không bị khống chế
        if (!CC.isStunned && (StateMachine.currentState == IdleState || StateMachine.currentState == MoveState))
        {
            CheckSpellInput();
        }
    }

    private void FixedUpdate()
    {
        StateMachine.currentState.PhysicUpdate();
    }

    // =========================================================
    // EVENT HANDLERS (Phản ứng với các hệ thống khác)
    // =========================================================
    
    private void HandleHurt()
    {
        // Nhận sát thương -> Đánh thức (nếu đang ngủ) và ép StateMachine chuyển sang bị thương
        CC.WakeUp();
        StateMachine.ChangeState(HurtState);
    }
    private void HandleDeath()
    {
        Debug.Log("Player Die! Game Over!");
        // Thêm logic hiển thị UI Game Over ở đây
    }
    private void HandleInterrupt()
    {
        if (CC.isStunned) 
            StateMachine.ChangeState(StunnedState);
        else 
            StateMachine.ChangeState(HurtState); 
    }

    private void HandleSuction(float duration)
    {
        // Chỉ kích hoạt Suction nếu đang không bị Choáng
        if (!CC.isStunned)
        {
            SuctionState.SetDuration(duration);
            StateMachine.ChangeState(SuctionState);
        }
    }
    public void HandleVacuumStart()
    {
        // Nếu không bị Stun cứng thì mới đổi sang State bị hút
        if (!CC.isStunned) 
        {
            StateMachine.ChangeState(VacuumState);
        }
    }
    public void HandleVacuumEnd()
    {
        if (StateMachine.currentState == VacuumState)
        {
            StateMachine.ChangeState(IdleState);
        }
    }
    // =========================================================
    // TIỆN ÍCH DÀNH CHO STATE
    // =========================================================
    
    public void StopVelocity() => rb.linearVelocity = Vector2.zero;

    public void CheckIfShouldFlip(float xInput)
    {
        if ((xInput > 0.01f && FacingDirX == -1) || (xInput < -0.01f && FacingDirX == 1))
        {
            FacingDirX *= -1;
            facingDirection = new Vector2(FacingDirX, facingDirection.y); 
            transform.Rotate(0.0f, 180.0f, 0.0f);
        }
    }

    public void ResetNormalAttackCooldown() => normalAttackCooldownTimer = playerData.normalAttackCooldown;
    
    public bool CanDash() => dashCooldownTimer <= 0;

    private void UpdateCooldowns()
    {
        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;
        if (normalAttackCooldownTimer > 0) normalAttackCooldownTimer -= Time.deltaTime;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateCooldownUI(0, normalAttackCooldownTimer, playerData.normalAttackCooldown);
            UIManager.Instance.UpdateCooldownUI(6, dashCooldownTimer, playerData.dashCooldown);
        }
    }

    private void CheckSpellInput()
    {
        int requestedSlot = -1;
        if (InputHandler.Spell1Input) requestedSlot = 0;
        else if (InputHandler.Spell2Input) requestedSlot = 1;
        else if (InputHandler.Spell3Input) requestedSlot = 2;
        else if (InputHandler.Spell4Input) requestedSlot = 3;
        else if (InputHandler.Spell5Input) requestedSlot = 4;

        if (requestedSlot != -1)
        {
            InputHandler.UseSpellInputByIndex(requestedSlot);
            
            if (SpellController.CanCastSpell(requestedSlot))
            {
                currentSpellSlot = requestedSlot;
                StateMachine.ChangeState(CastingSpellState);
            }
        }
    }
}