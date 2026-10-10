using UnityEngine;

[CreateAssetMenu(fileName = "New Mana On Hit Effect", menuName = "Data/Spell Data/Effects/Mana on Hit Effect")]
public class ManaOnHitBuffEffect : SpellEffect
{
    [Tooltip("Cần cò isSelfTargeted, số dương là Buff (Hút Mana), Số âm là Debuff (Đánh bị mất Mana)")]
    public float procChance = 30f; 
    public float manaRecovered = 2f; 
    
    public override void ApplyEffect(GameObject target, Vector2 sourcePosition)
    {
        Player player = target.GetComponent<Player>();
        if (player != null)
        {
            player.manaOnHitChance += procChance;
            player.manaOnHitAmount += manaRecovered;
        }
    }

    public override void RemoveEffect(GameObject target)
    {
        Player player = target.GetComponent<Player>();
        if (player != null)
        {
            player.manaOnHitChance -= procChance;
            player.manaOnHitAmount -= manaRecovered;
        }
    }
}