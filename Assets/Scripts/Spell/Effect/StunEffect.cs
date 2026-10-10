using UnityEngine;

[CreateAssetMenu(fileName = "New Stun Effect", menuName = "Data/Spell Data/Effects/Stun Effect")]
public class StunEffect : SpellEffect
{
    [Tooltip("Thời gian quái vật bị choáng (giây)")]
    public float stunDuration = 1.5f;
    [Tooltip("Xác suất gây choáng (0 - 100%)")]
    [Range(0f, 100f)]
    public float stunChance = 100f;
    public GameObject stunVFXPrefab;

    public override void ApplyEffect(GameObject target, Vector2 sourcePosition)
    {           
        if (Random.Range(0f, 100f) <= stunChance)
        {
            Entity entity = target.GetComponentInParent<Entity>();
            
            if (entity != null)
            {
                entity.ApplyStun(stunDuration, stunVFXPrefab);
            }
        }
    }
}