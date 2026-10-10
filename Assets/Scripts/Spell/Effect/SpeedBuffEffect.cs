using UnityEngine;

[CreateAssetMenu(fileName = "NewSpeedEffect", menuName = "Data/Spell Data/Effects/Speed Effect")]
public class SpeedBuffEffect : SpellEffect
{
    [Tooltip("> 1 là Buff (Tăng tốc), < 1 là Debuff (Làm chậm), 0 là Trói chân")]
    public float speedMultiplier = 1.5f; 
    public GameObject auraVFXPrefab; 
    
    // Vì ScriptableObject xài chung cho nhiều mục tiêu, không được lưu biến currentVFX ở đây
    // Ta sẽ dùng hệ thống Object Pool tự hủy hoặc script tạm trên đối tượng.

    public override void ApplyEffect(GameObject target, Vector2 sourcePosition)
    {
        Player player = target.GetComponent<Player>();
        if (player != null) player.moveSpeedMultiplier *= speedMultiplier;

        // Enemy enemy = target.GetComponent<Enemy>();
        // if (enemy != null) enemy.moveSpeedMultiplier *= speedMultiplier;

        // Sinh VFX dính vào mục tiêu
        if (auraVFXPrefab != null)
        {
            GameObject vfx = SpellPool.Instance.GetFromPool(auraVFXPrefab, target.transform.position, Quaternion.identity);
            vfx.transform.SetParent(target.transform);
            
            // Gắn cờ để RemoveEffect biết đường xóa
            vfx.name = "SpeedVFX_" + this.name; 
        }
    }

    public override void RemoveEffect(GameObject target)
    {
        Player player = target.GetComponent<Player>();
        // Tránh chia cho 0 nếu trước đó là chiêu trói chân
        if (player != null && speedMultiplier != 0) player.moveSpeedMultiplier /= speedMultiplier;
        else if (player != null && speedMultiplier == 0) player.moveSpeedMultiplier = 1f;

        // Tìm và dọn dẹp VFX
        Transform vfx = target.transform.Find("SpeedVFX_" + this.name);
        if (vfx != null)
        {
            Destroy(vfx.gameObject); // Hoặc đưa về Pool
        }
    }
}