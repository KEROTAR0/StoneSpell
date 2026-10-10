using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    
    // Sử dụng Hàng đợi (Queue) để tái sử dụng đạn
    private Queue<GameObject> availableObj = new Queue<GameObject>();

    public static ProjectilePool Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        GrowPool(10); // Tạo sẵn 10 viên đạn lúc mới vào game
    }

    private void GrowPool(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            var instanceToAdd = Instantiate(projectilePrefab);
            instanceToAdd.transform.SetParent(transform); // Gom vào 1 cục cho gọn Hierarchy
            AddToPool(instanceToAdd);
        }
    }

    public void AddToPool(GameObject instance)
    {
        instance.SetActive(false);
        availableObj.Enqueue(instance);
    }

    // Hàm lấy đạn ra, kèm theo việc thiết lập vị trí và góc xoay
    public GameObject GetFromPool(Vector3 position, Quaternion rotation)
    {
        if (availableObj.Count == 0)
        {
            GrowPool(5); // Nếu hết đạn dự trữ thì tạo thêm 5 viên
        }

        var instance = availableObj.Dequeue();
        instance.transform.position = position;
        instance.transform.rotation = rotation;
        instance.SetActive(true);
        return instance;
    }
}