using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RuntimeSpellStats
{
    public Vector3 currentScale;
    public float moveSpeed;
    public float acceleration;
    public int maxBounces;
    public int maxBlocks;
    public float lifeTime;
    
    public float cooldown;
    public float manaCost;
    public float castingTime;
    public int spawnCount;

    public float auraTickRate;
    public float auraCheckRadius;
    public float explosionRadius;
    
    public float laserMaxLength;
    public float laserWidth;
    public float rotationSpeed;
    public float inaccuracy;

    [Header("Đẩy Lùi / Hút (Vacuum)")]
    public float pullForce;
    public float knockbackForce;
    public float pushPullRadius; // Tách riêng khỏi explosionRadius

    [Header("Xuyên Thấu & Vỡ Vụn (Fragmentation)")]
    public int maxPierce;        // Xuyên thấu (0 = chạm là nổ, >0 = bay xuyên)
    public int fragmentCount;    // Số mảnh vỡ sinh ra khi nổ/hết tầm
    public float fragmentDamageMultiplier; // Sát thương mảnh vỡ (VD: 0.5 = 50% dmg gốc)
    
    // Giữ nguyên các biến Bool và List
    public bool isSelfTargeted;
    public bool canReflect;
    public bool isPenetrating;
    public bool isPassThroughtHighObstacle;
    
    public List<EffectData> activeEffects;

    public RuntimeSpellStats(D_Spell template, Vector3 baseScale)
    {
        // Khởi tạo các biến cũ y như cũ...
        currentScale = baseScale;
        moveSpeed = template.moveSpeed;
        acceleration = template.acceleration;
        maxBounces = template.maxBounces;
        maxBlocks = template.maxBlocks;
        lifeTime = template.lifeTime;
        cooldown = template.cooldown;
        manaCost = template.manaCost;
        castingTime = template.castingTime;
        spawnCount = template.spawnCount;
        auraTickRate = template.auraTickRate;
        auraCheckRadius = template.auraCheckRadius;
        explosionRadius = template.explosionRadius;
        laserMaxLength = template.laserMaxLength;
        laserWidth = template.laserWidth;
        rotationSpeed = template.rotationSpeed;
        inaccuracy = template.inaccuracy;
        isSelfTargeted = template.isSelfTargeted;
        canReflect = template.canReflect;
        isPenetrating = template.isPenetrating;
        isPassThroughtHighObstacle = template.isPassThroughtHighObstacle;
        
        activeEffects = new List<EffectData>();
        if (template.hitEffects != null) activeEffects.AddRange(template.hitEffects);

        pullForce = template.pullForce;
        knockbackForce = template.knockbackForce;
        pushPullRadius = template.pushPullRadius;
        maxPierce = template.maxPierce;
        fragmentCount = template.fragmentCount;
        fragmentDamageMultiplier = template.fragmentDamageMultiplier;
    }
}