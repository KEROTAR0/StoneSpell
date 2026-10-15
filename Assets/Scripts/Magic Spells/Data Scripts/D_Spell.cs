using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newSpellData", menuName = "Data/Spell Data/Spell")]
public class D_Spell : ScriptableObject
{    
[Header("Basic Info")]
    public string spellName;
    public Sprite spellIcon;
    public float manaCost = 10f;
    public float lifeTime = 5f;
    public float castingTime = 0.3f;
    public float cooldown = 2f;
    public bool isSelfTargeted;
    public bool isPermanentPassive;

    [Header("Targeting")]
    public bool isSpawnAtMouse;
    public float maxCastRange = 8f;

    [Header("Spell Types")]
    // (Đã có: public bool isProjectile; public bool hasAura; public bool isSelfTargeted; ...)
    public float inaccuracy = 0f;
    public bool isPenetrating;
    public bool isLaser;

    [Header("Penetrate & Laser Settings")]
    public float acceleration = 10f;
    public float laserMaxLength = 10f;
    [Tooltip("Độ dày (trục Y) của tia Laser")]
    public float laserWidth = 1f;

    [Header("Movement & Physics")]
    public bool isProjectile;
    public float moveSpeed = 15f;
    public int maxBounces = 0;

    [Header("Animation & Visuals")]
    public bool isRotating;
    public float rotationSpeed = 360f;
    [Header("Obstacle Interaction")]
    public bool isPassThroughtHighObstacle;


    [Header("Defensive Traits")]
    public bool canBlock;
    public int maxBlocks = 3;
    public bool canReflect;

    [Header("Aura")]
    public bool hasAura;
    public float auraCheckRadius = 3f;
    public float auraTickRate = 0.5f;

    [Header("Effects")]
    public List<EffectData> hitEffects;

}
