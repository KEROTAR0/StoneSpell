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
    [SerializeField] private GameObject hasteVfxPrefab;
    [SerializeField] private GameObject freezeVfxPrefab;
    [SerializeField] private GameObject markedVfxPrefab;
    [SerializeField] private GameObject shieldVfxPrefab;

    [Header("Global UI VFX")]
    public GameObject damageTextPrefab;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ==========================================
    // CÁC HÀM SPAWN VFX DÙNG CHUNG
    // ==========================================
    public void SpawnDamageText(Vector3 position, int damage, Color color)
    {
        if (damageTextPrefab == null || ObjectPoolManager.Instance == null) return;
        
        // Random ra xung quanh một chút để các con số không bị đè xếp lớp lên nhau nếu dính 5-6 hit cùng lúc
        Vector3 randomOffset = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(0.2f, 0.6f), 0);
        
        GameObject textObj = ObjectPoolManager.Instance.GetFromPool(damageTextPrefab, position + randomOffset, Quaternion.identity);
        
        if (textObj.TryGetComponent(out FloatingText floatingText))
        {
            floatingText.Setup(damage, color);
        }
    }
    public GameObject SpawnSpeedVFX(Transform parent, float multiplier, float heightOffset)
    {
        GameObject prefabToSpawn = null;
        if (multiplier == 0f) prefabToSpawn = freezeVfxPrefab;
        else if (multiplier < 1f) prefabToSpawn = slowVfxPrefab;
        else if (multiplier > 1f) prefabToSpawn = hasteVfxPrefab;

        if (prefabToSpawn == null) return null;

        Vector3 spawnPos = parent.position + Vector3.up * heightOffset;
        GameObject vfx = ObjectPoolManager.Instance.GetFromPool(prefabToSpawn, spawnPos, Quaternion.identity);
        vfx.transform.SetParent(parent);
        return vfx;
    }
    public GameObject SpawnDamageMultiplierVFX(Transform parent, float multiplier, float heightOffset)
    {
        GameObject prefabToSpawn = null;
        
        // Value = 1 thì prefabToSpawn vẫn là null (Không hiện gì)
        if (multiplier > 1f) prefabToSpawn = markedVfxPrefab;
        else if (multiplier < 1f) prefabToSpawn = shieldVfxPrefab;

        if (prefabToSpawn == null) return null;

        Vector3 spawnPos = parent.position + Vector3.up * heightOffset;
        GameObject vfx = ObjectPoolManager.Instance.GetFromPool(prefabToSpawn, spawnPos, Quaternion.identity);
        vfx.transform.SetParent(parent);
        return vfx;
    }
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

