using UnityEngine;

public enum StatusAilment 
{ 
    None, 
    Stun, 
    Stop, 
    Slow, 
    ManaDrain, 
    Weaken 
}

public enum AttackBehavior 
{ 
    NormalProjectile,
    StationaryMelee,
    SpawnAoEOnHit,
    TelegraphedAoE
}

[CreateAssetMenu(fileName = "New Enemy Attack Type", menuName = "Data/Enemy Attack")]
public class D_EnemyAttack : ScriptableObject
{
    [Header("Core Stats")]
    public float damage = 1f;
    public float moveSpeed = 5f;
    public float lifeTime = 5f;

    [Header("Ailment")]
    public StatusAilment ailmentType;
    public float ailmentDuration = 2f;
    public float ailmentValue = 0f; // Dùng cho Slow (vd: giảm 50% tốc) hoặc ManaDrain (trừ 10 MP)

    [Header("Behavior")]
    public AttackBehavior behavior;
    public GameObject spawnOnDeathPrefab; // Dành cho vũng lầy / AoE
    [Header("Melee Settings")]
    [Tooltip("Tick nếu muốn Hitbox tự xoay góc 360 độ chỉ thẳng vào Player. Bỏ tick nếu dùng Sprite cố định 4 hướng.")]
    public bool rotateToDirection = true;
}