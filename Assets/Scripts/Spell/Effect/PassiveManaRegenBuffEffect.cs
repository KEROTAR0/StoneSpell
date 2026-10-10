using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Passive Regen Mana Effect", menuName = "Data/Spell Data/Effects/Passive Mana Regen Effect")]
public class PassiveManaRegenBuffEffect : SpellEffect
{
    public float manaPerTick = 2f;
    public float tickRate = 1f;

    public override void ApplyEffect(GameObject target, Vector2 sourcePosition)
    {
        if (target.CompareTag("Player"))
        {
            var regenComp = target.GetComponent<ManaRegenComponent>() ?? target.AddComponent<ManaRegenComponent>();
            regenComp.StartRegen(manaPerTick, tickRate);
        }
    }

    public override void RemoveEffect(GameObject target)
    {
        if (target.CompareTag("Player"))
        {
            var regenComp = target.GetComponent<ManaRegenComponent>();
            if (regenComp != null) Destroy(regenComp);
        }
    }
}

// Lớp phụ trợ chạy Coroutine hồi Mana
public class ManaRegenComponent : MonoBehaviour
{
    private Coroutine regenCoroutine;

    public void StartRegen(float amount, float rate)
    {
        if (regenCoroutine != null) StopCoroutine(regenCoroutine);
        regenCoroutine = StartCoroutine(RegenRoutine(amount, rate));
    }

    private IEnumerator RegenRoutine(float amount, float rate)
    {
        while (true) // Chạy vô tận cho đến khi bị Destroy (bởi hàm RemoveEffect)
        {
            if (Player.Instance != null) Player.Instance.AddMana(amount);
            yield return new WaitForSeconds(rate);
        }
    }
}