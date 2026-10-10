using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackPool : MonoBehaviour
{
    public static EnemyAttackPool Instance { get; private set; }

    // Từ điển lưu trữ các hàng đợi (Queue), khóa là Prefab gốc
    private Dictionary<GameObject, Queue<GameObject>> poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Hàm đẻ đạn/bẫy
    public GameObject GetFromPool(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(prefab))
        {
            poolDictionary[prefab] = new Queue<GameObject>();
        }

        GameObject objToSpawn;

        if (poolDictionary[prefab].Count > 0)
        {
            objToSpawn = poolDictionary[prefab].Dequeue();
            objToSpawn.transform.position = position;
            objToSpawn.transform.rotation = rotation;
            objToSpawn.SetActive(true);
        }
        else
        {
            objToSpawn = Instantiate(prefab, position, rotation, transform);
        }

        // TỰ ĐỘNG GẮN THẺ: Lưu lại thông tin Prefab gốc vào chính object này
        PooledEnemyAttack pooledObj = objToSpawn.GetComponent<PooledEnemyAttack>();
        if (pooledObj == null)
        {
            pooledObj = objToSpawn.AddComponent<PooledEnemyAttack>();
        }
        pooledObj.originalPrefab = prefab; // Ghi nhớ cội nguồn

        return objToSpawn;
    }

    // Hàm thu hồi (Không cần truyền prefab gốc nữa)
    public void AddToPool(GameObject obj)
    {
        PooledEnemyAttack pooledObj = obj.GetComponent<PooledEnemyAttack>();
        
        // Kiểm tra xem nó có thẻ nhớ Prefab gốc không
        if (pooledObj != null && pooledObj.originalPrefab != null)
        {
            obj.SetActive(false);
            poolDictionary[pooledObj.originalPrefab].Enqueue(obj);
        }
        else
        {
            // Fallback an toàn: Nếu object không thuộc Pool này, hủy nó luôn
            Destroy(obj);
        }
    }
}

// Lớp phụ để dán nhãn lên đạn/bẫy, giúp nhớ Prefab gốc
public class PooledEnemyAttack : MonoBehaviour
{
    [HideInInspector] public GameObject originalPrefab;
}