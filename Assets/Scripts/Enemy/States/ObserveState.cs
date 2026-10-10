using UnityEngine;

public class ObserveState : State
{
    private State patrolState;
    
    private int lookCount;
    private int maxLooks;
    private float lookTimer;
    private float timePerLook = 1.2f; 
    private bool isAlertMode = false;

    public ObserveState(Entity entity, FiniteStateMachine stateMachine, string animBoolName) 
        : base(entity, stateMachine, animBoolName)
    {
    }

    public void SetTransitions(State patrolState)
    {
        this.patrolState = patrolState;
    }
    public void SetAlertMode(bool alert)
    {
        isAlertMode = alert;
    }
    public override void Enter()
    {
        base.Enter();
        entity.StopMovement(); // Gọi hàm StopMovement sẵn có[cite: 15]

        lookCount = 0;
        lookTimer = 0f;

        if (isAlertMode)
        {
            maxLooks = Random.Range(4, 7);
            timePerLook = 0.6f;
        }
        else
        {
            maxLooks = Random.Range(1, 4);
            timePerLook = 1.2f;
        }
        PickNewLookDirection();
    }

    private void PickNewLookDirection()
    {
        if (isAlertMode)
        {
            Vector2 newDir = Quaternion.Euler(0, 0, 90f) * entity.facingDirection;
            entity.SetVelocity(newDir, 0f);
        }
        else
        {
            float randomAngle = Random.Range(30f, 150f) * (Random.value > 0.5f ? 1f : -1f);
            Vector2 newDir = Quaternion.Euler(0, 0, randomAngle) * entity.facingDirection;
            entity.SetVelocity(newDir, 0f);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
        if (entity.CheckPlayerInAggroRange())
        {
            /*if (entity.currentRoom != null) entity.currentRoom.TriggerRoomBattle();
            else */
            entity.ForceAggro();
            return;
        }
        
        if (entity.GetNearestSpell() != null)
        {
            Debug.Log("<color=red>Quan sát thấy đạn phép -> BÁO ĐỘNG!</color>");
            entity.ForceAggro();
            return;
        }

        lookTimer += Time.deltaTime;
        
        if (lookTimer >= timePerLook)
        {
            lookCount++;
            
            if (lookCount >= maxLooks)
            {
                stateMachine.ChangeState(patrolState); // Đảo mắt xong -> Đi tuần tiếp
            }
            else
            {
                lookTimer = 0f;
                PickNewLookDirection(); // Xoay đầu nhìn sang hướng khác
            }
        }
    }
}