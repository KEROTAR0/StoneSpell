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
        
        // Lấy Khuôn đúc Data từ kiến trúc mới
        currentSpellData = currentSpellPrefab.GetComponent<SpellController>().baseSpellData;
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // NẾU BỊ KHỐNG CHẾ -> BỊ KHÓA LUÔN, KHÔNG CHO DASH ĐỂ CANCEL NỮA
        if (player.CC != null && player.CC.isStunned) return;

        // 1. CANCEL BẰNG DASH (Khi an toàn)
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
                
                CastSpell();
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
            GameObject spellObj = ObjectPoolManager.Instance.GetFromPool(currentSpellPrefab, spawnPosition, Quaternion.identity);
            
            if (spellObj.TryGetComponent(out SpellController spellScript))
            {
                // Dùng GameLayers đã tối ưu
                spellScript.Initialize(shootDirection);
            }   
        }
    }
}