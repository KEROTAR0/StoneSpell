using UnityEngine;

[CreateAssetMenu(fileName = "New Pull Effect", menuName = "Data/Spell Data/Effects/Pull Effect")]
public class PullEffect : SpellEffect
{
    public float pullSpeed = 10f; 
    public float pullRadius = 5f; 
    public float pullDuration = 1.2f;

    public override void ApplyEffect(GameObject target, Vector2 sourcePosition)
    {
        float distance = Vector2.Distance(target.transform.position, sourcePosition);
        
        if (distance <= pullRadius && distance > 0.1f)
        {
            Entity entity = target.GetComponent<Entity>();
            
            if (entity != null)
            {
                entity.ApplyPull(sourcePosition, pullSpeed, pullDuration);
            }
        }
    }
}