using UnityEngine;

[CreateAssetMenu(fileName = "New Damage Effect", menuName = "Data/Spell Data/Effects/Damage Effect")]
public class DamageEffect : SpellEffect
{
    public float baseDamage = 20f;
    public float stunDamageAmount = 0f;
    public float knockbackForce = 10f;
    public GameObject hitVFXPrefab; // Kéo thả prefab hiệu ứng trúng đòn vào đây

    public override void ApplyEffect(GameObject target, Vector2 sourcePosition)
    {
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            // 1. Tính toán sát thương tổng (Cơ bản x Hệ số Buff)
            float finalDamage = this.baseDamage;
            if (Player.Instance != null) finalDamage *= Player.Instance.damageMultiplier;

            AttackDetails attackDetails = new AttackDetails
            {
                position = sourcePosition,
                damageAmount = finalDamage,
                stunDamageAmount = this.stunDamageAmount,
                knockbackForce = this.knockbackForce
            };
            damageable.TakeDamage(attackDetails);

            // 2. Kích hoạt hiệu ứng hút Mana (Mana On Hit Buff)
            if (Player.Instance != null && Player.Instance.manaOnHitChance > 0)
            {
                if (Random.Range(0f, 100f) <= Player.Instance.manaOnHitChance)
                {
                    Player.Instance.AddMana(Player.Instance.manaOnHitAmount);
                }
            }

            // 3. Sinh hiệu ứng VFX từ Pool
            if (hitVFXPrefab != null)
            {
                GameObject vfx = SpellPool.Instance.GetFromPool(hitVFXPrefab, target.transform.position, Quaternion.identity);
                
                var vfxScript = vfx.GetComponent<VFXAnimationDespawn>();
                if (vfxScript == null) vfxScript = vfx.AddComponent<VFXAnimationDespawn>();
                
                // Gọi Setup chỉ với 1 tham số (không cần thời gian nữa)
                vfxScript.Setup(hitVFXPrefab); 
            }
        }
    }
}