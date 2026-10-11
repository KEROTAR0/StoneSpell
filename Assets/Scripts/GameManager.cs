using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Global Prefabs")]
    [SerializeField] private GameObject manaShardPrefab;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject playerNormalAttackPrefab;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Hàm gọi sinh Mana
    public void SpawnMana(Vector3 position)
    {
        GameObject mana = ObjectPoolManager.Instance.GetFromPool(manaShardPrefab, position, Quaternion.identity);
        
        mana.transform.SetParent(null);
        mana.transform.position = position;
    }

    // Bạn có thể làm tương tự cho vàng, máu, đạn của player...
    public GameObject SpawnPlayerNormalAttack(Vector3 position, Quaternion rotation)
    {
        return ObjectPoolManager.Instance.GetFromPool(playerNormalAttackPrefab, position, rotation);
    }
}

