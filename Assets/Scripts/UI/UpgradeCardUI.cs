using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections;
using System;

public class UpgradeCardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image cardBackgroundImage;
    [SerializeField] private Image iconBorderImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private RectTransform cardRect;
    
    // THÊM: Tham chiếu tới chữ trên nút bấm ("GET THIS" / "REMOVE")
    [SerializeField] private TMP_Text buttonText;

    [Header("Icon & Color")]
    public Sprite spellBorderSprite;
    public Sprite spellUpgradeBorderSprite;
    public Sprite playerUpgradeBorderSprite;
    
    [Header("Border Colors")]
    public Color spellBgColor = Color.white;
    public Color spellUpgradeBgColor = Color.yellow;
    public Color playerUpgradeBgColor = Color.cyan;

    [Header("Card Text Settings")]
    [SerializeField] private string manaPrefix = "Mana: ";
    [SerializeField] private string castTimePrefix = "Cast Time: ";
    [SerializeField] private string cooldownPrefix = "Cooldown: ";
    [SerializeField] private string secSuffix = "s";
    [SerializeField] private string effectHeader = "\n<color=#FFD700>Effects:</color>";

    private GameObject storedSpellPrefab;
    private D_Upgrade storedStatUpgrade;
    private bool isSpellCard;
    private float targetHeight = 640f;

    // THÊM: Cờ phân biệt thẻ trong Kho đồ và sự kiện yêu cầu xóa
    private bool isInventoryCard = false;
    private Action<D_Upgrade> onRemoveRequested;

    public void SetupSpellCard(GameObject spellPrefab, D_Spell spellData, float delayAnimTime)
    {
        if (spellPrefab == null || spellData == null) return;
        isSpellCard = true;
        isInventoryCard = false;
        storedSpellPrefab = spellPrefab;
        storedStatUpgrade = null;

        if (buttonText != null) buttonText.text = "GET THIS";

        if (cardBackgroundImage != null) cardBackgroundImage.color = spellBgColor;
        if (iconBorderImage != null)
        {
            iconBorderImage.sprite = spellBorderSprite;
            iconBorderImage.color = new Color(0.85f, 0.75f, 0.6f, 1f);
        }
        
        iconImage.sprite = spellData.spellIcon;
        nameText.text = spellData.spellName;
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"{manaPrefix}{spellData.manaCost}");
        sb.AppendLine($"{castTimePrefix}{spellData.castingTime}{secSuffix}");
        sb.Append($"{cooldownPrefix}{spellData.cooldown}{secSuffix}");

        if (spellData.hitEffects != null && spellData.hitEffects.Count > 0)
        {
            sb.Append(effectHeader);
            foreach (var effect in spellData.hitEffects)
            {
                if (effect.type == EffectType.DoT)
                {
                    if (effect.tickRate <= 0) sb.Append($"\n- Damage: {effect.value}");
                    else sb.Append($"\n- DoT: {effect.value} / {effect.tickRate}s");
                }
                else if (effect.type == EffectType.ManaRestore)
                {
                    string sign = effect.value > 0 ? "+" : "";
                    sb.Append($"\n- Mana: {sign}{effect.value}");
                }
                else
                {
                    sb.Append($"\n- {effect.type}: {effect.duration}s");
                }
            }
        }
        descText.text = sb.ToString();

        PlayUnfoldAnimation(delayAnimTime);
    }

    public void SetupStatCard(D_Upgrade statUpgrade, float delayAnimTime)
    {
        if (statUpgrade == null) return;
        isSpellCard = false;
        isInventoryCard = false;
        storedSpellPrefab = null;
        storedStatUpgrade = statUpgrade;

        if (buttonText != null) buttonText.text = "CHOOSE";

        ApplyStatVisuals(statUpgrade);
        PlayUnfoldAnimation(delayAnimTime);
    }

    // =========================================================
    // HÀM MỚI: DÙNG CHO THẺ TRONG UPGRADES INVENTORY PANEL
    // =========================================================
    public void SetupInventoryCard(D_Upgrade statUpgrade, Action<D_Upgrade> onRemoveCallback, float delayAnimTime = 0f)
    {
        if (statUpgrade == null) return;
        isSpellCard = false;
        isInventoryCard = true;
        storedSpellPrefab = null;
        storedStatUpgrade = statUpgrade;
        onRemoveRequested = onRemoveCallback;

        if (buttonText != null) buttonText.text = "REMOVE";

        ApplyStatVisuals(statUpgrade);
        PlayUnfoldAnimation(delayAnimTime);
    }

    private void ApplyStatVisuals(D_Upgrade statUpgrade)
    {
        if (statUpgrade.upgradeCategory == UpgradeCategory.PlayerStat)
        {
            if (cardBackgroundImage != null) cardBackgroundImage.color = playerUpgradeBgColor;
            if (iconBorderImage != null)
            {
                iconBorderImage.sprite = playerUpgradeBorderSprite;
                iconBorderImage.color = Color.white;
            }
        }
        else // SpellStat
        {
            if (cardBackgroundImage != null) cardBackgroundImage.color = spellUpgradeBgColor;
            if (iconBorderImage != null)
            {
                iconBorderImage.sprite = spellUpgradeBorderSprite;
                iconBorderImage.color = Color.white;
            }
        }

        iconImage.sprite = statUpgrade.icon;
        nameText.text = statUpgrade.upgradeName;
        string costColorHex = statUpgrade.upgradeCost switch
        {
            1 => "#55FF55",
            2 => "#FFFF00",
            3 => "#FFA500",
            >= 4 => "#FF4444",
            _ => "#FFFFFF"
        };
        descText.text = $"Cost: <color={costColorHex}>{statUpgrade.upgradeCost}</color>\n{statUpgrade.description}";
    }

    private void PlayUnfoldAnimation(float delay)
    {
        if (cardRect == null) return;
        StopAllCoroutines();
        StartCoroutine(UnfoldRoutine(delay));
    }

    private IEnumerator UnfoldRoutine(float delay)
    {
        Vector2 size = cardRect.sizeDelta;
        size.y = 0;
        cardRect.sizeDelta = size;

        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);
            
            size.y = Mathf.Lerp(0f, targetHeight, t);
            cardRect.sizeDelta = size;
            yield return null;
        }

        size.y = targetHeight;
        cardRect.sizeDelta = size;
    }

    public void OnClick_SelectUpgrade()
    {
        // NẾU ĐANG Ở TRONG INVENTORY -> GỌI BẢNG XÁC NHẬN XÓA
        if (isInventoryCard)
        {
            onRemoveRequested?.Invoke(storedStatUpgrade);
            return;
        }

        PlayerController player = PlayerController.Instance;
        if (player == null) return;

        if (isSpellCard)
        {
            int unlockedCount = player.PSpellController.unlockedSlotCount;
            bool equipSuccess = false;
            
            for (int i = 0; i < unlockedCount; i++)
            {
                if (player.PSpellController.equippedSpellPrefabs[i] == null)
                {
                    player.PSpellController.EquipSpell(i, storedSpellPrefab);
                    equipSuccess = true;
                    Debug.Log($"Đã lắp phép {storedSpellPrefab.name} vào ô số {i}");
                    break;
                }
            }
            
            if (!equipSuccess) 
            {
                Debug.LogWarning("Túi đã đầy! Đang kích hoạt chế độ THAY THẾ PHÉP.");
                player.PSpellController.StartReplaceMode(storedSpellPrefab);
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