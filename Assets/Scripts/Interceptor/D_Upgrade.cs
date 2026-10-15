using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Upgrade", menuName = "Data/Upgrade")]
public class D_Upgrade : ScriptableObject
{
    [Header("UI Info")]
    public string upgradeName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Player Stats Modifiers")]
    public int flatHealthBonus = 0;
    public int flatManaBonus = 0;
    public float moveSpeedBonus = 0f;

    [Header("Spell Stats Modifiers (Cộng thẳng)")]
    public float flatDamageBonus = 0f;
    public float percentDamageBonus = 0f; // VD: 0.2 = Tăng 20% sát thương
    public float speedBonus = 0f;
    public int bonusBounces = 0;
    public int bonusBlocks = 0;
    public float inaccuracy = 0f;

    [Header("Hit Effects (Gắn thêm Hiệu ứng)")]
    [Tooltip("Thêm Độc, Làm Chậm, v.v... vào mọi đòn đánh")]
    public List<EffectData> additionalHitEffects = new List<EffectData>();

    [Header("Behaviors (Gắn thêm Component)")]
    [Tooltip("Nhập TÊN SCRIPT (ví dụ: 'SpellSplitOnHit', 'FireTrail') để tự động đính vào đạn")]
    public List<string> behaviorComponentNames = new List<string>();
}