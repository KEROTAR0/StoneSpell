using UnityEngine;

// Đảm bảo phải có Mana Component đi kèm
[RequireComponent(typeof(PlayerManaComponent))]
public class PlayerSpellController : MonoBehaviour
{
    [Header("Spell Slots (Kéo Prefab Phép Thuật vào đây)")]
    public GameObject[] equippedSpellPrefabs = new GameObject[5]; 
    
    // Mảng lưu đệm Data để KHÔNG gọi GetComponent trong Update
    private D_Spell[] cachedSpellData = new D_Spell[5]; 
    private float[] cooldownTimers = new float[5];

    private PlayerManaComponent playerMana;

    private void Awake()
    {
        // Liên kết nội bộ thay vì dùng PlayerController.Instance
        playerMana = GetComponent<PlayerManaComponent>();
    }

    private void Start()
    {
        // Lưu đệm toàn bộ data lúc bắt đầu
        for (int i = 0; i < equippedSpellPrefabs.Length; i++)
        {
            CacheSpellData(i, equippedSpellPrefabs[i]);
        }
        InitializeUI();
    }

    private void Update()
    {
        for (int i = 0; i < cooldownTimers.Length; i++)
        {
            if (cooldownTimers[i] > 0)
            {
                cooldownTimers[i] -= Time.deltaTime;
                
                // Dùng thẳng data đã lưu đệm, tốc độ xử lý nhanh gấp 100 lần!
                if (UIManager.Instance != null && cachedSpellData[i] != null)
                {
                    UIManager.Instance.UpdateCooldownUI(i + 1, cooldownTimers[i], cachedSpellData[i].cooldown);
                }
            }
        }
    }

    // Hàm tiện ích để nạp Data vào mảng đệm
    private void CacheSpellData(int index, GameObject prefab)
    {
        if (prefab != null && prefab.TryGetComponent(out SpellController spellCtrl))
        {
            // SỬ DỤNG baseSpellData ĐÚNG THEO KIẾN TRÚC MỚI
            cachedSpellData[index] = spellCtrl.baseSpellData; 
        }
        else
        {
            cachedSpellData[index] = null;
        }
    }

    public void InitializeUI()
    {
        for (int i = 0; i < cachedSpellData.Length; i++)
        {
            if (UIManager.Instance != null)
            {
                Sprite icon = cachedSpellData[i] != null ? cachedSpellData[i].spellIcon : null;
                UIManager.Instance.SetSpellIcon(i, icon);
            }
        }
    }

    public void EquipSpell(int slotIndex, GameObject spellPrefab)
    {
        if (slotIndex < 0 || slotIndex >= equippedSpellPrefabs.Length) return;

        equippedSpellPrefabs[slotIndex] = spellPrefab;
        
        // Cập nhật mảng đệm ngay khi thay đồ
        CacheSpellData(slotIndex, spellPrefab); 

        if (UIManager.Instance != null)
        {
            Sprite icon = cachedSpellData[slotIndex] != null ? cachedSpellData[slotIndex].spellIcon : null;
            UIManager.Instance.SetSpellIcon(slotIndex, icon);
        }
    }

    public bool CanCastSpell(int slotIndex)
    {
        // Tra cứu cực nhanh từ mảng đệm
        D_Spell data = cachedSpellData[slotIndex];
        if (data == null) return false;

        if (data.isPermanentPassive) 
        {
            Debug.Log("Không thể tự kích hoạt phép nội tại tĩnh!");
            return false;
        }
        
        if (cooldownTimers[slotIndex] > 0) return false;
        
        // Gọi thẳng qua Component Mana thay vì chạy vòng qua Controller
        if (playerMana.currentMana < data.manaCost) return false;

        return true;
    }

    public void TriggerCooldown(int slotIndex)
    {
        D_Spell data = cachedSpellData[slotIndex];
        if (data != null)
        {
            cooldownTimers[slotIndex] = data.cooldown;
        }
    }
}