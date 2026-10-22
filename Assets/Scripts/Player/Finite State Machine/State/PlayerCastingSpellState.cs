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

        // 1. TÍNH TOÁN SỐ LƯỢNG (Tăng 1 nửa nếu thỏa mãn điều kiện)
        int count = currentSpellData.spawnCount > 0 ? currentSpellData.spawnCount : 1;
        
        if ((currentSpellData.isSelfTargeted && currentSpellData.isProjectile) || 
            (currentSpellData.isSpawnAtMouse && currentSpellData.isProjectile))
        {
            count = Mathf.CeilToInt(count * 1.5f); // Tăng 1.5 lần (Làm tròn lên)
        }

        // 2. TÍNH TOÁN VỊ TRÍ GỐC
        Vector3 baseSpawnPos = player.attackPosition.position;
        if (currentSpellData.isSpawnAtMouse)
        {
            float distanceToMouse = Vector2.Distance(player.attackPosition.position, mouseWorldPos);
            baseSpawnPos = distanceToMouse > currentSpellData.maxCastRange 
                ? player.attackPosition.position + (Vector3)(shootDirection * currentSpellData.maxCastRange) 
                : mouseWorldPos;
        }

        // 3. SINH ĐẠN VÀ RẢI RÁC
        if (ObjectPoolManager.Instance != null)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 finalSpawnPos = baseSpawnPos;
                Vector2 castDir = currentSpellData.isSpawnAtMouse ? Vector2.zero : shootDirection;

                // RẢI MÌN BÒ XUNG QUANH CHUỘT
                if (currentSpellData.isSpawnAtMouse && count > 1 && !currentSpellData.isProjectile)
                {
                    Vector2 randomOffset = Random.insideUnitCircle * currentSpellData.orbitRadius;
                    finalSpawnPos += (Vector3)randomOffset;
                }
                
                // ==========================================
                // XỬ LÝ ĐỘI HÌNH BẮN ĐẠN (SPREAD / LINE)
                // ==========================================
                if (!currentSpellData.isSpawnAtMouse && currentSpellData.isProjectile && count > 1)
                {
                    if (currentSpellData.multiShotPattern == MultiShotPattern.Spread)
                    {
                        // Hình quạt: Chia đều góc
                        float startAngle = -currentSpellData.multiShotAngle / 2f;
                        float step = count > 1 ? currentSpellData.multiShotAngle / (count - 1) : 0;
                        float angle = startAngle + (step * i);
                        castDir = Quaternion.Euler(0, 0, angle) * shootDirection;
                    }
                    else if (currentSpellData.multiShotPattern == MultiShotPattern.TrailingLine)
                    {
                        // Thẳng hàng nối đuôi: Kéo lùi vị trí đẻ đạn về phía sau
                        finalSpawnPos -= (Vector3)(shootDirection * (currentSpellData.multiShotSpacing * i));
                    }
                    else if (currentSpellData.multiShotPattern == MultiShotPattern.ParallelLine)
                    {
                        // Song song dàn ngang: Dịch chuyển sang hai bên hướng bắn
                        Vector2 right = Vector2.Perpendicular(shootDirection) * -1f;
                        float offset = (i - (count - 1) / 2f) * currentSpellData.multiShotSpacing;
                        finalSpawnPos += (Vector3)(right * offset);
                    }
                }

                GameObject spellObj = ObjectPoolManager.Instance.GetFromPool(currentSpellPrefab, finalSpawnPos, Quaternion.identity);
                
                if (spellObj.TryGetComponent(out SpellController spellScript))
                {
                    // Truyền shootDirection (Hướng ngắm thẳng) làm rawDir để đạn Gatling dùng
                    spellScript.Initialize(castDir, i, count, shootDirection);
                }   
            }
        }
    }
}