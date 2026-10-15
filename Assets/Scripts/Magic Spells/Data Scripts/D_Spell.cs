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
    public bool isPermanentPassive;


    [Header("Movement & Physics")]
    public float inaccuracy = 0f;
    public float moveSpeed = 5f;
    public int maxBounces = 0;
    public float acceleration = 1.1f;
    public float laserMaxLength = 10f;
    [Tooltip("Độ dày (trục Y) của tia Laser")]
    public float laserWidth = 1f;


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

    [Header("Effects")]
    public List<EffectData> hitEffects;

}
