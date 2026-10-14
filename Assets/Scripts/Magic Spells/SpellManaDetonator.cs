using UnityEngine;

// Gắn script này vào Prefab Đạn/Phép thuật của Player
public class SpellManaDetonator : MonoBehaviour
{
    [Tooltip("Bán kính sóng âm tỏa ra để kích nổ Mana. Có thể set to bằng cả màn hình (20f)")]
    public float detonationRadius = 5f;
    
    
    [Tooltip("Kích nổ ngay khi đạn sinh ra (true) hay kích nổ liên tục khi bay (false)?")]
    public bool triggerOnStart = true;

    private void Start()
    {
        if (triggerOnStart) ScanAndDetonate();
    }

    // Nếu muốn đạn bay tới đâu kích nổ tới đó, hãy bỏ comment hàm Update hoặc ngược lại
    
    private void Update()
    {
        if (!triggerOnStart) ScanAndDetonate();
    }
    

    private void ScanAndDetonate()
    {
        Collider2D[] shards = Physics2D.OverlapCircleAll(transform.position, detonationRadius, GameLayers.ManaMask);
        foreach (Collider2D col in shards)
        {
            if (col.TryGetComponent(out ManaShard shard))
            {
                shard.Detonate();
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detonationRadius);
    }
}