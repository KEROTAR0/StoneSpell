using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New DoT Effect", menuName = "Data/Spell Data/Effects/DoT Effect")]
public class DoTEffect : SpellEffect
{
    public float totalDamage = 5f;
    public float duration = 5f;
    public float tickRate = 1f;
    public GameObject tickVFXPrefab;
    public float damageRadius = 1f;

    public override void ApplyEffect(GameObject target, Vector2 sourcePosition)
    {
        float distance = Vector2.Distance(target.transform.position, sourcePosition);
        if(distance <= damageRadius)
        {
            DoTStatusComponent dotComp = target.GetComponent<DoTStatusComponent>();
            if (dotComp == null)
            {
                dotComp = target.AddComponent<DoTStatusComponent>();
            }
            dotComp.ApplyDoT(totalDamage, duration, tickRate, tickVFXPrefab);
        }
    }
}

// Lớp phụ trợ gắn lên quái vật
public class DoTStatusComponent : MonoBehaviour
{
    private IDamageable damageable;
    private Coroutine dotCoroutine;

    public void ApplyDoT(float totalDamage, float duration, float tickRate, GameObject vfxPrefab)
    {
        damageable = GetComponent<IDamageable>();
        if (damageable == null) return;

        if (dotCoroutine != null) StopCoroutine(dotCoroutine);
        dotCoroutine = StartCoroutine(DoTRoutine(totalDamage, duration, tickRate, vfxPrefab));
    }

    private IEnumerator DoTRoutine(float totalDamage, float duration, float tickRate, GameObject vfxPrefab)
    {
        float damagePerTick = totalDamage / (duration / tickRate);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Trừ máu
            damageable.TakeDamage(new AttackDetails { damageAmount = damagePerTick, position = transform.position, knockbackForce = 0 });

            // Hiện VFX
            if (vfxPrefab != null)
            {
                GameObject vfx = SpellPool.Instance.GetFromPool(vfxPrefab, transform.position, Quaternion.identity);
                var vfxScript = vfx.GetComponent<VFXAnimationDespawn>();
                if (vfxScript == null) vfxScript = vfx.AddComponent<VFXAnimationDespawn>();
                
                // Gọi Setup chỉ với 1 tham số (không cần thời gian nữa)
                vfxScript.Setup(vfxPrefab);
            }

            yield return new WaitForSeconds(tickRate);
            elapsed += tickRate;
        }

        Destroy(this); // Gỡ bỏ script sau khi hết DoT
    }
}