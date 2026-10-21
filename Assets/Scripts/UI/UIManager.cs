using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HP - Single Heart")]
    [SerializeField] private Image heartImage;
    [SerializeField] private Sprite[] heartSprites;

    [Header("Bar")]
    [SerializeField] private Image manaFill; // Lớp ảnh có Image Type là Filled
    [SerializeField] private TMP_Text manaText;
    [SerializeField] private Image expFill; // Lớp ảnh có Image Type là Filled
    [SerializeField] private TMP_Text levelText;

    [Header("Action Slots (0: Normal, 1-5: Spells)")]
    [SerializeField] private Image[] cooldownOverlays; // Ảnh nền mờ che lên ô chiêu thức
    [SerializeField] private Image[] spellIconImages;
    [SerializeField] private Sprite lockedSlotSprite;

    [Header("Level Up UI")]
    [SerializeField] private GameObject levelUpPanel;

    private void Awake()
    {
        Instance = this;
    }

    // 1. Cập nhật Trái tim
    public void UpdateHealthUI(float currentHealth, float maxHealth)
    {
        float healthPercentage = (currentHealth / maxHealth) * 100f;
        if (maxHealth <= 0f)
        {
            heartImage.sprite = heartSprites[5];
            return;
        }
        int index = healthPercentage switch
        {
            >= 100f => 0,
            >= 75f => 1,
            >= 50f => 2,
            >= 25f => 3,
            > 0f => 4,
            _ => 5
        };
        heartImage.sprite = heartSprites[index];
    }
    public bool IsLevelUpPanelActive()
    {
        return levelUpPanel != null && levelUpPanel.activeSelf;
    }
    // 2. Cập nhật Thanh Mana
    public void UpdateManaUI(float currentMana, float maxMana)
    {
        manaFill.fillAmount = currentMana / maxMana;
        manaText.text = $"{Mathf.CeilToInt(currentMana)} / {Mathf.CeilToInt(maxMana)}";
    }
    public void UpdateExpUI(float currentExp, float maxExp, int currentLevel)
    {
        expFill.fillAmount = currentExp / maxExp;
        levelText.text = $"{currentLevel}";
    }

    // 3. Cập nhật Cooldown (Gọi liên tục trong Update của Player/SpellController)
    // slotIndex: 0 là đòn thường, 1-5 là Spells
    public void UpdateCooldownUI(int slotIndex, float currentCooldown, float maxCooldown, bool isToggleActive = false)
    {
        if (slotIndex >= 0 && slotIndex < cooldownOverlays.Length)
        {
            if (currentCooldown > 0)
            {
                cooldownOverlays[slotIndex].fillAmount = currentCooldown / maxCooldown;
                Color overlayColor = isToggleActive ? new Color(0f, 0.5f, 1f, 0.7f) : new Color(1f, 0f, 0f, 0.7f);
                cooldownOverlays[slotIndex].color = overlayColor;
            }
            else
            {
                cooldownOverlays[slotIndex].fillAmount = 0f;
            }
        }
    }
    public void ShowLevelUpPanel(bool show)
    {
        if (levelUpPanel != null)
        {
            levelUpPanel.SetActive(show);
            // Dừng thời gian khi bảng nâng cấp hiện lên (Giống Vampire Survivors)
            Time.timeScale = show ? 0f : 1f; 
            
            // Tùy chọn: Ở đây bạn có thể gọi hàm random 3 phép và gán hình ảnh/text vào upgradeCards
        }
    }
    public void SetSpellIcon(int slotIndex, Sprite icon, bool isUnlocked = true)
    {
        if (slotIndex >= 0 && slotIndex < spellIconImages.Length)
        {
            if (icon != null)
            {
                spellIconImages[slotIndex].sprite = icon;
                spellIconImages[slotIndex].color = Color.white; 
            }
            else
            {
                spellIconImages[slotIndex].sprite = isUnlocked ? null : lockedSlotSprite;
                // ĐÃ SỬA: Chưa mở khóa = Trắng (thấy ổ khóa), Mở rồi = Trong suốt (trống trơn)
                spellIconImages[slotIndex].color = isUnlocked ? new Color(1, 1, 1, 0) : Color.white; 
            }
        }
    }
}