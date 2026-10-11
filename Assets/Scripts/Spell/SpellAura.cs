using UnityEngine;

[RequireComponent(typeof(SpellController))]
public class SpellAura : MonoBehaviour
{
    private SpellController controller;
    private float auraTimer;

    private void Awake() => controller = GetComponent<SpellController>();

    public void Init() => auraTimer = controller.spellData.auraTickRate;

    private void Update()
    {
        if (controller.spellData.auraTickRate <= 0) return; 
        
        auraTimer += Time.deltaTime;
        if (auraTimer >= controller.spellData.auraTickRate)
        {
            auraTimer = 0;
            Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, controller.spellData.auraCheckRadius, controller.targetLayer);
            foreach (var target in targets) 
            {
                controller.ApplyEffectsToTarget(target.gameObject);
            }
        }
    }
}