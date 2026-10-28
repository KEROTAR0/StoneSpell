using System;
using System.Collections.Generic;
using UnityEngine;

public static class EffectDictionary
{
    // KÍCH HOẠT 1 LẦN KHI BẮT ĐẦU (Buff Tức thì, Áp dụng Multiplier, CC)
    public static readonly Dictionary<EffectType, Action<EffectReceiver, EffectData, Vector2>> OnStartMap = new Dictionary<EffectType, Action<EffectReceiver, EffectData, Vector2>>()
    {
        { EffectType.Stun, (receiver, data, pos) => receiver.HandleStun(data.duration) },
        { EffectType.Sleep, (receiver, data, pos) => receiver.HandleSleep(data.duration) },
        { EffectType.Pull, (receiver, data, pos) => receiver.HandleImpulsePull(pos, data.value, data.duration) },
        { EffectType.Vacuum, (receiver, data, pos) => receiver.HandleContinuousVacuum(pos, data.value, data.tickRate > 0 ? data.tickRate : data.duration) },        { EffectType.Knockback, (receiver, data, pos) => receiver.HandleKnockback(pos, data.value) },
        { EffectType.MovementMultiplier, (receiver, data, pos) => receiver.ModifyMovement(data.value) },
        { EffectType.DamageMultiplier, (receiver, data, pos) => receiver.ModifyDamageTaken(data.value) },

        // Xử lý Instant (Chỉ chạy nếu tickRate <= 0)
        { EffectType.DoT, (receiver, data, pos) => { if(data.tickRate <= 0) receiver.TakeDamage(data.value); } },
        { EffectType.Heal, (receiver, data, pos) => { if(data.tickRate <= 0) receiver.RestoreHealth(data.value); } },
        { EffectType.ManaRestore, (receiver, data, pos) => { if(data.tickRate <= 0) receiver.RestoreMana(data.value); } }
    };

    // KÍCH HOẠT THEO NHỊP (DoT, HoT)
    public static readonly Dictionary<EffectType, Action<EffectReceiver, EffectData, Vector2>> OnTickMap = new Dictionary<EffectType, Action<EffectReceiver, EffectData, Vector2>>()
    {
        { EffectType.DoT, (receiver, data, pos) => { if(data.tickRate > 0) receiver.TakeDamage(data.value); } },
        
        // Xử lý Over Time (Chỉ chạy nếu tickRate > 0)
        { EffectType.Heal, (receiver, data, pos) => { if(data.tickRate > 0) receiver.RestoreHealth(data.value); } },
        { EffectType.ManaRestore, (receiver, data, pos) => { if(data.tickRate > 0) receiver.RestoreMana(data.value); } }
    };

    // KÍCH HOẠT KHI KẾT THÚC (Hoàn trả Multiplier)
    public static readonly Dictionary<EffectType, Action<EffectReceiver, EffectData, Vector2>> OnEndMap = new Dictionary<EffectType, Action<EffectReceiver, EffectData, Vector2>>()
    {
        // Phải đảo ngược phép nhân bằng phép chia (1 / value)
        { EffectType.MovementMultiplier, (receiver, data, pos) => receiver.ModifyMovement(1f / data.value) },
        { EffectType.DamageMultiplier, (receiver, data, pos) => receiver.ModifyDamageTaken(1f / data.value) }
    };
}