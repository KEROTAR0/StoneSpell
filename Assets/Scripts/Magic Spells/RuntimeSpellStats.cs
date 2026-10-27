using System.Collections.Generic;
using UnityEngine; // Cần thêm Unity Engine để dùng Vector3

[System.Serializable]
public class RuntimeSpellStats
{
    // Kích thước mượn từ scale mặc định của Prefab
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
    
    public bool isSelfTargeted;
    public bool canReflect;
    public bool isPenetrating;
    public bool isPassThroughtHighObstacle;
    
    public List<EffectData> activeEffects;

    public RuntimeSpellStats(D_Spell template, Vector3 baseScale)
    {
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
        if (template.hitEffects != null)
            activeEffects.AddRange(template.hitEffects);
    }
}