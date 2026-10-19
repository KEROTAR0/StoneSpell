using UnityEngine;

public class PlayerManaComponent : MonoBehaviour
{
    private float maxMana;
    public float currentMana { get; private set; }
    
    public float manaGainMultiplier = 1f;
    private int hitCountNeedToSpawnMana;
    private int normalHitCount = 0;

    public void Init(float templateMaxMana, int hitCountNeeded)
    {
        this.maxMana = templateMaxMana;
        this.currentMana = Mathf.RoundToInt(maxMana/2); // Bắt đầu với full mana
        this.hitCountNeedToSpawnMana = hitCountNeeded;
        UpdateUI();
    }
    public void IncreaseMaxMana(int mana)
    {
        this.maxMana += mana;
        RestoreMana(mana);
    }
    public void RestoreMana(float amount)
    {
        float actualAmount = amount * manaGainMultiplier;
        currentMana = Mathf.Clamp(currentMana + actualAmount, 0, maxMana);
        UpdateUI();
    }

    public bool TryConsumeMana(float amount)
    {
        if (currentMana >= amount)
        {
            currentMana -= amount;
            UpdateUI();
            return true;
        }
        return false;
    }
    public void RestoreManaPercentage(float percentMultiplier)
    {
        float amountToRestore = maxMana * percentMultiplier;
        RestoreMana(amountToRestore);
    }
    public void RecordNormalHit(Vector3 spawnPosition)
    {
        normalHitCount++;
        if (normalHitCount >= hitCountNeedToSpawnMana)
        {
            normalHitCount = 0;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SpawnMana(spawnPosition);
            }
        }
    }

    private void UpdateUI()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateManaUI(currentMana, maxMana);
    }
}