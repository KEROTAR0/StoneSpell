using UnityEngine;

[System.Serializable]
public class EffectData
{
    public EffectType type;
    [Range(0f, 1f)]
    public float procChance = 1;
    public float duration;   // Thời gian tồn tại (Để 0 nếu là hiệu ứng nổ tức thì)
    public float value;      // Lượng sát thương, tốc độ hút, lượng mana...
    
    [Tooltip("Khoảng cách giữa các lần giật. Ví dụ: 1 = mỗi giây giật 1 lần. Để 0 nếu hiệu ứng tác dụng 1 lần duy nhất.")]
    public float tickRate;   
    public bool isBuff;
    public GameObject vfxPrefab;
}