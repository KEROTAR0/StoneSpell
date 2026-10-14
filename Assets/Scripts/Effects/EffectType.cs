public enum EffectType
{
    DoT,                 // Rút máu (Cháy, Độc)
    Heal,                // Hồi máu (Instant hoặc HoT)
    ManaRestore,         // Hồi mana (Instant hoặc HoT)
    Stun,                // Choáng
    Sleep,               // Ngủ (Choáng nhưng thức dậy khi bị đánh)
    Pull,                // Hút
    Vacuum,
    Knockback,           // Đẩy lùi (Có Wall-Slam Stun)
    MovementMultiplier,  // Tăng/giảm tốc độ di chuyển
    DamageMultiplier     // Tăng/giảm sát thương nhận vào/gây ra
}