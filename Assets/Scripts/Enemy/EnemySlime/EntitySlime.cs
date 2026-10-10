using UnityEngine;

public class EntitySlime : Entity, IDamageable
{
    //State
    public SuctionState suctionState { get; private set; }
    public StunnedState stunnedState { get; private set; }
    public MeleeState meleeState { get; private set; }
    public RangedState rangedState { get; private set; }
    public DeadState deadState { get; private set; }
    public SelfDestructState selfDestructState { get; private set; }
    public DodgeState dodgeState { get; private set; }
    public ChaseState chaseState { get; private set; }
    public PatrolState patrolState { get; private set; }
    public ObserveState observeState { get; private set; }
    public RepositionState repositionState { get; private set; }


    public override void Start()
    {
        base.Start();

        patrolState = new PatrolState(this, stateMachine, "move");
        observeState = new ObserveState(this, stateMachine, "idle");
        suctionState = new SuctionState(this, stateMachine, "idle");
        stunnedState = new StunnedState(this, stateMachine, "stun");
        
        deadState = new DeadState(this, stateMachine, "dead");

        if (entityData.meleeAttackPrefab != null) meleeState = new MeleeState(this, stateMachine, "melee", entityData.meleeAttackPrefab);
        else meleeState = null;

        if (entityData.rangedAttackPrefab != null)
        {
            rangedState = new RangedState(this, stateMachine, "ranged", entityData.rangedAttackPrefab);
            repositionState = new RepositionState(this, stateMachine, "move");
        }
        else rangedState = null;

        if (entityData.canSelfDestruct) selfDestructState = new SelfDestructState(this, stateMachine, "destruct");
        else selfDestructState = null;

        if(entityData.canDodge) dodgeState = new DodgeState(this, stateMachine, "dodge");
        else dodgeState = null;

        chaseState = new ChaseState(this, stateMachine, "chase", meleeState, selfDestructState, rangedState, repositionState, dodgeState);

        patrolState.SetTransitions(observeState);
        observeState.SetTransitions(patrolState);

        stateMachine.Initialize(observeState);
    }
    public override void ApplyPull(Vector2 center, float speed, float duration)
    {
        bool isMeleeOnlyDodge = entityData.meleeAttackPrefab != null && 
                                entityData.rangedAttackPrefab == null && 
                                !entityData.canSelfDestruct && 
                                entityData.canDodge;

        if (isMeleeOnlyDodge && CanDodge() && stateMachine.currentState != dodgeState)
        {
            Vector2 dirTowardsPlayer = (Player.Instance.transform.position - transform.position).normalized;
            dodgeState.SetDodgeDirection(dirTowardsPlayer);
            stateMachine.ChangeState(dodgeState);
            return;
        }
        bool canBePulled = stateMachine.currentState != suctionState && 
                           stateMachine.currentState != stunnedState && 
                           stateMachine.currentState != deadState;
        if (stateMachine.currentState != suctionState && canBePulled)
        {
            suctionState.SetupPull(center, speed, duration);
            stateMachine.ChangeState(suctionState);
        }

    }
    public override void TakeDamage(AttackDetails attackDetails)
    {
        base.TakeDamage(attackDetails); 

        bool isHpBelowThreshold = currentHealth <= (entityData.maxHealth * entityData.selfDestructHpThreshold);
        bool isDead = currentHealth <= 0;

        if (isDead)
        {
            if (stateMachine.currentState != deadState)
            {
                stateMachine.ChangeState(deadState);
            }
            return;
        }

        if (entityData.canSelfDestruct && isHpBelowThreshold)
        {
            isKamikazeMode = true; 
            
            if (stateMachine.currentState != chaseState)
            {
                stateMachine.ChangeState(chaseState);
            }
            return;
        }

        if (!isAggroed)
        {
            // 1. Bị trúng đòn khi đang ngây thơ đi tuần -> Cảnh giác quét 4 hướng
            if (stateMachine.currentState == patrolState)
            {
                Debug.Log("<color=yellow>Bị đánh trúng khi đang đi tuần -> Dừng lại quét 4 hướng!</color>");
                observeState.SetAlertMode(true); 
                stateMachine.ChangeState(observeState);
            }
            // 2. Bị trúng đòn khi đang cảnh giác (Observe) -> Điên tiết báo động luôn
            else if (stateMachine.currentState == observeState)
            {
                Debug.Log("<color=red>Bị đánh trúng khi đang cảnh giác -> BÁO ĐỘNG AGGRO!</color>");
                ForceAggro();
            }
        }
    }
    public override void ApplyStun(float duration, GameObject stunVfxPrefab)
    {
        if (CanBeStunned())
        {
            stunnedState.SetupStun(duration, stunVfxPrefab);
            stateMachine.ChangeState(stunnedState);
        }
    }
    public override void ReturnToDefaultState()
    {
        if (isAggroed)
        {
            stateMachine.ChangeState(chaseState);
        }
        else
        {
            stateMachine.ChangeState(patrolState);
        }
    }
    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
    }
}
