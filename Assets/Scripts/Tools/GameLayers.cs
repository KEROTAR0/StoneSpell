using UnityEngine;

public static class GameLayers
{
    // =========================================================
    // 1. LAYER INDEX
    // (Dùng để gán Layer cho GameObject: gameObject.layer = ...)
    // =========================================================
    public static readonly int PlayerLayerIndex = LayerMask.NameToLayer("Player");
    public static readonly int EnemyLayerIndex = LayerMask.NameToLayer("Enemy");
    public static readonly int EnemyAttackLayerIndex = LayerMask.NameToLayer("EnemyAttack");
    public static readonly int PlayerAttackLayerIndex = LayerMask.NameToLayer("PlayerAttack");
    public static readonly int WallLayerIndex = LayerMask.NameToLayer("Wall");
    public static readonly int HighObstacleLayerIndex = LayerMask.NameToLayer("HighObstacle");
    public static readonly int LowObstacleLayerIndex = LayerMask.NameToLayer("LowObstacle");
    public static readonly int DamageableLayerIndex = LayerMask.NameToLayer("Damageable");
    public static readonly int TrapLayerIndex = LayerMask.NameToLayer("Trap");
    public static readonly int ManaLayerIndex = LayerMask.NameToLayer("Mana");

    // =========================================================
    // 2. LAYER MASK ĐƠN LẺ 
    // (Dùng để kiểm tra va chạm, Physics2D.Raycast, OverlapCircle)
    // =========================================================
    public static readonly LayerMask PlayerMask = 1 << PlayerLayerIndex;
    public static readonly LayerMask EnemyMask = 1 << EnemyLayerIndex;
    public static readonly LayerMask EnemyAttackMask = 1 << EnemyAttackLayerIndex;
    public static readonly LayerMask PlayerAttackMask = 1 << PlayerAttackLayerIndex;
    public static readonly LayerMask WallMask = 1 << WallLayerIndex;
    public static readonly LayerMask HighObstacleMask = 1 << HighObstacleLayerIndex;
    public static readonly LayerMask LowObstacleMask = 1 << LowObstacleLayerIndex;
    public static readonly LayerMask DamageableMask = 1 << DamageableLayerIndex;
    public static readonly LayerMask TrapMask = 1 << TrapLayerIndex;
    public static readonly LayerMask ManaMask = 1 << ManaLayerIndex;

    // =========================================================
    // 3. LAYER MASK KẾT HỢP
    // (Dùng toán tử Bitwise OR '|' để gộp nhiều Mask lại với nhau)
    // =========================================================
    
    // Dùng cho đạn của Player hoặc Enemy khi phản đòn (Trúng Quái hoặc Đồ vật có thể phá hủy)
    public static readonly LayerMask EnemyAndDamageableMask = EnemyMask | DamageableMask;
    
    // Dùng cho đạn của Quái vật (Trúng Player hoặc Đồ vật)
    public static readonly LayerMask PlayerAndDamageableMask = PlayerMask | DamageableMask;
    
    // Dùng để chặn tầm nhìn (Raycast Vision) và chặn đạn bay
    public static readonly LayerMask WallAndHighObstacleMask = WallMask | HighObstacleMask;
    
    // Dùng cho những kỹ năng/đạn sát mặt đất, bị chặn bởi cả vật cản thấp (như vũng nước, tường thấp)
    public static readonly LayerMask AllObstaclesMask = WallMask | HighObstacleMask | LowObstacleMask;
}