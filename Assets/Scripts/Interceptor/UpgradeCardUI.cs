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
        if (spellPrefab == null || spellData == null) return;
        isSpellCard = true;
        storedSpellPrefab = spellPrefab;
        storedStatUpgrade = null;

        iconImage.sprite = spellData.spellIcon;
        nameText.text = "New Spell"; // Bạn có thể lấy spellData.spellName nếu có
        descText.text = $"Mana Cost: {spellData.manaCost} \nCasting Time: {spellData.castingTime} \nCooldown: {spellData.cooldown}s"; 
    }

    public void SetupStatCard(D_Upgrade statUpgrade)
    {
        if (statUpgrade == null) return;
        isSpellCard = false;
        storedSpellPrefab = null;
        storedStatUpgrade = statUpgrade;

        iconImage.sprite = statUpgrade.icon;
        nameText.text = statUpgrade.upgradeName;
        descText.text = statUpgrade.description;

        if (iconImage != null) iconImage.sprite = statUpgrade.icon;
        if (nameText != null) nameText.text = statUpgrade.upgradeName;
        if (descText != null) descText.text = statUpgrade.description;
    }

    // Hàm này phải được NỐI vào sự kiện OnClick() của Button (UI Thẻ)
    public void OnClick_SelectUpgrade()
    {
        PlayerController player = PlayerController.Instance;
        if (player == null) return;

        if (isSpellCard)
        {
            int unlockedCount = player.SpellController.unlockedSlotCount;
            bool equipSuccess = false;
            
            for (int i = 0; i < unlockedCount; i++)
            {
                if (player.SpellController.equippedSpellPrefabs[i] == null)
                {
                    player.SpellController.EquipSpell(i, storedSpellPrefab);
                    equipSuccess = true;
                    Debug.Log($"Đã lắp phép {storedSpellPrefab.name} vào ô số {i}");
                    break;
                }
            }
            
            if (!equipSuccess) 
            {
                Debug.LogWarning("Túi đã đầy! Đang kích hoạt chế độ THAY THẾ PHÉP."); 
                // Gửi yêu cầu thay thế cho PlayerSpellController
                player.SpellController.StartReplaceMode(storedSpellPrefab);
                
                // KHÔNG TẮT BẢNG NÂNG CẤP VỘI (Chờ người chơi chọn ô để đè lên)
                // Đổi text hướng dẫn trên UI (Tùy chọn)
                return; 
            }
        }
        else
        {
            if (storedStatUpgrade != null && RunManager.Instance != null)
            {
                RunManager.Instance.AddUpgrade(storedStatUpgrade);
            }
        }

        // Nếu lắp thành công (hoặc là thẻ Chỉ số), tiến hành đóng bảng và mở bảng tiếp theo nếu còn nợ
        FinalizeSelection(player);
    }

    private void FinalizeSelection(PlayerController player)
    {
        UIManager.Instance.ShowLevelUpPanel(false);
        if (player.TryGetComponent(out PlayerLevelComponent levelComp))
        {
            levelComp.CheckAndShowLevelUpPanel();
        }
    }
}