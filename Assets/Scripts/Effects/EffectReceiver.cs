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
        if (data.type == EffectType.DamageMultiplier)
        {
            // Nếu value = 1 (Không thay đổi gì) -> Hủy bỏ, không thêm Effect
            if (Mathf.Approximately(data.value, 1f)) return;
            
            // bỏ qua người chơi
            if ((GameLayers.PlayerMask.value & (1 << gameObject.layer)) > 0) return;
        }

        bool isInstantDamageOrHeal = (data.type == EffectType.DoT || data.type == EffectType.Heal || data.type == EffectType.ManaRestore) && data.tickRate <= 0f;
        bool isInstantPushPull = (data.type == EffectType.Pull || data.type == EffectType.Knockback);

        if (isInstantDamageOrHeal || isInstantPushPull)
        {
            if (EffectDictionary.OnStartMap.TryGetValue(data.type, out var instantAction))
            {
                instantAction.Invoke(this, data, sourcePosition);
            }
            return; 
        }

        if (activeEffects.TryGetValue(data.type, out ActiveEffect existingEffect))
        {
            bool isStronger = false;

            if (data.type == EffectType.MovementMultiplier)
            {
                // Làm chậm (Slow): Hệ số càng NHỎ thì tốc độ càng chậm -> Mạnh hơn
                if (data.value < existingEffect.data.value) isStronger = true;
            }
            else if (data.type == EffectType.DamageMultiplier)
            {
                // Độ chênh lệch so với 1 càng lớn thì Effect càng mạnh (VD: 1.5 mạnh hơn 1.2 | 0.5 mạnh hơn 0.8)
                if (Mathf.Abs(1f - data.value) > Mathf.Abs(1f - existingEffect.data.value)) isStronger = true;
            }
            else
            {
                // Các loại khác (DoT, Tăng sát thương...): Value càng LỚN càng mạnh
                if (data.value > existingEffect.data.value) isStronger = true;
            }

            if (isStronger)
            {
                RemoveEffect(data.type);
            }
            else
            {
                // NẾU BẰNG HOẶC YẾU HƠN: Giữ nguyên Value cũ đang mạnh, CHỈ làm mới lại thời gian
                existingEffect.currentTimer = Mathf.Max(existingEffect.currentTimer, data.duration);
                return;
            }
        }

        if (!activeEffects.ContainsKey(data.type))
        {
            ActiveEffect newEffect = new ActiveEffect { data = data, currentTimer = data.duration, tickTimer = data.tickRate, sourcePos = sourcePosition };
            
            if (data.type == EffectType.MovementMultiplier)
            {
                newEffect.vfxInstance = GameManager.Instance.SpawnSpeedVFX(transform, data.value, 0f);
            }
            else if (data.type == EffectType.DamageMultiplier && GameManager.Instance != null)
            {
                newEffect.vfxInstance = GameManager.Instance.SpawnDamageMultiplierVFX(transform, data.value, 0f);
            }
            // VFX THÔNG THƯỜNG (Dựa vào data.vfxPrefab)
            else if (data.vfxPrefab != null)
            {
                newEffect.vfxInstance = ObjectPoolManager.Instance.GetFromPool(data.vfxPrefab, transform.position, Quaternion.identity);
                newEffect.vfxInstance.transform.SetParent(transform);
            }

            activeEffects.Add(data.type, newEffect);

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

    public void HandleContinuousVacuum(Vector2 pos, float speed, float duration)
    {
        if (TryGetComponent(out PullableComponent pullable)) 
        {
            // Truyền 3 biến sang cho PullableComponent
            pullable.ReceiveContinuousVacuum(pos, speed, duration);
        }

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

        if (TryGetComponent(out Pathfinding.AIPath aiPath))
        {
            // Nếu multiplier bị ép về 0 (Đóng băng) -> Tắt canMove
            if (multiplier == 0f || (cc != null && cc.moveSpeedMultiplier == 0f))
            {
                aiPath.canMove = false;
                
                if (TryGetComponent(out Rigidbody2D rb))
                {
                    rb.linearVelocity = Vector2.zero;
                }
            }
            else
            {
                aiPath.canMove = true;
            }
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