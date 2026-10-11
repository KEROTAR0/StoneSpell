using UnityEngine;

public class PlayerCastingSpellState : PlayerAttackState
{
    private GameObject currentSpellPrefab;
    private D_Spell currentSpellData;
    private int slotIndex;
    private bool hasCast;
    private float manaDrainedThisCast; 

    public PlayerCastingSpellState(Player player, PlayerStateMachine stateMachine, D_Player playerData, string animBoolName) 
        : base(player, stateMachine, playerData, animBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        hasCast = false;
        manaDrainedThisCast = 0f;
        
        slotIndex = player.currentSpellSlot;
        currentSpellPrefab = player.SpellController.equippedSpellPrefabs[slotIndex];
        currentSpellData = currentSpellPrefab.GetComponent<SpellBase>().spellData;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 1. CANCEL BẰNG DASH
        if (player.InputHandler.DashInput && player.CanDash())
        {
            player.InputHandler.UsedDashInput(); 
            stateMachine.ChangeState(player.DashState); 
            return; 
        }

        if (!hasCast)
        {
            if (currentSpellData.castingTime > 0)
            {
                float manaDrainPerFrame = (currentSpellData.manaCost / currentSpellData.castingTime) * Time.deltaTime;
                
                if (manaDrainedThisCast + manaDrainPerFrame > currentSpellData.manaCost)
                {
                    manaDrainPerFrame = currentSpellData.manaCost - manaDrainedThisCast;
                }

                player.AddMana(-manaDrainPerFrame);
                manaDrainedThisCast += manaDrainPerFrame;

                if (player.currentMana <= 0)
                {
                    stateMachine.ChangeState(player.IdleState);
                    return;
                }
            }

            if (Time.time >= (startTime + currentSpellData.castingTime))
            {
                float leftoverMana = currentSpellData.manaCost - manaDrainedThisCast;
                if (leftoverMana > 0)
                {
                    player.AddMana(-leftoverMana);
                }
                
                CastSpell();
                hasCast = true;
                FinishAttack();
            }
        }

        if (isAnimationFinished)
        {
            if (input.sqrMagnitude > 0.01f)
            {
                stateMachine.ChangeState(player.MoveState);
            }
            else
            {
                stateMachine.ChangeState(player.IdleState);
            }
        }
    }

    private void CastSpell()
    {
        player.SpellController.TriggerCooldown(slotIndex);

        Vector3 spawnPosition = player.attackPosition.position;
        if (currentSpellData.isSpawnAtMouse)
        {
            float distanceToMouse = Vector2.Distance(player.attackPosition.position, mouseWorldPos);
            
            if (distanceToMouse > currentSpellData.maxCastRange)
            {
                spawnPosition = player.attackPosition.position + (Vector3)(shootDirection * currentSpellData.maxCastRange);
            }
            else
            {
                spawnPosition = mouseWorldPos;
            }
        }

        GameObject spellObj = ObjectPoolManager.Instance.GetFromPool(currentSpellPrefab, spawnPosition, Quaternion.identity);
        SpellBase spellScript = spellObj.GetComponent<SpellBase>();

        if (spellScript != null)
        {
            LayerMask enemyLayer = LayerMask.GetMask("Enemy", "Damageable");
            LayerMask wallLayer = LayerMask.GetMask("Wall");
            LayerMask highObsLayer = LayerMask.GetMask("HighObstacle");
            LayerMask enemySpellLayer = LayerMask.GetMask("EnemySpell");

            spellScript.Initialize(enemyLayer, wallLayer, highObsLayer, enemySpellLayer, shootDirection);
        }   
    }
}