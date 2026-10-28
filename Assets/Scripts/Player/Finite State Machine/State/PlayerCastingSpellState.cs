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
        currentSpellPrefab = player.PSpellController.equippedSpellPrefabs[slotIndex];
        currentSpellData = currentSpellPrefab.GetComponent<SpellController>().baseSpellData;

        // CHỐT CHẶN: Nếu State bị gọi khi Toggle ĐANG BẬT -> Tắt đi và thoát ngay
        if (currentSpellData.isManualToggle && player.PSpellController.toggleStates[slotIndex])
        {
            TurnOffToggle();
            hasCast = true;
            FinishAttack();
            stateMachine.ChangeState(player.IdleState);
            return;
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // 1. NẾU BỊ CHOÁNG -> TẮT CHIÊU
        if (player.CC != null && player.CC.isStunned)
        {
            if (hasCast && currentSpellData.isManualToggle && player.PSpellController.toggleStates[slotIndex])
            {
                TurnOffToggle();
            }
            return;
        }

        // 2. HỦY CHIÊU HOẶC NGẮT TOGGLE BẰNG LƯỚT (DASH)
        if (player.InputHandler.DashInput && player.CanDash())
        {
            if (hasCast && currentSpellData.isManualToggle && player.PSpellController.toggleStates[slotIndex])
            {
                TurnOffToggle();
            }
            player.InputHandler.UsedDashInput(); 
            stateMachine.ChangeState(player.DashState); 
            return; 
        }

        // ========================================================
        // 3. XỬ LÝ KHI TOGGLE ĐANG HOẠT ĐỘNG (ĐỨNG IM VẬN CÔNG)
        // ========================================================
        if (hasCast && currentSpellData.isManualToggle)
        {
            // A. Tự động thoát nếu hết Mana (Toggle đã bị PlayerSpellController tắt)
            if (!player.PSpellController.toggleStates[slotIndex])
            {
                stateMachine.ChangeState(player.IdleState);
                return;
            }

            // B. Chờ người chơi bấm phím kỹ năng MỘT LẦN NỮA để tắt
            bool isInputPressedAgain = false;
            if (slotIndex == 0 && player.InputHandler.Spell1Input) isInputPressedAgain = true;
            else if (slotIndex == 1 && player.InputHandler.Spell2Input) isInputPressedAgain = true;
            else if (slotIndex == 2 && player.InputHandler.Spell3Input) isInputPressedAgain = true;
            else if (slotIndex == 3 && player.InputHandler.Spell4Input) isInputPressedAgain = true;
            else if (slotIndex == 4 && player.InputHandler.Spell5Input) isInputPressedAgain = true;

            if (isInputPressedAgain)
            {
                player.InputHandler.UseSpellInputByIndex(slotIndex); // Xóa bộ nhớ phím
                TurnOffToggle();
                stateMachine.ChangeState(player.IdleState);
                return;
            }
            
            // C. NẾU CHƯA BẤM GÌ -> GIỮ KẸT Ở STATE NÀY ĐỂ NHÂN VẬT ĐỨNG IM
            return; 
        }

        // ========================================================
        // 4. QUÁ TRÌNH GỒNG (CASTING TIME) VÀ BẮN
        // ========================================================
        if (!hasCast)
        {
            // Xử lý trừ Mana khi đang gồng
            if (currentSpellData.castingTime > 0)
            {
                float manaDrainPerFrame = (currentSpellData.manaCost / currentSpellData.castingTime) * Time.deltaTime;
                if (manaDrainedThisCast + manaDrainPerFrame > currentSpellData.manaCost)
                {
                    manaDrainPerFrame = currentSpellData.manaCost - manaDrainedThisCast;
                }

                player.Mana.RestoreMana(-manaDrainPerFrame);
                manaDrainedThisCast += manaDrainPerFrame;

                if (player.Mana.currentMana <= 0)
                {
                    stateMachine.ChangeState(player.IdleState);
                    return;
                }
            }

            // Đã gồng xong
            if (Time.time >= (startTime + currentSpellData.castingTime))
            {
                // Bù trừ phần Mana lẻ còn thiếu
                float leftoverMana = currentSpellData.manaCost - manaDrainedThisCast;
                if (leftoverMana > 0) player.Mana.RestoreMana(-leftoverMana);

                // NẾU LÀ PHÉP TOGGLE: Bật chiêu và GIỮ LẠI State
                if (currentSpellData.isManualToggle)
                {
                    if (currentSpellData.isSelfTargeted || currentSpellData.hasAura)
                        player.PSpellController.ToggleSpell(slotIndex, player.gameObject);
                    else 
                    {
                        player.PSpellController.ToggleSpell(slotIndex, null);
                        CastSpell(); 
                    }
                    
                    hasCast = true; 
                    // BỎ QUA ChangeState Ở ĐÂY ĐỂ ĐỨNG IM CẦM CHIÊU
                }
                else
                {
                    // NẾU LÀ PHÉP BẮN 1 LẦN: Bắn đạn và THOÁT State
                    CastSpell(); 
                    hasCast = true;
                    FinishAttack();

                    if (player.InputHandler.movementInput != Vector2.zero)
                        stateMachine.ChangeState(player.MoveState);
                    else
                        stateMachine.ChangeState(player.IdleState);
                }
            }
        }
    }

    // Hàm tiện ích để gọi tắt Toggle cho gọn code
    private void TurnOffToggle()
    {
        if (currentSpellData.isSelfTargeted || currentSpellData.hasAura) 
            player.PSpellController.ToggleSpell(slotIndex, player.gameObject);
        else 
            player.PSpellController.ToggleSpell(slotIndex, null);
    }

    private void CastSpell()
    {
        player.PSpellController.TriggerCooldown(slotIndex);

        int count = currentSpellData.spawnCount > 0 ? currentSpellData.spawnCount : 1;
        bool isSingleInstanceSpell = (currentSpellData.isSelfTargeted && !currentSpellData.isProjectile && !currentSpellData.isLaser) 
                                     || (currentSpellData.isManualToggle && currentSpellData.hasAura);

        if (isSingleInstanceSpell)
        {
            count = 1;
        }

        Vector3 baseSpawnPos = player.attackPosition.position;
        if (currentSpellData.isSpawnAtMouse)
        {
            float distanceToMouse = Vector2.Distance(player.attackPosition.position, mouseWorldPos);
            baseSpawnPos = distanceToMouse > currentSpellData.maxCastRange 
                ? player.attackPosition.position + (Vector3)(shootDirection * currentSpellData.maxCastRange) 
                : mouseWorldPos;
        }

        if (ObjectPoolManager.Instance != null)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 finalSpawnPos = baseSpawnPos;
                Vector2 castDir = currentSpellData.isSpawnAtMouse ? Vector2.zero : shootDirection;

                if (currentSpellData.isSpawnAtMouse && count > 1 && !currentSpellData.isProjectile)
                {
                    Vector2 randomOffset = Random.insideUnitCircle * currentSpellData.orbitRadius;
                    finalSpawnPos += (Vector3)randomOffset;
                }
                
                if (!currentSpellData.isSpawnAtMouse && currentSpellData.isProjectile && count > 1)
                {
                    if (currentSpellData.multiShotPattern == MultiShotPattern.Spread)
                    {
                        float startAngle = -currentSpellData.multiShotAngle / 2f;
                        float step = count > 1 ? currentSpellData.multiShotAngle / (count - 1) : 0;
                        float angle = startAngle + (step * i);
                        castDir = Quaternion.Euler(0, 0, angle) * shootDirection;
                    }
                    else if (currentSpellData.multiShotPattern == MultiShotPattern.TrailingLine)
                    {
                        finalSpawnPos -= (Vector3)(shootDirection * (currentSpellData.multiShotSpacing * i));
                    }
                    else if (currentSpellData.multiShotPattern == MultiShotPattern.ParallelLine)
                    {
                        Vector2 right = Vector2.Perpendicular(shootDirection) * -1f;
                        float offset = (i - (count - 1) / 2f) * currentSpellData.multiShotSpacing;
                        finalSpawnPos += (Vector3)(right * offset);
                    }
                }

                GameObject spellObj = ObjectPoolManager.Instance.GetFromPool(currentSpellPrefab, finalSpawnPos, Quaternion.identity);
                
                if (spellObj.TryGetComponent(out SpellController spellScript))
                {
                    spellScript.Initialize(castDir, i, count, shootDirection);
                }   
            }
        }
    }
}