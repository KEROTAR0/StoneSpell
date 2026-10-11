using UnityEngine;

public static class GameLayers
{
    // Dịch chuỗi thành số Layer 1 lần duy nhất khi game bắt đầu
    public static readonly int PlayerLayerIndex = LayerMask.NameToLayer("Player");
    public static readonly int EnemyLayerIndex = LayerMask.NameToLayer("Enemy");
    public static readonly int EnemyAttackLayerIndex = LayerMask.NameToLayer("EnemyAttack");
    public static readonly int PlayerAttackLayerIndex = LayerMask.NameToLayer("PlayerAttack");
    public static readonly int WallLayerIndex = LayerMask.NameToLayer("Wall");
    public static readonly int HighObstacleLayerIndex = LayerMask.NameToLayer("HighObstacle");
    public static readonly int LowObstacleLayerIndex = LayerMask.NameToLayer("LowObstacle");
    public static readonly int DamageableLayerIndex = LayerMask.NameToLayer("Damageable");
    public static readonly int TrapLayerIndex = LayerMask.NameToLayer("Trap");


    // 2. LAYER MASK (Dùng để kiểm tra va chạm, Raycast, và targetLayer)
    public static readonly LayerMask PlayerMask = 1 << PlayerLayerIndex;
    public static readonly LayerMask EnemyMask = 1 << EnemyLayerIndex;
    public static readonly LayerMask WallMask = 1 << WallLayerIndex;
    public static readonly LayerMask HighObstacleMask = 1 << HighObstacleLayerIndex;
    public static readonly LayerMask EnemyAttackMask = 1 << EnemyAttackLayerIndex;
    
    // Tạo sẵn các Mask kết hợp (Kết hợp bằng phép toán bitwise OR '|')
    public static readonly LayerMask EnemyAndDamageableMask = (1 << EnemyLayerIndex) | (1 << DamageableLayerIndex);
    public static readonly LayerMask PlayerAndDamageableMask = (1 << PlayerLayerIndex) | (1 << DamageableLayerIndex);
    public static readonly LayerMask WallAndHighObstacleMask = (1 << WallLayerIndex) | (1 << HighObstacleLayerIndex);
}