using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("Kho Nâng Cấp Tĩnh")]
    public List<D_Upgrade> availableStatUpgrades; 
    public List<GameObject> availableSpellPrefabs; // Dùng GameObject nếu kho vũ khí là Prefab chứa SpellController

    [Header("UI Của 3 Thẻ")]
    public UpgradeCardUI[] upgradeCards; // Kéo 3 script của 3 nút UI vào đây

    private void Awake()
    {
        Instance = this;
    }

    // Hàm này được gọi bởi PlayerLevelComponent khi lên cấp
    public void GenerateUpgrades()
    {
        // 1. TẠO DANH SÁCH LỌC (Chỉ lấy những thẻ/phép chưa sở hữu)
        List<GameObject> validSpells = new List<GameObject>();
        List<D_Upgrade> validStats = new List<D_Upgrade>();

        PlayerController player = PlayerController.Instance;

        // Lọc Phép (Chỉ lấy phép chưa được gắn vào 5 ô của Player)
        if (player != null && player.SpellController != null)
        {
            foreach (GameObject spellPrefab in availableSpellPrefabs)
            {
                if (spellPrefab == null) continue;
                bool alreadyEquipped = false;
                foreach (GameObject equipped in player.SpellController.equippedSpellPrefabs)
                {
                    if (equipped == spellPrefab) 
                    {
                        alreadyEquipped = true; 
                        break;
                    }
                }
                if (!alreadyEquipped) validSpells.Add(spellPrefab);
            }
        }

        // Lọc Chỉ Số (Chỉ lấy những thẻ mà RunManager chưa có, hoặc thẻ đó cho phép cộng dồn nhiều lần)
        if (RunManager.Instance != null)
        {
            foreach (D_Upgrade stat in availableStatUpgrades)
            {
                if (stat == null) continue;
                if (!RunManager.Instance.activeUpgrades.Contains(stat))
                {
                    validStats.Add(stat);
                }
            }
        }

        // 2. RÚT THẺ VÀ HIỂN THỊ (Không cho phép 3 thẻ hiện ra bị trùng nhau)
        List<GameObject> selectedSpellsThisTurn = new List<GameObject>();
        List<D_Upgrade> selectedStatsThisTurn = new List<D_Upgrade>();

        for (int i = 0; i < upgradeCards.Length; i++)
        {
            // Nếu còn phép để bốc, và (Random > 0.5 HOẶC hết thẻ chỉ số rồi)
            if (validSpells.Count > 0 && (Random.value > 0.5f || validStats.Count == 0))
            {
                // Bốc 1 phép
                int randomIndex = Random.Range(0, validSpells.Count);
                GameObject randomSpell = validSpells[randomIndex];
                
                D_Spell spellData = randomSpell.GetComponent<SpellController>().baseSpellData;
                upgradeCards[i].SetupSpellCard(randomSpell, spellData);
                
                // Loại bỏ khỏi danh sách tạm để thẻ tiếp theo không bốc trúng thẻ này nữa
                validSpells.RemoveAt(randomIndex); 
            }
            else if (validStats.Count > 0)
            {
                // Bốc 1 chỉ số
                int randomIndex = Random.Range(0, validStats.Count);
                D_Upgrade randomStat = validStats[randomIndex];
                
                upgradeCards[i].SetupStatCard(randomStat);
                
                // Loại bỏ khỏi danh sách tạm để không bị trùng lặp trong 3 lựa chọn
                validStats.RemoveAt(randomIndex); 
            }
            else
            {
                // Trường hợp người chơi đã học SẠCH SÀNH SANH mọi thứ trong game
                // Bạn có thể Ẩn thẻ UI này đi hoặc hiển thị thẻ "Gà Tây Hồi Máu"
                upgradeCards[i].gameObject.SetActive(false);
            }
        }
    }
}