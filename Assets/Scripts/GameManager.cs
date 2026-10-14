using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Global Prefabs")]
    [SerializeField] private GameObject manaShardPrefab;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject playerNormalAttackPrefab;

    [Header("Global Status VFX")]
    [SerializeField] private GameObject stunVfxPrefab;
    [SerializeField] private GameObject sleepVfxPrefab;
    [SerializeField] private GameObject healVfxPrefab;
    [SerializeField] private GameObject slowVfxPrefab;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ==========================================
    // CÁC HÀM SPAWN VFX DÙNG CHUNG
    // ==========================================
    public GameObject SpawnStunVFX(Transform parent, float heightOffset)
    {
        if (stunVfxPrefab == null) return null;
        Vector3 spawnPos = parent.position + Vector3.up * heightOffset;
        GameObject vfx = ObjectPoolManager.Instance.GetFromPool(stunVfxPrefab, spawnPos, Quaternion.identity);
        vfx.transform.SetParent(parent);
        return vfx;
    }
    public GameObject SpawnSleepVFX(Transform parent, float heightOffset)
    {
        if (sleepVfxPrefab == null) return null;
        Vector3 spawnPos = parent.position + Vector3.up * heightOffset;
        GameObject vfx = ObjectPoolManager.Instance.GetFromPool(sleepVfxPrefab, spawnPos, Quaternion.identity);
        vfx.transform.SetParent(parent);
        return vfx;
    }
    public void SpawnHealVFX(Transform parent)
    {
        if (healVfxPrefab == null) return;
        GameObject vfx = ObjectPoolManager.Instance.GetFromPool(healVfxPrefab, parent.position, Quaternion.identity);
        vfx.transform.SetParent(parent);
        // Heal thường là vfx nổ 1 lần (Particle tự tắt và tự trả về pool), nên không cần return GameObject.
    }

    // ==========================================
    // CÁC HÀM SPAWN VẬT PHẨM VÀ ĐẠN
    // ==========================================
    public void SpawnMana(Vector3 position)
    {
        GameObject mana = ObjectPoolManager.Instance.GetFromPool(manaShardPrefab, position, Quaternion.identity);
        mana.transform.SetParent(null);
    }
    public GameObject SpawnPlayerNormalAttack(Vector3 position, Quaternion rotation)
    {
        return ObjectPoolManager.Instance.GetFromPool(playerNormalAttackPrefab, position, rotation);
    }
}

