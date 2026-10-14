using UnityEngine;

[CreateAssetMenu(fileName = "newEntityData", menuName = "Data/Entity Data/Base Data")]
public class D_Entity : ScriptableObject
{
    public bool hasNoAnimLeftOrRight;
    public int maxHealth = 100;
    public float stunResistance = 3f;
    public float stunRecoveryTime = 2f;
    public float stunVFXPrefabHeight = 0.25f;
    public float shortRangeAction = 1f;

    [Header("Movement")]
    public float movementSpeed = 2f;

    [Header("Patrol & Vision")]
    public float patrolRadius = 8f;
    public float visionRange = 7f;
    public float visionAngle = 90f;
    
    [Header("Mana Hunter Settings")]
    public bool isManaHunter = false;

    [Header("Dodge Settings")]
    public bool canDodge = false;
    public float dodgeCooldown = 6f;
    public float dodgeSpeed = 12f;
    public float dodgeDuration = 0.25f;

    [Header("Spell Detection")]
    public bool canDodgeSpells = false;
    public float spellDetectionRadius = 3f;
    
    [Header("Attack Prefabs")]
    public GameObject meleeAttackPrefab;
    public GameObject rangedAttackPrefab;
    
    [Header("Attack Settings")]
    public float meleeCooldown = 1.5f;
    public float rangedCooldown = 1.5f;

    [Header("Self Destruct")]
    public bool canSelfDestruct = false;
    public float knockbackForce = 1;
    [Range(0f, 1f)]
    public float selfDestructHpThreshold = 0.2f; 
    public float selfDestructDamage = 2f;
    public GameObject selfDestructVFX;
    
    [Header("Death Settings")]
    public GameObject deathAoEPrefab;

    [Header("Obstacle Detection")]
    public float obstacleCheckDistance = 0.5f;

}
