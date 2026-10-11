using UnityEngine;

[CreateAssetMenu(fileName = "newEntityData", menuName = "Data/Player Data/Base Data")]
public class D_Player : ScriptableObject
{
    public int maxHealth = 2;
    [Header("Move State")]
    public float movementSpeed = 2.0f;

    [Header("Dash State")]
    public float dashVelocity = 15f;
    public float dashTime = 0.2f;
    public float dashCooldown = 1f;

    [Header("Dash Effects")]
    public GameObject afterImagePrefab;

    [Header("Normal Attack")]
    public float bulletSpeed = 15f;
    public float normalAttackCooldown = 0.5f;
    public float normalAttackCastingTime = 0.15f;
    [Header("Attack State Settings")]
    public float attackSlowRate = 15f;      // Lực phanh (Tăng số này nếu muốn nhân vật dừng nhanh hơn)
    public float attackSlowDuration = 0.3f;
    [Header("Hurt State")]
    public float hurtDuration = 0.2f; // Thời gian nhân vật bị khựng lại khi dính đòn

    [Header("Mana Settings")]
    public float maxMana = 100f;
    public float manaCollectionRadius = 3f;
    public float manaVacuumSpeed = 15f;

    [Header("Other")]
    //public GameObject hitParticle;
    public LayerMask waterLayer;
    public LayerMask damagableLayer;
}
