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
        // Rút ngẫu nhiên 3 thẻ. Ở đây tôi gộp chung cả Spell và Stat thành 1 List cho dễ hiển thị.
        // Bạn có thể tùy chỉnh tỷ lệ rớt (Ví dụ 2 Stat, 1 Spell).
        
        for (int i = 0; i < upgradeCards.Length; i++)
        {
            // Trộn ngẫu nhiên (Ví dụ đơn giản: Random 50/50 giữa lấy Phép mới và Tăng chỉ số)
            if (Random.value > 0.5f && availableSpellPrefabs.Count > 0)
            {
                // Rút 1 Phép
                GameObject randomSpell = availableSpellPrefabs[Random.Range(0, availableSpellPrefabs.Count)];
                D_Spell spellData = randomSpell.GetComponent<SpellController>().baseSpellData;
                
                // Hiển thị lên UI thẻ
                upgradeCards[i].SetupSpellCard(randomSpell, spellData);
            }
            else if (availableStatUpgrades.Count > 0)
            {
                // Rút 1 Chỉ số
                D_Upgrade randomStat = availableStatUpgrades[Random.Range(0, availableStatUpgrades.Count)];
                
                // Hiển thị lên UI thẻ
                upgradeCards[i].SetupStatCard(randomStat);
            }
        }
    }
}