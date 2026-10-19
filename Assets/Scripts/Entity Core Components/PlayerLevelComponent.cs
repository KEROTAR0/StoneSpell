using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;

public class PlayerLevelComponent : MonoBehaviour
{
    private PlayerManaComponent playerMana;
    private PlayerSpellController spellController;
    [Header("Level Info")]
    public int currentLevel { get; private set; }
    public int currentXp { get; private set; }
    public int xpToNextLevel { get; private set; }
    
    [Tooltip("Hệ số nhân XP cần thiết cho cấp tiếp theo (VD: 1.2 = tăng 20%)")]
    public float xpScalingFactor = 1.2f;

    [Header("Events")]
    public UnityEvent OnLevelUp;

    public void Init(int currentXp, int xpToNextLevel, int currentLevel)
    {
        this.currentXp = currentXp;
        this.xpToNextLevel = xpToNextLevel;
        this.currentLevel = currentLevel;
        UpdateUI();
    }
    private void Awake()
    {
        playerMana = GetComponent<PlayerManaComponent>();
        spellController = GetComponent<PlayerSpellController>();
        
        // Đăng ký sự kiện: Cứ lên cấp là gọi hàm Hồi 20% Mana và Kiểm tra mở slot
        OnLevelUp.AddListener(HandleLevelUpRewards);
    }
    private void HandleLevelUpRewards()
    {
        // 1. Hồi 20% Max Mana
        if (playerMana != null)
        {
            playerMana.RestoreManaPercentage(0.2f);
        }

        // 2. Mở khóa ô phép theo mốc Level (Ví dụ: Cấp 3 mở slot 3, Cấp 5 mở slot 4, Cấp 10 mở slot 5)
        if (spellController != null)
        {
            if (currentLevel == 3) spellController.UnlockSlot(2); // Index 2 = Ô số 3
            if (currentLevel == 5) spellController.UnlockSlot(3); // Index 3 = Ô số 4
            if (currentLevel == 10) spellController.UnlockSlot(4); // Index 4 = Ô số 5
        }
    }
    public void AddXp(int amount)
    {
        currentXp += amount;
        
        // Dùng while để xử lý trường hợp ăn 1 cục XP to lên luôn 2-3 cấp
        while (currentXp >= xpToNextLevel)
        {
            LevelUp();
        }
        
        // GỌI CẬP NHẬT UI THANH XP Ở ĐÂY
        UpdateUI();
    }

    private void LevelUp()
    {
        currentXp -= xpToNextLevel;
        currentLevel++;
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * xpScalingFactor);

        // Cập nhật giao diện thanh Exp ngay lập tức
        UpdateUI();

        // Hồi 20% Mana
        if (TryGetComponent(out PlayerManaComponent mana))
            mana.RestoreManaPercentage(0.2f);

        // Mở khóa slot (Ví dụ ở cấp 3, 5, 10)
        if (TryGetComponent(out PlayerSpellController spellController))
        {
            if (currentLevel == 3) spellController.UnlockSlot(2); 
            if (currentLevel == 5) spellController.UnlockSlot(3); 
            if (currentLevel == 10) spellController.UnlockSlot(4); 
        }

        // BẬT BẢNG NÂNG CẤP VÀ DỪNG GAME
        if (UpgradeManager.Instance != null)
        {
            UpgradeManager.Instance.GenerateUpgrades();
        }
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLevelUpPanel(true);
        }

        OnLevelUp?.Invoke();
    }
    private void UpdateUI()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateExpUI(currentXp, xpToNextLevel, currentLevel);
    }
}