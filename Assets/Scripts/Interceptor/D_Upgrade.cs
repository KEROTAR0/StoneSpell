using System.Collections.Generic;
using UnityEngine;

// THÊM ENUM BỘ LỌC ĐỂ DỄ DÀNG QUẢN LÝ
[System.Flags]
public enum SpellCategoryFilter
{
    None = 0,
    Projectile = 1 << 0,
    Toggle = 1 << 1,
    Aura = 1 << 2,
    Laser = 1 << 3,
    All = ~0
}
public enum UpgradeCategory
{
    PlayerStat, // Dành cho người chơi (Máu, Mana, Tốc độ chạy...)
    SpellStat   // Dành cho phép thuật (Sát thương, Xuyên thấu, Kích thước...)
}
[CreateAssetMenu(fileName = "New Upgrade", menuName = "Data/Upgrade")]
public class D_Upgrade : ScriptableObject
{
    public UpgradeCategory upgradeCategory = UpgradeCategory.SpellStat;
    [Header("Giới hạn Nâng cấp")]
    public bool isUnique = true;
    public int maxUpgrade = 3;
    public int upgradeCost = 1;

    [Header("UI Info")]
    public string upgradeName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("--- BỘ LỌC PHÉP THUẬT (SPELL FILTERS) ---")]
    [Tooltip("Kéo phép vào đây nếu thẻ này CHỈ dành cho 1 phép duy nhất")]
    public D_Spell targetSpecificSpell; 
    
    [Tooltip("Áp dụng cho các loại phép nào? (Có thể chọn nhiều cái cùng lúc)")]
    public SpellCategoryFilter categoryFilter = SpellCategoryFilter.All;

    [Header("--- NÂNG CẤP CHỈ SỐ PLAYER (PLAYER STATS) ---")]
    public int flatHealthBonus = 0;
    public int flatManaBonus = 0;
    public float manaRegenBonus = 0f;
    public float moveSpeedBonus = 0f;

    [Header("--- NÂNG CẤP CHỈ SỐ PHÉP (SPELL STATS) ---")]
    [Tooltip("Tăng kích thước tổng thể của phép (VD: 0.5 = to thêm 50%)")]
    public float sizePercentBonus = 0f;
    public float flatDamageBonus = 0f;
    public float percentDamageBonus = 0f; 
    
    [Header("Chỉ số Tiêu hao & Thời gian")]
    public float cooldownReduction = 0f; // VD: 0.1 = Giảm 10% CD
    public float manaCostReduction = 0f; // VD: 0.2 = Giảm 20% Mana
    public float castTimeReduction = 0f;
    public float lifeTimeBonus = 0f;

    [Header("Chỉ số Vật lý & Di chuyển")]
    public float speedBonus = 0f;
    public float accelerationBonus = 0f;
    public float rotationSpeedBonus = 0f;
    public float inaccuracyBonus = 0f;

    [Header("Chỉ số Đặc thù")]
    public int bonusSpawnCount = 0;
    public int bonusBounces = 0;
    public int bonusBlocks = 0;
    public float laserLengthBonus = 0f;
    public float laserWidthBonus = 0f;
    public float explosionRadiusBonus = 0f;

    [Header("Đẩy Lùi / Hút (Vacuum)")]
    public float pullForce;
    public float knockbackForce;
    public float pushPullRadius;

    [Header("Xuyên Thấu & Vỡ Vụn (Fragmentation)")]
    public int maxPierce;
    public int fragmentCount;
    public float fragmentDamageMultiplier;

    [Header("--- HIỆU ỨNG VÀ COMPONENT ---")]
    public List<EffectData> additionalHitEffects = new List<EffectData>();
    public List<string> behaviorComponentNames = new List<string>();
}