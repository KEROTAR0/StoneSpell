using UnityEngine;

public class AnimationToStateMachine : MonoBehaviour
{
    public Entity entity;
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
