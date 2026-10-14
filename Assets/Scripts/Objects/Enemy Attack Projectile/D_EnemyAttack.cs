using System.Collections.Generic;
using UnityEngine;

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

    [Header("Behavior")]
    public AttackBehavior behavior;
    public GameObject spawnOnDeathPrefab; 

    [Header("Melee Settings")]
    public bool rotateToDirection = true;

    [Header("Hit Effects")]
    public List<EffectData> hitEffects = new List<EffectData>();
}