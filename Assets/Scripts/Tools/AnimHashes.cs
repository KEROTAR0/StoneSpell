using UnityEngine;

// Lớp này lưu trữ toàn bộ tên Animation của cả game
public static class AnimHashes 
{
    // Locomotion
    public static readonly int Idle = Animator.StringToHash("idle");
    public static readonly int Move = Animator.StringToHash("move");
    public static readonly int DirX = Animator.StringToHash("dirX");
    public static readonly int DirY = Animator.StringToHash("dirY");
    public static readonly int Speed = Animator.StringToHash("speed");

    
    // Actions
    public static readonly int Attack = Animator.StringToHash("attack");
    public static readonly int Melee = Animator.StringToHash("melee");
    public static readonly int Ranged = Animator.StringToHash("ranged");
    public static readonly int Hurt = Animator.StringToHash("hurt");
    public static readonly int Stun = Animator.StringToHash("stun");
    public static readonly int Dead = Animator.StringToHash("dead");
    public static readonly int Dodge = Animator.StringToHash("dodge");
    public static readonly int Destruct = Animator.StringToHash("destruct");
}