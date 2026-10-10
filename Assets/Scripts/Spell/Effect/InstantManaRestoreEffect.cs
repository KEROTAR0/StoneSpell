using UnityEngine;

[CreateAssetMenu(fileName = "New Instant Mana Restore", menuName = "Data/Spell Data/Effects/Instant Mana Restore")]
public class InstantManaRestoreEffect : SpellEffect
{
    [Tooltip("Tỉ lệ phần trăm hồi Mana mỗi khi viên đạn trúng địch")]
    public float procChance = 50f; 
    public float manaToRestore = 1f;

    public override void ApplyEffect(GameObject target, Vector2 sourcePosition)
    {
        // Kiểm tra tỉ lệ thành công (ví dụ: 100% là chắc chắn hồi, 30% là có lúc hồi lúc không)
        if (Random.Range(0f, 100f) <= procChance)
        {
            if (Player.Instance != null)
            {
                Player.Instance.AddMana(manaToRestore);
            }
        }
    }
}