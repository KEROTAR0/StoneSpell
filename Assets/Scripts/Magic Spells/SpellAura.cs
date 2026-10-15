using UnityEngine;

[RequireComponent(typeof(SpellController))]
public class SpellAura : MonoBehaviour
{
    private SpellController controller;
    private float auraTimer;

    private void Awake() => controller = GetComponent<SpellController>();

    public void Init() => auraTimer = controller.currentStats.auraTickRate;

    private void Update()
    {
        if (controller.currentStats.auraTickRate <= 0) return; 
        
        auraTimer += Time.deltaTime;
        if (auraTimer >= controller.currentStats.auraTickRate)
        {
            auraTimer = 0;
            Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, controller.baseSpellData.auraCheckRadius, GameLayers.EnemyAndDamageableMask);
            foreach (var target in targets) 
            {
                if(target != null)
                {
                    controller.ApplyEffectsToTarget(target.gameObject);
                    Vector2 hitPoint = target.ClosestPoint(transform.position);
                    controller.SpawnImpactVFXAtPosition(hitPoint);
                }
            }
        }
    }
}