using UnityEngine;

[CreateAssetMenu(fileName = "New Damage Boots Effect", menuName = "Data/Spell Data/Effects/Damage Boots Effect")]
public class DamageBoostBuffEffect : SpellEffect
{
    [Tooltip("> 1 là Buff (Tăng ST), < 1 là Debuff (Giảm ST)")]
    public float damageMultiplier = 1.5f; // x1.5 sát thương
    
    public override void ApplyEffect(GameObject target, Vector2 sourcePosition)
    {
        Player player = target.GetComponent<Player>();
        if (target.CompareTag("Player") && Player.Instance != null)
            if (player != null) player.damageMultiplier *= damageMultiplier;
    }

    public override void RemoveEffect(GameObject target)
    {
        Player player = target.GetComponent<Player>();
        if (target.CompareTag("Player") && Player.Instance != null)
            if (player != null) player.damageMultiplier /= damageMultiplier;
    }
}