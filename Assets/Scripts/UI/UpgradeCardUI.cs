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
                if (player.SpellController.equippedSpellPrefabs[i] == null)
                {
                    player.SpellController.EquipSpell(i, storedSpellPrefab);
                    equipSuccess = true;
                    break;
                }
            }
            
            // Nếu full ô đã mở khóa, có thể phải làm hệ thống thay thế (Replace)
            if (!equipSuccess) Debug.Log("Túi đã đầy, không thể lắp thêm!"); 
        }
        else
        {
            // ÁP DỤNG CHỈ SỐ VÀO NGƯỜI
            // (Bạn sẽ cần viết một hàm ApplyUpgrade(D_Upgrade) vào Player để cộng dồn máu/mana/sát thương)
            // player.ApplyStatUpgrade(storedStatUpgrade); 
        }

        // Tắt bảng và tiếp tục Game
        UIManager.Instance.ShowLevelUpPanel(false);
    }
}