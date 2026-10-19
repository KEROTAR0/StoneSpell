using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newSpellData", menuName = "Data/Spell Data")]
public class D_Spell : ScriptableObject
{    
[Header("Basic Info")]
    public string spellName;
    public Sprite spellIcon;
    public float manaCost = 10f;
    public float lifeTime = 5f;
    public float castingTime = 0.3f;
    public float cooldown = 2f;


    [Header("Targeting")]
    public bool isSpawnAtMouse;
    public float maxCastRange = 8f;


    [Header("Spell Types")]
    public bool isProjectile;
    public bool isPenetrating;
    public bool isLaser;
    public bool isSelfTargeted;
    public bool isManualToggle = false;


    [Header("Movement & Physics")]
    public float inaccuracy = 0f;
    public float moveSpeed = 5f;
    public int maxBounces = 0;
    public float acceleration = 1.1f;
    public float laserMaxLength = 10f;
    [Tooltip("Độ dày (trục Y) của tia Laser")]
    public float laserWidth = 1f;

    [Header("Explosion on Despawn (Rocket/Fireball)")]
    [Tooltip("Tích vào đây nếu muốn đạn phát nổ khi va chạm hoặc hết thời gian bay")]
    public bool explodeOnDespawn = false;
    public float explosionRadius = 2.5f;
    
    [Header("Homing / Chasing Mechanics")]
    public bool isHoming = false;
    [Tooltip("Bán kính quét tìm quái vật gần nhất")]
    public float homingDetectionRadius = 8f;
    [Tooltip("Tốc độ bẻ lái (Độ/giây) - Càng cao quành càng gắt")]
    public float homingTurnSpeed = 180f;
    [Tooltip("Thời gian bay thẳng trước khi bắt đầu nhắm mục tiêu")]
    public float homingDelayTimer = 0.3f;
    
    [Header("VFX")]
    [Tooltip("VFX của vụ nổ (Đảm bảo VFX này tự động tắt/thu hồi sau khi nổ xong)")]
    public GameObject explosionVFX;
    public GameObject toggleIndicatorVFX;

    [Header("Animation & Visuals")]
    public bool isRotating;
    public float rotationSpeed = 360f;
    [Header("Obstacle Interaction")]
    public bool isPassThroughtHighObstacle;


    [Header("Defensive Traits")]
    public bool canBlock;
    public int maxBlocks = 1;
    public bool canReflect;

    [Header("Aura")]
    public bool hasAura;
    public float auraCheckRadius = 3f;
    public float auraTickRate = 0.5f;

    [Header("Orb & Mine Mechanics (Only if isProjectile / isSelfTargeted)")]
    public int spawnCount = 1;
    public float orbitRadius = 1.5f;
    public float orbRotationSpeed = 90f;

    [Header("Effects")]
    public List<EffectData> hitEffects;

}
