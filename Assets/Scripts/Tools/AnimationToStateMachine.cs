using UnityEngine;

public class AnimationToStateMachine : MonoBehaviour
{
    public EnemyController entity;
    private void TriggerAttack()
    {
        entity.stateMachine.currentState.TriggerAttack();
    }
    private void FinishAttack()
    {
        entity.stateMachine.currentState.FinishAttack();
    }
    private void DisableThisEntity()
    {
        entity.Die();
    }

}
