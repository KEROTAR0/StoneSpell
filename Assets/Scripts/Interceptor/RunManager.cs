using System.Collections.Generic;
using UnityEngine;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    [Header("Capacity System")]
    public int maxCapacity = 10;
    public int currentCapacity { get; private set; } = 0;

    [Header("Active Upgrades")]
    public List<D_Upgrade> activeUpgrades = new List<D_Upgrade>();
    

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddUpgrade(D_Upgrade newUpgrade)
    {
        activeUpgrades.Add(newUpgrade);
        currentCapacity += newUpgrade.upgradeCost;

        if (PlayerController.Instance != null)
        {
            // Tích hợp ĐẦY ĐỦ 4 chỉ số vào đây
            if (newUpgrade.flatHealthBonus > 0 && PlayerController.Instance.TryGetComponent(out HealthComponent health))
                health.IncreaseMaxHealth(newUpgrade.flatHealthBonus);

            if (newUpgrade.flatManaBonus > 0 && PlayerController.Instance.TryGetComponent(out PlayerManaComponent mana))
                mana.IncreaseMaxMana(newUpgrade.flatManaBonus);

            if (newUpgrade.manaRegenBonus > 0 && PlayerController.Instance.TryGetComponent(out PlayerManaComponent manaRegen))
                manaRegen.IncreaseManaRegen(newUpgrade.manaRegenBonus);
                
            if (newUpgrade.moveSpeedBonus > 0 && PlayerController.Instance.TryGetComponent(out CoreCCComponent cc))
                cc.moveSpeedMultiplier += newUpgrade.moveSpeedBonus; 
        }
    }
    public void RemoveUpgrade(D_Upgrade upgradeToRemove)
    {
        if (!activeUpgrades.Contains(upgradeToRemove)) return;

        activeUpgrades.Remove(upgradeToRemove);
        currentCapacity -= upgradeToRemove.upgradeCost;

        // TRỪ NGƯỢC LẠI CÁC CHỈ SỐ STAT ĐÃ CỘNG CHO PLAYER
        if (PlayerController.Instance != null)
        {
            if (upgradeToRemove.flatHealthBonus > 0 && PlayerController.Instance.TryGetComponent(out HealthComponent health))
                health.IncreaseMaxHealth(-upgradeToRemove.flatHealthBonus); // Trừ ngược máu

            if (upgradeToRemove.flatManaBonus > 0 && PlayerController.Instance.TryGetComponent(out PlayerManaComponent mana))
                mana.IncreaseMaxMana(-upgradeToRemove.flatManaBonus);

            if (upgradeToRemove.manaRegenBonus > 0 && PlayerController.Instance.TryGetComponent(out PlayerManaComponent manaRegen))
                manaRegen.IncreaseManaRegen(-upgradeToRemove.manaRegenBonus);
                
            if (upgradeToRemove.moveSpeedBonus > 0 && PlayerController.Instance.TryGetComponent(out CoreCCComponent cc))
                cc.moveSpeedMultiplier -= upgradeToRemove.moveSpeedBonus; 
        }
        
        // CÁC CHỈ SỐ CỦA PHÉP SẼ TỰ ĐỘNG ĐƯỢC RESET! 
        // Vì mỗi khi người chơi bắn đạn, đạn sẽ đọc danh sách activeUpgrades mới để tính toán.
        Debug.Log($"Đã gỡ {upgradeToRemove.upgradeName}. Cost hiện tại: {currentCapacity}/{maxCapacity}");
    }
    public void InterceptSpell(SpellController spell)
    {
        if (spell == null || spell.baseSpellData == null || spell.currentStats == null) return;
        if (activeUpgrades == null || activeUpgrades.Count == 0) return;

        float totalFlatDamage = 0f;
        float totalPercentDamage = 0f;

        foreach (var upgrade in activeUpgrades)
        {
            if (upgrade == null) continue;
            // =========================================================
            // 1. LỌC THÔNG MINH BẰNG ENUM (Gọn gàng và chính xác)
            // =========================================================
            bool passFilter = false;

            if (upgrade.targetSpecificSpell != null)
            {
                // Nếu thẻ chỉ đích danh phép này, cho qua luôn
                if (spell.baseSpellData == upgrade.targetSpecificSpell) passFilter = true;
            }
            else if (upgrade.categoryFilter == SpellCategoryFilter.All)
            {
                passFilter = true;
            }
            else
            {
                // Dùng toán tử Bitwise (AND) để check cờ
                if ((upgrade.categoryFilter & SpellCategoryFilter.Projectile) != 0 && spell.baseSpellData.isProjectile) passFilter = true;
                if ((upgrade.categoryFilter & SpellCategoryFilter.Toggle) != 0 && spell.baseSpellData.isManualToggle) passFilter = true;
                if ((upgrade.categoryFilter & SpellCategoryFilter.Aura) != 0 && spell.baseSpellData.hasAura) passFilter = true;
                if ((upgrade.categoryFilter & SpellCategoryFilter.Laser) != 0 && spell.baseSpellData.isLaser) passFilter = true;
            }

            if (!passFilter) continue;

            // =========================================================
            // 2. ÁP DỤNG CHỈ SỐ ĐỘNG BẢN THÂN
            // =========================================================
            
            // KÍCH THƯỚC (Scale vật lý)
            if (upgrade.sizePercentBonus != 0f)
            {
                spell.currentStats.currentScale += spell.currentStats.currentScale * upgrade.sizePercentBonus;
                spell.transform.localScale = spell.currentStats.currentScale;
                
                // Scale đồng bộ các vùng nổ và hào quang
                spell.currentStats.auraCheckRadius += spell.currentStats.auraCheckRadius * upgrade.sizePercentBonus;
                spell.currentStats.explosionRadius += spell.currentStats.explosionRadius * upgrade.sizePercentBonus;
                spell.currentStats.laserWidth += spell.currentStats.laserWidth * upgrade.sizePercentBonus;
            }

            // Vật lý & Di chuyển
            spell.currentStats.moveSpeed += upgrade.speedBonus;
            spell.currentStats.acceleration += upgrade.accelerationBonus;
            spell.currentStats.rotationSpeed += upgrade.rotationSpeedBonus;
            spell.currentStats.inaccuracy += upgrade.inaccuracyBonus;
            spell.currentStats.maxBounces += upgrade.bonusBounces;
            spell.currentStats.maxBlocks += upgrade.bonusBlocks;
            
            // Đặc thù
            spell.currentStats.lifeTime += upgrade.lifeTimeBonus;
            spell.currentStats.spawnCount += upgrade.bonusSpawnCount;
            spell.currentStats.laserMaxLength += upgrade.laserLengthBonus;
            spell.currentStats.laserWidth += upgrade.laserWidthBonus;
            spell.currentStats.explosionRadius += upgrade.explosionRadiusBonus;
            
            // Tiêu hao (Tính theo % giảm)
            spell.currentStats.cooldown *= (1f - upgrade.cooldownReduction);
            spell.currentStats.manaCost *= (1f - upgrade.manaCostReduction);
            spell.currentStats.castingTime *= (1f - upgrade.castTimeReduction);

            if (spell.baseSpellData.isManualToggle && spell.currentStats.isSelfTargeted && !spell.baseSpellData.isProjectile)
            {
                spell.currentStats.lifeTime = 99999f;
                if (spell.currentStats.canReflect || spell.currentStats.maxBlocks > 0)
                {
                    spell.currentStats.maxBlocks = 99999;
                }
            }
            // =========================================================
            // 3.1 GOM SÁT THƯƠNG TRƯỚC KHI XỬ LÝ
            // =========================================================
            totalFlatDamage += upgrade.flatDamageBonus;
            totalPercentDamage += upgrade.percentDamageBonus;
            // Copy Hiệu ứng bổ sung (Độc, Choáng...) cũng bằng Deep Copy
            if (upgrade.additionalHitEffects != null)
            {
                foreach(var eff in upgrade.additionalHitEffects) {
                    if (eff.procChance <= 0f)
                        spell.currentStats.activeEffects.RemoveAll(e => e.type == eff.type);
                    else
                        spell.currentStats.activeEffects.Add(JsonUtility.FromJson<EffectData>(JsonUtility.ToJson(eff)));
                }
            }

            
            // Gắn Component Phụ (Gatling, Chasing...)
            if (upgrade.behaviorComponentNames != null)
            {
                foreach (string compName in upgrade.behaviorComponentNames)
                {
                    System.Type type = System.Type.GetType(compName);
                    if (type != null && spell.GetComponent(type) == null)
                    {
                        Component newComp = spell.gameObject.AddComponent(type);
                        newComp.SendMessage("OnEnable", SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
        }
        // =========================================================
        // 3.2 ÁP DỤNG CÔNG THỨC CHUẨN LÊN DAMAGE
        // =========================================================
        if (spell.currentStats.activeEffects != null)
        {
            bool hasBaseDamageEffect = false;
            
            for (int i = 0; i < spell.currentStats.activeEffects.Count; i++)
            {
                EffectData originalEffect = spell.currentStats.activeEffects[i];
                
                // Chỉ tăng sát thương cho Effect gây Damage trực tiếp
                if (originalEffect.type == EffectType.DoT && originalEffect.tickRate <= 0)
                {
                    hasBaseDamageEffect = true;
                    
                    EffectData clonedEffect = JsonUtility.FromJson<EffectData>(JsonUtility.ToJson(originalEffect));
                    
                    // CÔNG THỨC CHUẨN: (Base + Flat) * (1 + Percent)
                    clonedEffect.value = (clonedEffect.value + totalFlatDamage) * (1f + totalPercentDamage);

                    // Đảm bảo sát thương không bao giờ tụt xuống dưới 1
                    if (clonedEffect.value < 1f) clonedEffect.value = 1f; 
                    
                    spell.currentStats.activeEffects[i] = clonedEffect;
                }
            }

            // Nếu phép vốn không có sát thương gốc (ví dụ: Phép tạo khiên), nhưng thẻ Upgrade có cho sát thương (Ví dụ: Khiên nổ gây 10 dmg)
            if (!hasBaseDamageEffect && (totalFlatDamage > 0))
            {
                // Vẫn áp dụng công thức tương tự với Base = 0
                float newDmg = totalFlatDamage * (1f + totalPercentDamage);
                if (newDmg < 1f) newDmg = 1f;
                
                spell.currentStats.activeEffects.Add(new EffectData { type = EffectType.DoT, tickRate = 0, value = newDmg });
            }
        }
    }
}