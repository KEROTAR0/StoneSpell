using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    // Từ điển lưu trữ các hàng đợi (Queue), khóa là Prefab gốc
    private Dictionary<GameObject, Queue<GameObject>> poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Hàm tạo sẵn (Pre-warm) một số lượng object nhất định lúc mới vào game
    public void Prewarm(GameObject prefab, int amount)
    {
        if (!poolDictionary.ContainsKey(prefab))
        {
            poolDictionary[prefab] = new Queue<GameObject>();
        }

        for (int i = 0; i < amount; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            
            // Dán nhãn cội nguồn
            PooledObject pooledObj = obj.AddComponent<PooledObject>();
            pooledObj.originalPrefab = prefab;

            poolDictionary[prefab].Enqueue(obj);
        }
    }

    // Hàm đẻ Object (Dùng chung cho Đạn, Mana, Bóng mờ, Bẫy, Spell...)
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
            // Nếu hết hàng dự trữ thì tạo mới
            objToSpawn = Instantiate(prefab, position, rotation, transform);
            
            // TỰ ĐỘNG GẮN THẺ: Lưu lại thông tin Prefab gốc vào chính object này
            PooledObject pooledObj = objToSpawn.AddComponent<PooledObject>();
            pooledObj.originalPrefab = prefab;
        }

        return objToSpawn;
    }

    // Hàm thu hồi chung (Chỉ cần truyền object, tự nó biết đường về đúng hàng đợi)
    public void AddToPool(GameObject obj)
    {
        PooledObject pooledObj = obj.GetComponent<PooledObject>();
        
        if (pooledObj != null && pooledObj.originalPrefab != null)
        {
            obj.SetActive(false);

            // Reset gia tốc vật lý (nếu có)
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            poolDictionary[pooledObj.originalPrefab].Enqueue(obj);
        }
        else
        {
            // Fallback: Nếu không phải object từ Pool, hủy luôn
            Destroy(obj);
        }
    }
}
