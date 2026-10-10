using System.Collections.Generic;
using UnityEngine;

public class SpellPool : MonoBehaviour
{
    public static SpellPool Instance { get; private set; }

    // THAY ĐỔI: Dùng trực tiếp GameObject (Prefab gốc) làm Key thay vì dùng int
    private Dictionary<GameObject, Queue<GameObject>> poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();

    private void Awake()
    {
        Instance = this;
    }

    public GameObject GetFromPool(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        // Kiểm tra xem đã có hàng đợi cho Prefab này chưa
        if (!poolDictionary.ContainsKey(prefab))
        {
            poolDictionary[prefab] = new Queue<GameObject>();
        }

        // Nếu trong Pool có sẵn đạn
        if (poolDictionary[prefab].Count > 0)
        {
            GameObject obj = poolDictionary[prefab].Dequeue();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.SetActive(true);
            return obj;
        }

        // Nếu hết đạn dự trữ, tạo mới
        GameObject newObj = Instantiate(prefab, position, rotation, transform);
        newObj.transform.SetParent(transform);
        
        return newObj;
    }

    public void AddToPool(GameObject obj, GameObject prefabOriginal)
    {
        obj.SetActive(false);
        
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // Xóa gia tốc cũ
        }
        
        // Trả về đúng hàng đợi của Prefab gốc đó
        if (!poolDictionary.ContainsKey(prefabOriginal))
        {
            poolDictionary[prefabOriginal] = new Queue<GameObject>();
        }
        
        poolDictionary[prefabOriginal].Enqueue(obj);
    }
}