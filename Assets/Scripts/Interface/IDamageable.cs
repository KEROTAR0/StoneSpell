public interface IDamageable {
    void TakeDamage(AttackDetails attackDetails);
    void ApplyAilment(StatusAilment ailmentType, float duration, float value);
}
