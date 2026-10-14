using UnityEngine;

public class DetonateManaState : AttackState
{
    private ManaShard targetMana;

    public DetonateManaState(EnemyController entity, FiniteStateMachine stateMachine, int animBoolHash) 
        : base(entity, stateMachine, animBoolHash)
    {
    }

    // Nhận mục tiêu từ SeekManaState truyền sang
    public void SetTargetMana(ManaShard mana)
    {
        this.targetMana = mana;
    }

    public override void Enter()
    {
        base.Enter();
        entity.StopMovement(); // Phanh gấp lại để chuẩn bị đạp nổ
    }

    // HÀM NÀY ĐƯỢC GỌI BỞI ANIMATION EVENT (thông qua AnimationToStateMachine)
    public override void TriggerAttack()
    {
        base.TriggerAttack();
        
        if (targetMana != null)
        {
            targetMana.Detonate();
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
        if (isAnimationFinished)
        {
            entity.ReturnToDefaultState();
        }
    }
}