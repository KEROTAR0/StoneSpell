using System.Collections.Generic;

[System.Serializable]
public class RuntimeSpellStats
{
    public float moveSpeed;
    public float acceleration;
    public int maxBounces;
    public int maxBlocks;
    public float lifeTime;
    public float auraTickRate;
    public float auraCheckRadius;
    public float laserMaxLength;
    public float laserWidth;
    public float rotationSpeed;
    public float inaccuracy;
    public bool isSelfTargeted;
    public bool canReflect;
    public bool isPenetrating;
    public bool isPassThroughtHighObstacle;
    
    // Bản sao danh sách hiệu ứng để Trạm kiểm duyệt nhét thêm Độc, Cháy...
    public List<EffectData> activeEffects;

    public RuntimeSpellStats(D_Spell template)
    {
        moveSpeed = template.moveSpeed;
        acceleration = template.acceleration;
        maxBounces = template.maxBounces;
        maxBlocks = template.maxBlocks;
        lifeTime = template.lifeTime;
        auraTickRate = template.auraTickRate;
        auraCheckRadius = template.auraCheckRadius;
        laserMaxLength = template.laserMaxLength;
        laserWidth = template.laserWidth;
        rotationSpeed = template.rotationSpeed;
        inaccuracy = template.inaccuracy;
        isSelfTargeted = template.isSelfTargeted;
        canReflect= template.canReflect;
        isPenetrating = template.isPenetrating;
        isPassThroughtHighObstacle = template.isPassThroughtHighObstacle;

        activeEffects = new List<EffectData>();
        if (template.hitEffects != null)
            activeEffects.AddRange(template.hitEffects);
    }
}