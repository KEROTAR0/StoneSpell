using UnityEngine;

public class PlayerSpellController : MonoBehaviour
{
    [Header("Spell Slots (Kéo Prefab Phép Thuật vào đây)")]
    public GameObject[] equippedSpellPrefabs = new GameObject[5]; 
    
    private float[] cooldownTimers = new float[5];
    private void Start()
    {
        // Gửi thông tin Icon lên UI ngay khi bắt đầu game
        InitializeUI();
    }
    private void Update()
    {
        for (int i = 0; i < cooldownTimers.Length; i++)
        {
            if (cooldownTimers[i] > 0)
            {
                cooldownTimers[i] -= Time.deltaTime;
                
                if (UIManager.Instance != null && equippedSpellPrefabs[i] != null)
                {
                    D_Spell data = equippedSpellPrefabs[i].GetComponent<SpellController>().spellData;
                    UIManager.Instance.UpdateCooldownUI(i + 1, cooldownTimers[i], data.cooldown);
                }
            }
        }
    }
    public void InitializeUI()
    {
        // Vòng lặp quét 5 ô kỹ năng
        for (int i = 0; i < equippedSpellPrefabs.Length; i++)
        {
            int uiSlotIndex = i; 

            if (equippedSpellPrefabs[i] != null)
            {
                D_Spell data = equippedSpellPrefabs[i].GetComponent<SpellController>().spellData;
                if (data != null && UIManager.Instance != null)
                {
                    // Gắn hình ảnh vào UI
                    UIManager.Instance.SetSpellIcon(uiSlotIndex, data.spellIcon);
                }
            }
            else
            {
                // Nếu ô phép trống, gửi null để UI ẩn icon đi
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.SetSpellIcon(uiSlotIndex, null);
                }
            }
        }
    }
    // Hàm dùng để trang bị (gán) phép thuật vào túi đồ
    public void EquipSpell(int slotIndex, GameObject spellPrefab)
    {
        // Kiểm tra xem index có hợp lệ không (từ 0 đến 4)
        if (slotIndex < 0 || slotIndex >= equippedSpellPrefabs.Length) return;

        // 1. Gán prefab vào mảng
        equippedSpellPrefabs[slotIndex] = spellPrefab;

        // 2. Cập nhật Icon lên giao diện UI ngay lập tức
        if (spellPrefab != null)
        {
            D_Spell data = spellPrefab.GetComponent<SpellController>().spellData;
            if (UIManager.Instance != null && data != null)
            {
                UIManager.Instance.SetSpellIcon(slotIndex, data.spellIcon);
            }
        }
        else
        {
            // Nếu truyền prefab = null (tháo phép), ẩn icon đi
            if (UIManager.Instance != null)
            {
                UIManager.Instance.SetSpellIcon(slotIndex, null);
            }
        }
    }
    public bool CanCastSpell(int slotIndex)
    {
        GameObject prefab = equippedSpellPrefabs[slotIndex];
        if (prefab == null) return false;

        SpellController SpellController = prefab.GetComponent<SpellController>();
        if (SpellController == null || SpellController.spellData == null) return false;

        D_Spell spellData = SpellController.spellData;

        if (spellData.isPermanentPassive) 
        {
            Debug.Log("Không thể tự kích hoạt phép nội tại tĩnh!");
            return false;
        }
        
        if (cooldownTimers[slotIndex] > 0) return false;
        
        if (Player.Instance.currentMana < spellData.manaCost) return false;

        return true;
    }

    public void TriggerCooldown(int slotIndex)
    {
        D_Spell spellData = equippedSpellPrefabs[slotIndex].GetComponent<SpellController>().spellData;
        cooldownTimers[slotIndex] = spellData.cooldown;
    }
}