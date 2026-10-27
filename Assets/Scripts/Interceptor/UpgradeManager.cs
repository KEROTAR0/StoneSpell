using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    [Header("Kho Nâng Cấp Tĩnh")]
    public List<D_Upgrade> availableStatUpgrades; 
    public List<GameObject> availableSpellPrefabs; // Dùng GameObject nếu kho vũ khí là Prefab chứa SpellController

    [Header("UI Của Thẻ")]
    public UpgradeCardUI[] upgradeCards;
    public float bonusCardChance = 0.15f;

    [Header("Reroll System")]
    public int currentRerolls { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        currentRerolls = 1; 
    }
    // Hàm này được gọi bởi PlayerLevelComponent khi lên cấp
    public void GenerateUpgrades()
    {
        if (UIManager.Instance != null && !UIManager.Instance.IsLevelUpPanelActive())
        {
            UIManager.Instance.ShowLevelUpPanel(true);
        }
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

        // Lọc Chỉ Số (áp dụng isUnique và maxUpgrade)
        if (RunManager.Instance != null)
        {
            foreach (D_Upgrade stat in availableStatUpgrades)
            {
                if (stat == null) continue;

                // 1. Đếm xem người chơi ĐÃ NHẶT thẻ này bao nhiêu lần rồi
                int ownedCount = 0;
                foreach (var activeStat in RunManager.Instance.activeUpgrades)
                {
                    if (activeStat == stat) ownedCount++;
                }

                // 2. Chốt chặn
                if (stat.isUnique && ownedCount >= 1) 
                    continue; 
                    
                if (!stat.isUnique && ownedCount >= stat.maxUpgrade) 
                    continue; 

                if (RunManager.Instance.currentCapacity + stat.upgradeCost > RunManager.Instance.maxCapacity)
                    continue;
                // Nếu an toàn qua trạm kiểm duyệt, cho phép bốc
                validStats.Add(stat);
            }
        }
        // 2. RÚT THẺ VÀ HIỂN THỊ (Không cho phép 3 thẻ hiện ra bị trùng nhau)
        int totalCardsToDraw = 3;
        List<GameObject> selectedSpellsThisTurn = new List<GameObject>();
        List<D_Upgrade> selectedStatsThisTurn = new List<D_Upgrade>();
        if (upgradeCards.Length >= 4 && Random.value <= bonusCardChance)
        {
            totalCardsToDraw = 4;
        }
        for (int i = 0; i < upgradeCards.Length; i++)
        {
            if (i >= totalCardsToDraw)
            {
                upgradeCards[i].gameObject.SetActive(false);
                continue; 
            }
            upgradeCards[i].gameObject.SetActive(true);
            float animDelay = i * 0.1f;
            // Nếu còn phép để bốc, và (Random > 0.5 HOẶC hết thẻ chỉ số rồi)
            if (validSpells.Count > 0 && (Random.value > 0.5f || validStats.Count == 0))
            {
                int randomIndex = Random.Range(0, validSpells.Count);
                GameObject randomSpell = validSpells[randomIndex];
                
                D_Spell spellData = randomSpell.GetComponent<SpellController>().baseSpellData;
                
                // GỌI HÀM VÀ TRUYỀN DELAY ANIMATION
                upgradeCards[i].SetupSpellCard(randomSpell, spellData, animDelay);
                
                validSpells.RemoveAt(randomIndex); 
            }
            else if (validStats.Count > 0)
            {
                int randomIndex = Random.Range(0, validStats.Count);
                D_Upgrade randomStat = validStats[randomIndex];
                
                upgradeCards[i].SetupStatCard(randomStat, animDelay);
                
                validStats.RemoveAt(randomIndex); 
            }
            else
            {
                upgradeCards[i].gameObject.SetActive(false);
            }
        }
    }
    public void RerollUpgrades()
    {
        if (currentRerolls > 0)
        {
            currentRerolls--;
            Debug.Log($"Đã Reroll! Còn lại {currentRerolls} lượt.");
            
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateRerollText(currentRerolls);
            }
            
            GenerateUpgrades(); 
        }
        else
        {
            Debug.LogWarning("Đã hết lượt Reroll!");
            UIManager.Instance.ChangeSText("You dont have any reroll left!");
        }
    }
    public void AddReroll(int amount = 1)
    {
        currentRerolls += amount;
        Debug.Log($"Nhận thêm {amount} lượt Reroll! Hiện đang có: {currentRerolls} lượt.");
    }
    public void SkipUpgrade()
    {
        Debug.Log("Bạn đã từ chối nhận nâng cấp lần này.");
        
        // Đóng bảng và cho thời gian chạy lại
        UIManager.Instance.ShowLevelUpPanel(false);

        // Kiểm tra xem có đang "nợ" lần lên cấp nào nữa không thì mở bảng tiếp
        PlayerController player = PlayerController.Instance;
        if (player != null)
        {
            if (player.SpellController != null) 
                player.SpellController.CancelReplaceMode();

            if (player.TryGetComponent(out PlayerLevelComponent levelComp))
                levelComp.CheckAndShowLevelUpPanel();
        }
    }
}