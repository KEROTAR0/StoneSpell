using UnityEngine;

public class PlayerCastingSpellState : PlayerAttackState
{
    private GameObject currentSpellPrefab;
    private D_Spell currentSpellData;
    private int slotIndex;
    private bool hasCast;
    private float manaDrainedThisCast; 

    public PlayerCastingSpellState(PlayerController player, PlayerStateMachine stateMachine, D_Player playerData, int animBoolHash) 
        : base(player, stateMachine, playerData, animBoolHash)
    {
    }

    public override void Enter()
    {
        base.Enter();
        hasCast = false;
        manaDrainedThisCast = 0f;
        
        slotIndex = player.currentSpellSlot;
        currentSpellPrefab = player.SpellController.equippedSpellPrefabs[slotIndex];
        currentSpellData = currentSpellPrefab.GetComponent<SpellController>().baseSpellData;

        // ĐÃ THÊM: NẾU ĐANG BẬT TOGGLE -> TẮT NGAY LẬP TỨC VÀ KẾT THÚC STATE, KHÔNG CẦN CHỜ CASTING TIME
        if (currentSpellData.isManualToggle && player.SpellController.toggleStates[slotIndex])
        {
            if (currentSpellData.isSelfTargeted) player.SpellController.ToggleSpell(slotIndex, player.gameObject);
            else player.SpellController.ToggleSpell(slotIndex, null);
            
            hasCast = true;
            FinishAttack();
            return;
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (player.CC != null && player.CC.isStunned) return;

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

                // GỌI COMPONENT MANA: Truyền số âm để trừ Mana
                player.Mana.RestoreMana(-manaDrainPerFrame);
                manaDrainedThisCast += manaDrainPerFrame;

                // KIỂM TRA TỪ COMPONENT MANA
                if (player.Mana.currentMana <= 0)
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
                    player.Mana.RestoreMana(-leftoverMana);
                }

                if (currentSpellData.isManualToggle)
                {
                    if (currentSpellData.isSelfTargeted)
                    {
                        player.SpellController.ToggleSpell(slotIndex, player.gameObject);
                    }
                    else 
                    {
                        // ĐÃ SỬA: Sinh Cột sét ra sân
                        player.SpellController.ToggleSpell(slotIndex, null);
                        CastSpell(); 
                    }
                }
                else
                {
                    CastSpell(); // Phép bắn bình thường
                }
                
                hasCast = true;
                FinishAttack();
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
            spawnPosition = distanceToMouse > currentSpellData.maxCastRange 
                ? player.attackPosition.position + (Vector3)(shootDirection * currentSpellData.maxCastRange) 
                : mouseWorldPos;
        }

        if (ObjectPoolManager.Instance != null)
        {
            int count = currentSpellData.spawnCount > 0 ? currentSpellData.spawnCount : 1;
            for (int i = 0; i < count; i++)
            {
                GameObject spellObj = ObjectPoolManager.Instance.GetFromPool(currentSpellPrefab, spawnPosition, Quaternion.identity);
                
                if (spellObj.TryGetComponent(out SpellController spellScript))
                {
                    Vector2 castDir = currentSpellData.isSpawnAtMouse ? Vector2.zero : shootDirection;
                    spellScript.Initialize(castDir, i, count);
                }   
            }  
        }
    }
}