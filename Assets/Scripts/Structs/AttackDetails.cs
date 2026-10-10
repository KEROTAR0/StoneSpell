using UnityEngine;

public struct AttackDetails 
{
    public Vector2 position;
    public float damageAmount;
    public float stunDamageAmount;
    public float knockbackForce;
    
    public CCType crowdControl;
    public float ccDuration;
    public bool bypassHurtAnim;
}

// Khai báo Enum này ở ngoài cùng để dễ dùng chung
public enum CCType 
{
    None,
    Knockback,
    Pull,
    Stun,
    Freeze
}
