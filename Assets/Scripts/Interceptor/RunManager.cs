using System.Collections.Generic;
using UnityEngine;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

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

    public void InterceptSpell(SpellController spell)
    {
        if (activeUpgrades.Count == 0) return;

        foreach (var upgrade in activeUpgrades)
        {
            // =========================================================
            // 1. LỌC THÔNG MINH (OR LOGIC)
            // Nếu tick nhiều ô, đạn chỉ cần thỏa mãn 1 trong các ô đó là được qua!
            // =========================================================
            bool passFilter = false;
            if (upgrade.targetSpecificSpell == null && !upgrade.targetProjectilesOnly && !upgrade.targetToggleOnly && !upgrade.targetAurasOnly)
            {
                passFilter = true; // Không tick gì cả -> Áp dụng mọi phép
            }
            else
            {
                if (upgrade.targetSpecificSpell != null && spell.baseSpellData == upgrade.targetSpecificSpell) passFilter = true;
                if (upgrade.targetProjectilesOnly && spell.baseSpellData.isProjectile) passFilter = true;
                if (upgrade.targetToggleOnly && spell.baseSpellData.isManualToggle) passFilter = true;
                if (upgrade.targetAurasOnly && spell.baseSpellData.hasAura) passFilter = true;
            }

            if (!passFilter) continue;

            // =========================================================
            // 2. CHỈ SỐ ĐỘNG BẢN THÂN
            // =========================================================
            spell.currentStats.moveSpeed += upgrade.speedBonus;
            spell.currentStats.maxBounces += upgrade.bonusBounces;
            spell.currentStats.maxBlocks += upgrade.bonusBlocks;
            spell.currentStats.inaccuracy += upgrade.inaccuracy;

            // =========================================================
            // 3. SÁT THƯƠNG: DEEP COPY BẰNG JSON (CHỐNG LỖI DATA GỐC VÀ X2)
            // XÓA HOÀN TOÀN ĐOẠN CODE BỊ LẶP TRƯỚC ĐÂY!
            // =========================================================
            if (spell.currentStats.activeEffects != null)
            {
                bool hasBaseDamageEffect = false;
                
                for (int i = 0; i < spell.currentStats.activeEffects.Count; i++)
                {
                    EffectData originalEffect = spell.currentStats.activeEffects[i];
                    
                    if (originalEffect.type == EffectType.DoT && originalEffect.tickRate <= 0)
                    {
                        hasBaseDamageEffect = true;
                        
                        // DEEP COPY: Nhân bản Effect ra để thao tác, cách ly với Data của Unity
                        EffectData clonedEffect = JsonUtility.FromJson<EffectData>(JsonUtility.ToJson(originalEffect));
                        
                        clonedEffect.value += upgrade.flatDamageBonus;
                        if (upgrade.percentDamageBonus != 0) 
                        {
                            clonedEffect.value += (originalEffect.value * upgrade.percentDamageBonus);
                        }

                        if (clonedEffect.value < 1f) clonedEffect.value = 1f; 
                        
                        // Gắn bản sao đã sửa vào danh sách
                        spell.currentStats.activeEffects[i] = clonedEffect;
                    }
                }

                if (!hasBaseDamageEffect && (upgrade.flatDamageBonus > 0 || upgrade.percentDamageBonus > 0))
                {
                    spell.currentStats.activeEffects.Add(new EffectData { type = EffectType.DoT, tickRate = 0, value = upgrade.flatDamageBonus });
                }
            }

            // Copy Hiệu ứng bổ sung (Độc, Choáng...) cũng bằng Deep Copy
            if (upgrade.additionalHitEffects != null)
            {
                foreach(var eff in upgrade.additionalHitEffects) {
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
    }
}