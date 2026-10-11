using UnityEngine;

public class ChaseState : State
{
    private State meleeState;
    private State selfDestructState;
    private State rangedState;
    private DodgeState dodgeState;
    private State repositionState;

    public ChaseState(Entity entity, FiniteStateMachine stateMachine, int animBoolHash, State meleeState, State selfDestructState, State rangedState, State repositionState, DodgeState dodgeState) 
        : base(entity, stateMachine, animBoolHash)
    {
        this.meleeState = meleeState;
        this.selfDestructState = selfDestructState;
        this.rangedState = rangedState;
        this.repositionState = repositionState;
        this.dodgeState = dodgeState;
    }

    private Vector2 GetRandomEvasiveDirection(Vector2 baseAwayDir)
    {
        int rand = Random.Range(0, 3);
        float angle = rand == 1 ? 45f : (rand == 2 ? -45f : 0f);
        return Quaternion.Euler(0, 0, angle) * baseAwayDir;
    }

    public override void Enter()
    {
        base.Enter();

        Debug.Log("<color=green>>>> ĐÃ VÀO CHASE STATE!</color>");

        if (entity.aiPath != null && Player.Instance != null)
        {
            entity.aiPath.SetPath(null); 
            entity.aiPath.canMove = true;
            entity.aiPath.isStopped = false;
            entity.aiPath.destination = Player.Instance.transform.position;
            entity.aiPath.SearchPath();
        }
    }

public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 1. KAMIKAZE (Tự sát)
        if (entity.isKamikazeMode && selfDestructState != null)
        {
            if (entity.CheckPlayerInShortRangeAction())
                stateMachine.ChangeState(selfDestructState); 
            else
                entity.MoveTowardsPlayer(); 
            return;
        }

        // 2. NÉ TRÁNH (Dodge)
        if (dodgeState != null && entity.CanDodge())
        {
            Transform incomingSpell = entity.GetNearestSpell();
            if (incomingSpell != null)
            {
                Vector2 dirAwayFromSpell = (entity.transform.position - incomingSpell.position).normalized;
                dodgeState.SetDodgeDirection(GetRandomEvasiveDirection(dirAwayFromSpell));
                stateMachine.ChangeState(dodgeState); 
                return;
            }
            else if (entity.CheckPlayerInShortRangeAction())
            {
                Vector2 dirAwayFromPlayer = (entity.transform.position - Player.Instance.transform.position).normalized;
                dodgeState.SetDodgeDirection(GetRandomEvasiveDirection(dirAwayFromPlayer));
                stateMachine.ChangeState(dodgeState);
                return;
            }
        }

        // 3. CẬN CHIẾN (Melee)
        if (meleeState != null && entity.CheckPlayerInShortRangeAction() && entity.CanMelee())
        {
            stateMachine.ChangeState(meleeState);
            return;
        }

        // 4. ĐÁNH XA (Ranged)
        if (rangedState != null && entity.CanRanged())
        {
            stateMachine.ChangeState(rangedState);
            return;
        }

        // 5. CÁC HÀNH ĐỘNG DỰ PHÒNG KHI HẾT CHIÊU
        if (meleeState == null && rangedState != null)
        {
            // Bắn xong chiêu đang hồi -> Đổi vị trí ngay
            if (repositionState != null) 
            {
                stateMachine.ChangeState(repositionState);
            }
            return; 
        }

        // 6. MẶC ĐỊNH: ĐUỔI THEO PLAYER (Chase)
        // Chạy tới đây nghĩa là Player đang ở xa, cần rút ngắn khoảng cách.
        entity.MoveTowardsPlayer();
    }


    public override void Exit()
    {
        base.Exit();
        entity.StopMovement();
    }
}