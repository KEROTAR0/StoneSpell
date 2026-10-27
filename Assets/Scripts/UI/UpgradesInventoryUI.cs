using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradesInventoryUI : MonoBehaviour
{
    [Header("Main Inventory Panel")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform scrollContent;          // Object Content bên trong Scroll View
    [SerializeField] private UpgradeCardUI cardPrefab;         // Prefab của Thẻ Nâng Cấp
    [SerializeField] private GameObject emptyTextObject;       // Chữ hiện lên khi chưa có nâng cấp nào

    [Header("Capacity Bar UI")]
    [SerializeField] private Image capacityFillImage;          // Ảnh Fill (Image Type = Filled) của thanh Capacity
    [SerializeField] private TMP_Text capacityText;            // VD: "Capacity: 4/10 (Remaining: 6)"

    [Header("Confirm Remove Popup")]
    [SerializeField] private GameObject confirmPopup;          // Panel xác nhận xóa
    [SerializeField] private TMP_Text confirmMessageText;      // Chữ hỏi xác nhận
    
    private D_Upgrade pendingRemoveUpgrade;
    private List<UpgradeCardUI> spawnedCards = new List<UpgradeCardUI>();

    private void Start()
    {
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (confirmPopup != null) confirmPopup.SetActive(false);
    }

    // =========================================================
    // 1. CLICK EVENTS: MỞ / ĐÓNG PANEL INVENTORY
    // =========================================================
    public void OpenInventoryPanel()
    {
        if (inventoryPanel == null) return;
        
        inventoryPanel.SetActive(true);
        if (confirmPopup != null) confirmPopup.SetActive(false);
        
        Time.timeScale = 0f; // Đảm bảo tạm dừng game khi đang xem kho đồ
        RefreshInventoryUI();
    }

    public void CloseInventoryPanel()
    {
        if (confirmPopup != null) confirmPopup.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);

        // Nếu bảng Level Up đang KHÔNG mở thì mới cho thời gian chạy lại
        if (UIManager.Instance != null && !UIManager.Instance.IsLevelUpPanelActive())
        {
            Time.timeScale = 1f;
        }
    }

    // =========================================================
    // 2. HIỂN THỊ DANH SÁCH THẺ & THANH CAPACITY
    // =========================================================
    public void RefreshInventoryUI()
    {
        if (RunManager.Instance == null) return;

        // Cập nhật Thanh Capacity
        int used = RunManager.Instance.currentCapacity;
        int max = RunManager.Instance.maxCapacity;
        int remaining = Mathf.Max(0, max - used);

        if (capacityFillImage != null)
        {
            capacityFillImage.fillAmount = max > 0 ? (float)used / max : 0f;
        }

        if (capacityText != null)
        {
            capacityText.text = $"{used} / {max} (Remaining: {remaining})";
        }

        // Xóa các thẻ cũ đang hiển thị
        foreach (var card in spawnedCards)
        {
            if (card != null) Destroy(card.gameObject);
        }
        spawnedCards.Clear();

        List<D_Upgrade> activeList = RunManager.Instance.activeUpgrades;

        if (emptyTextObject != null)
        {
            emptyTextObject.SetActive(activeList.Count == 0);
        }

        // Sinh các thẻ nâng cấp hiện có vào Scroll Content
        for (int i = 0; i < activeList.Count; i++)
        {
            D_Upgrade upgrade = activeList[i];
            if (upgrade == null) continue;

            UpgradeCardUI newCard = Instantiate(cardPrefab, scrollContent);
            newCard.gameObject.SetActive(true);
            
            float animDelay = i * 0.05f;
            newCard.SetupInventoryCard(upgrade, OnRequestRemoveUpgrade, animDelay);
            spawnedCards.Add(newCard);
        }
    }

    // =========================================================
    // 3. CLICK EVENTS: XÁC NHẬN XÓA / HỦY XÓA UPGRADE
    // =========================================================
    private void OnRequestRemoveUpgrade(D_Upgrade upgrade)
    {
        if (upgrade == null) return;
        pendingRemoveUpgrade = upgrade;

        if (confirmMessageText != null)
        {
            confirmMessageText.text = $"Remove <color=#FFD700>{upgrade.upgradeName}</color>?\nRefunds {upgrade.upgradeCost} Cost(s)";
        }

        if (confirmPopup != null)
        {
            confirmPopup.SetActive(true);
        }
    }

    // Gắn vào nút "YES / CONFIRM" trên bảng xác nhận
    public void OnClick_ConfirmRemove()
    {
        if (pendingRemoveUpgrade != null && RunManager.Instance != null)
        {
            RunManager.Instance.RemoveUpgrade(pendingRemoveUpgrade);
            pendingRemoveUpgrade = null;
        }

        if (confirmPopup != null) confirmPopup.SetActive(false);
        
        // Vẽ lại danh sách và cập nhật lại thanh Capacity ngay lập tức
        RefreshInventoryUI();
    }

    // Gắn vào nút "NO / CANCEL" hoặc vùng trống ngoài bảng xác nhận
    public void OnClick_CancelRemove()
    {
        pendingRemoveUpgrade = null;
        if (confirmPopup != null) confirmPopup.SetActive(false);
    }
}