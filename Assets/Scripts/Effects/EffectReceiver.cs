using System.Collections.Generic;
using UnityEngine;

public class EffectReceiver : MonoBehaviour
{
    // Kho chứa các hiệu ứng đang bám trên cơ thể
    private Dictionary<EffectType, ActiveEffect> activeEffects = new Dictionary<EffectType, ActiveEffect>();

    // Lớp phụ để theo dõi thời gian của từng hiệu ứng
    private class ActiveEffect
    {
        public EffectData data;
        public float currentTimer;
        public float tickTimer;
        public Vector2 sourcePos;
        public GameObject vfxInstance;

        public void Reset(EffectData newData, Vector2 newPos)
        {
            data = newData;
            sourcePos = newPos;
            currentTimer = newData.duration;
            tickTimer = newData.tickRate;
        }
    }

    public void ApplyEffect(EffectData data, Vector2 sourcePosition = default)
    {
        if (Random.value > data.procChance) return;
        
        if (activeEffects.TryGetValue(data.type, out ActiveEffect existingEffect))
        {
            // Trùng hiệu ứng -> Reset lại thời gian (Không bị cộng dồn vô hạn)
            existingEffect.Reset(data, sourcePosition);
        }
        else
        {
            // Hiệu ứng mới -> Khởi tạo và thêm vào Dictionary
            ActiveEffect newEffect = new ActiveEffect { data = data, currentTimer = data.duration, tickTimer = data.tickRate, sourcePos = sourcePosition };
            if (data.vfxPrefab != null)
            {
                newEffect.vfxInstance = ObjectPoolManager.Instance.GetFromPool(data.vfxPrefab, transform.position, Quaternion.identity);
                newEffect.vfxInstance.transform.SetParent(transform);
            }
            activeEffects.Add(data.type, newEffect);

            // Kích hoạt tác dụng tức thời (nếu có)
            if (EffectDictionary.OnStartMap.TryGetValue(data.type, out var action))
                action.Invoke(this, data, sourcePosition);
        }
    }

    public void RemoveEffect(EffectType type)
    {
        if (activeEffects.TryGetValue(type, out ActiveEffect effect))
        {
            if (EffectDictionary.OnEndMap.TryGetValue(type, out var action))
                action.Invoke(this, effect.data, effect.sourcePos);
            if (effect.vfxInstance != null)
            {
                effect.vfxInstance.transform.SetParent(null);
                ObjectPoolManager.Instance.AddToPool(effect.vfxInstance);
            }
            activeEffects.Remove(type);
        }
    }

    private void Update()
    {
        List<EffectType> keysToRemove = new List<EffectType>();

        foreach (var kvp in activeEffects)
        {
            ActiveEffect effect = kvp.Value;
            effect.currentTimer -= Time.deltaTime;
            effect.tickTimer -= Time.deltaTime;

            // Xử lý giật Tick (DoT, Hồi Mana)
            if (effect.tickTimer <= 0 && effect.data.tickRate > 0)
            {
                effect.tickTimer = effect.data.tickRate;
                if (EffectDictionary.OnTickMap.TryGetValue(effect.data.type, out var action))
                    action.Invoke(this, effect.data, effect.sourcePos);
            }

            // Hết giờ
            if (effect.currentTimer <= 0) keysToRemove.Add(kvp.Key);
        }

        foreach (var key in keysToRemove) RemoveEffect(key);
    }

    // =========================================================================
    // CÁC HÀM XỬ LÝ LÕI (DÙNG CHUNG / TỰ ĐỘNG PHÂN LOẠI PLAYER-ENEMY)
    // =========================================================================

    public void HandleStun(float duration)
    {
        if (TryGetComponent(out CoreCCComponent cc)) cc.ApplyStun(duration, false);
    }

    public void HandleImpulsePull(Vector2 pos, float force, float duration)
    {
        if (TryGetComponent(out KnockbackComponent kb)) kb.ApplyPull(pos, force, duration);
    }

    public void HandleContinuousVacuum(Vector2 pos, float speed)
    {
        if (TryGetComponent(out PullableComponent pullable)) pullable.ReceiveContinuousVacuum(pos, speed);
        if (TryGetComponent(out CoreCCComponent cc)) 
        {
            cc.OnInterrupt?.Invoke(); 
        }
    }

    public void TakeDamage(float amount)
    {
        if (TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(new AttackDetails 
                { 
                    damageAmount = amount, 
                    position = transform.position,
                    isDoT = true
                });
        }
    }

    public void RestoreMana(float amount)
    {
        if (TryGetComponent(out PlayerManaComponent mana)) mana.RestoreMana(amount);
    }

    // --- MULTIPLIERS ---
    public void ModifyMovement(float multiplier)
    {
        if (TryGetComponent(out CoreCCComponent cc)) 
        {
            cc.moveSpeedMultiplier *= multiplier;
        }
    }

    public void ModifyDamageTaken(float multiplier)
    {
        if (TryGetComponent(out HealthComponent health)) health.damageTakenMultiplier *= multiplier;
    }

    // --- HEAL & CC ---
    public void RestoreHealth(float amount)
    {
        int finalAmount = Mathf.RoundToInt(amount);
        if (TryGetComponent(out HealthComponent health)) health.RestoreHealth(finalAmount);
    }
    
    public void HandleSleep(float duration)
    {
        if (TryGetComponent(out CoreCCComponent cc)) cc.ApplyStun(duration, true);
    }

    public void HandleKnockback(Vector2 sourcePos, float force)
    {
        if (TryGetComponent(out KnockbackComponent kb)) 
        {
            kb.ApplyKnockback(sourcePos, force);
        }
    }
}