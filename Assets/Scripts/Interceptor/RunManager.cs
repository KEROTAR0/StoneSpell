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

    // -------------------------------------------------------------
    // HÀM 1: GỌI KHI NHẶT NÂNG CẤP (Level Up / Mở Rương)
    // -------------------------------------------------------------
    public void AddUpgrade(D_Upgrade newUpgrade)
    {
        activeUpgrades.Add(newUpgrade);

        // Nâng cấp trực tiếp vào chỉ số cơ thể Player (Máu, Mana, Tốc chạy)
        if (PlayerController.Instance != null)
        {
            if (newUpgrade.flatHealthBonus > 0 && PlayerController.Instance.Health != null)
                PlayerController.Instance.Health.IncreaseMaxHealth(newUpgrade.flatHealthBonus);

            if (newUpgrade.flatManaBonus > 0 && PlayerController.Instance.Mana != null)
                PlayerController.Instance.Mana.IncreaseMaxMana(newUpgrade.flatManaBonus);

            // (Có thể thêm code tăng MoveSpeed tại đây nếu playerData hỗ trợ)
        }
    }

    // -------------------------------------------------------------
    // HÀM 2: TRẠM KIỂM DUYỆT - GỌI MỖI KHI BẮN ĐẠN/DÙNG PHÉP
    // -------------------------------------------------------------
    public void InterceptSpell(SpellController spell)
    {
        if (activeUpgrades.Count == 0) return;

        foreach (var upgrade in activeUpgrades)
        {
            // 1. Áp dụng Chỉ số Động (Stats cơ bản)
            spell.currentStats.moveSpeed += upgrade.speedBonus;
            spell.currentStats.maxBounces += upgrade.bonusBounces;
            spell.currentStats.maxBlocks += upgrade.bonusBlocks;
            spell.currentStats.inaccuracy += upgrade.inaccuracy;

            // =========================================================
            // SỬA CHỮA: ÁP DỤNG SÁT THƯƠNG THEO CHUẨN DOT (TICKRATE = 0)
            // =========================================================
            if (spell.currentStats.activeEffects != null)
            {
                // Bước A: Tìm xem Phép này có sát thương gốc không (TickRate == 0)
                bool hasBaseDamageEffect = false;
                
                for (int i = 0; i < spell.currentStats.activeEffects.Count; i++)
                {
                    EffectData currentEffect = spell.currentStats.activeEffects[i];
                    
                    // Nếu nó là loại sát thương tức thì (DoT có tickRate = 0)
                    if (currentEffect.type == EffectType.DoT && currentEffect.tickRate <= 0)
                    {
                        hasBaseDamageEffect = true;
                        
                        // Cộng sát thương cố định (Flat)
                        currentEffect.value += upgrade.flatDamageBonus;
                        
                        // Cộng sát thương theo phần trăm (Percent) - Dựa vào Data Gốc
                        // Ta cần đối chiếu với baseSpellData để tính % chuẩn xác
                        if (upgrade.percentDamageBonus != 0) // Kiểm tra != 0 thay vì > 0
                        {
                            EffectData originalEffect = spell.baseSpellData.hitEffects.Find(e => e.type == EffectType.DoT && e.tickRate <= 0);
                            if (originalEffect.type == EffectType.DoT) // Check an toàn
                            {
                                // Ví dụ: Base = 10, percent = -0.9f -> Trừ đi 9 máu
                                currentEffect.value += (originalEffect.value * upgrade.percentDamageBonus);
                            }
                        }

                        // ÉP SÁT THƯƠNG TỐI THIỂU = 1 (Chỉ áp dụng nếu đây là đòn có sát thương lúc đầu)
                        if (currentEffect.value < 1f && spell.baseSpellData.hitEffects.Exists(e => e.type == EffectType.DoT && e.tickRate <= 0))
                        {
                            currentEffect.value = 1f;
                        }
                        
                        // Cập nhật lại vào danh sách Động
                        spell.currentStats.activeEffects[i] = currentEffect;
                    }
                }

                // Bước B: Nếu phép này ban đầu KHÔNG CÓ sát thương (Ví dụ: phép Buff, hoặc Tường đá)
                // Mà người chơi lại nhặt nâng cấp "Tăng 10 sát thương cho mọi đòn đánh", 
                // thì ta tự động tạo mới một Effect Sát thương cho nó!
                if (!hasBaseDamageEffect && (upgrade.flatDamageBonus > 0 || upgrade.percentDamageBonus > 0))
                {
                    EffectData newDamageEffect = new EffectData
                    {
                        type = EffectType.DoT,
                        tickRate = 0, // Sát thương tức thì
                        value = upgrade.flatDamageBonus // Chỉ lấy Flat, vì ban đầu không có Base để nhân %
                    };
                    spell.currentStats.activeEffects.Add(newDamageEffect);
                }
            }

            // =========================================================
            // 2. Nhồi thêm Hiệu Ứng Bất Lợi (Độc, Choáng...)
            // =========================================================
            if (upgrade.additionalHitEffects != null && upgrade.additionalHitEffects.Count > 0)
            {
                if (spell.currentStats.activeEffects == null) 
                    spell.currentStats.activeEffects = new List<EffectData>();
                
                spell.currentStats.activeEffects.AddRange(upgrade.additionalHitEffects);
            }

            // 3. Phép thuật Component (Ma thuật thực sự)
            // Tự động Add các file Script phụ (như để lại vũng lửa, bắn nảy ra 8 hướng)
            if (upgrade.behaviorComponentNames != null)
            {
                foreach (string compName in upgrade.behaviorComponentNames)
                {
                    System.Type type = System.Type.GetType(compName);
                    if (type != null && spell.GetComponent(type) == null)
                    {
                        spell.gameObject.AddComponent(type);
                    }
                    else if (type == null)
                    {
                        Debug.LogWarning($"[RunManager] Không tìm thấy script nào tên là '{compName}'. Hãy kiểm tra lại lỗi chính tả trong D_Upgrade.");
                    }
                }
            }
        }
    }
}