using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeCardUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descText;
    
    private GameObject storedSpellPrefab;
    private D_Upgrade storedStatUpgrade;
    private bool isSpellCard;

    public void SetupSpellCard(GameObject spellPrefab, D_Spell spellData)
    {
        isSpellCard = true;
        storedSpellPrefab = spellPrefab;
        storedStatUpgrade = null;

        iconImage.sprite = spellData.spellIcon;
        nameText.text = "Phép thuật mới"; // Bạn có thể lấy spellData.spellName nếu có
        descText.text = $"Mana Cost: {spellData.manaCost} \nCasting Time: {spellData.castingTime} \nCooldown: {spellData.cooldown}s"; 
    }

    public void SetupStatCard(D_Upgrade statUpgrade)
    {
        isSpellCard = false;
        storedSpellPrefab = null;
        storedStatUpgrade = statUpgrade;

        iconImage.sprite = statUpgrade.icon;
        nameText.text = statUpgrade.upgradeName;
        descText.text = statUpgrade.description;
    }

    // Hàm này phải được NỐI vào sự kiện OnClick() của Button (UI Thẻ)
    public void OnClick_SelectUpgrade()
    {
        PlayerController player = PlayerController.Instance;
        if (player == null) return;

        if (isSpellCard)
        {
            // TÌM Ô TRỐNG ĐỂ LẮP PHÉP (Chỉ lấy trong phạm vi số ô đã mở khóa)
            int unlockedCount = player.SpellController.unlockedSlotCount;
            bool equipSuccess = false;
            
            for (int i = 0; i < unlockedCount; i++)
            {
                // Kiểm tra xem ô đó có trống không (chưa có phép nào được gán)
                if (player.SpellController.equippedSpellPrefabs[i] == null)
                {
                    // Lắp phép vào ô trống
                    player.SpellController.EquipSpell(i, storedSpellPrefab);
                    equipSuccess = true;
                    Debug.Log($"Đã lắp phép {storedSpellPrefab.name} vào ô số {i}");
                    break; // Thoát khỏi vòng lặp sau khi lắp thành công
                }
            }
            
            // Nếu duyệt hết các ô đã mở khóa mà vẫn không lắp được
            if (!equipSuccess) 
            {
                Debug.LogWarning("Túi đã đầy! Không còn ô trống để lắp thêm phép."); 
                // TODO: Xử lý trường hợp túi đầy (Ví dụ: Mở bảng thay thế phép)
            }
        }
        else
        {
            if (storedStatUpgrade != null && RunManager.Instance != null)
            {
                RunManager.Instance.AddUpgrade(storedStatUpgrade);
            }
        }

        // Tắt bảng và tiếp tục Game
        UIManager.Instance.ShowLevelUpPanel(false);
        if (player.TryGetComponent(out PlayerLevelComponent levelComp))
        {
            levelComp.CheckAndShowLevelUpPanel();
        }
    }
}