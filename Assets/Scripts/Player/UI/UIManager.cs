using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HP - Single Heart")]
    [SerializeField] private Image heartImage;
    [SerializeField] private Sprite heart100;
    [SerializeField] private Sprite heart75;
    [SerializeField] private Sprite heart50;
    [SerializeField] private Sprite heart25;
    [SerializeField] private Sprite heart1;
    [SerializeField] private Sprite heart0;

    [Header("Mana - Bar")]
    [SerializeField] private Image manaFill; // Lớp ảnh có Image Type là Filled
    [SerializeField] private TMP_Text manaText;

    [Header("Action Slots (0: Normal, 1-5: Spells)")]
    [SerializeField] private Image[] cooldownOverlays; // Ảnh nền mờ che lên ô chiêu thức
    [SerializeField] private Image[] spellIconImages;

    private void Awake()
    {
        Instance = this;
    }

    // 1. Cập nhật Trái tim
    public void UpdateHealthUI(float currentHealth, float maxHealth)
    {
        float healthPercentage = (currentHealth / maxHealth) * 100f;

        if (healthPercentage >= 100f)
        {
            heartImage.sprite = heart100;
        }
        else if (healthPercentage >= 75f)
        {
            heartImage.sprite = heart75;
        }
        else if (healthPercentage >= 50f)
        {
            heartImage.sprite = heart50;
        }
        else if (healthPercentage >= 25f)
        {
            heartImage.sprite = heart25;
        }
        else if (healthPercentage > 0f)
        {
            heartImage.sprite = heart1;
        }
        else
        {
            heartImage.sprite = heart0; 
        }
    }

    // 2. Cập nhật Thanh Mana
    public void UpdateManaUI(float currentMana, float maxMana)
    {
        manaFill.fillAmount = currentMana / maxMana;
        manaText.text = $"{Mathf.CeilToInt(currentMana)} / {Mathf.CeilToInt(maxMana)}";
    }

    // 3. Cập nhật Cooldown (Gọi liên tục trong Update của Player/SpellController)
    // slotIndex: 0 là đòn thường, 1-5 là Spells
    public void UpdateCooldownUI(int slotIndex, float currentCooldown, float maxCooldown)
    {
        if (slotIndex >= 0 && slotIndex < cooldownOverlays.Length)
        {
            if (currentCooldown > 0)
            {
                cooldownOverlays[slotIndex].fillAmount = currentCooldown / maxCooldown;
            }
            else
            {
                cooldownOverlays[slotIndex].fillAmount = 0f;
            }
        }
    }
    public void SetSpellIcon(int slotIndex, Sprite icon)
    {
        if (slotIndex >= 0 && slotIndex < spellIconImages.Length)
        {
            if (icon != null)
            {
                spellIconImages[slotIndex].sprite = icon;
                spellIconImages[slotIndex].color = Color.white; // Hiển thị rõ màu
            }
            else
            {
                spellIconImages[slotIndex].sprite = null;
                spellIconImages[slotIndex].color = new Color(1, 1, 1, 0); // Ẩn đi (trong suốt) nếu ô đó trống
            }
        }
    }
}