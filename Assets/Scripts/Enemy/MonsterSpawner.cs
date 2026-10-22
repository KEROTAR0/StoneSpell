using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Cấu hình Lồng Ấp")]
    [Tooltip("Danh sách các loại quái vật có thể sinh ra từ lồng này")]
    public GameObject[] enemyPrefabs;
    
    [Tooltip("Thời gian chờ giữa mỗi lần đẻ quái (giây)")]
    public float spawnInterval = 3f;
    
    [Tooltip("Số lượng quái vật TỐI ĐA được phép tồn tại cùng lúc từ lồng này")]
    public int maxActiveEnemies = 5;
    
    [Tooltip("Tổng số quái đẻ ra trước khi lồng tự hủy (-1 là đẻ vô hạn)")]
    public int totalEnemiesToSpawn = -1; 
    
    [Tooltip("Bán kính đẻ quái quanh lồng")]
    public float spawnRadius = 3f;
    
    [Header("Hiệu ứng")]
    public GameObject spawnVFX;

    private float timer;
    private int spawnedCount = 0;
    
    // Danh sách theo dõi các con quái đang sống
    private List<GameObject> activeEnemies = new List<GameObject>();

    private void OnEnable()
    {
        timer = spawnInterval; // Khởi tạo đếm ngược
        spawnedCount = 0;
        activeEnemies.Clear();
    }

    private void Update()
    {
        // 1. DỌN DẸP DANH SÁCH: Tự động loại bỏ những con quái đã chết (bị tắt active)
        activeEnemies.RemoveAll(e => e == null || !e.activeInHierarchy);

        // 2. KIỂM TRA GIỚI HẠN TỔNG
        if (totalEnemiesToSpawn > 0 && spawnedCount >= totalEnemiesToSpawn)
        {
            // Nếu đã đẻ đủ số lượng và tất cả quái đã chết -> Tự tắt/Hủy lồng
            if (activeEnemies.Count == 0) 
            {
                gameObject.SetActive(false); 
            }
            return;
        }

        // 3. ĐẾM NGƯỢC VÀ ĐẺ QUÁI
        timer -= Time.deltaTime;
        
        // Chỉ đẻ thêm nếu số lượng trên sân chưa vượt mức cho phép
        if (timer <= 0f && activeEnemies.Count < maxActiveEnemies)
        {
            SpawnEnemy();
            timer = spawnInterval;
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;

        // Bốc ngẫu nhiên 1 loại quái trong danh sách
        GameObject prefabToSpawn = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        // Tính toán vị trí ngẫu nhiên trong vòng tròn bán kính
        Vector2 randomPos = (Vector2)transform.position + Random.insideUnitCircle * spawnRadius;

        // (Tùy chọn) Bắn Raycast kiểm tra xem vị trí đẻ có bị kẹt vào tường không
        // Nếu kẹt, dời nó về đúng tâm của lồng
        Collider2D wallCheck = Physics2D.OverlapPoint(randomPos, GameLayers.AllObstaclesMask);
        if (wallCheck != null)
        {
            randomPos = transform.position;
        }

        // Tạo hiệu ứng Spawn
        if (spawnVFX != null && ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.GetFromPool(spawnVFX, randomPos, Quaternion.identity);
        }

        // Tạo Quái vật qua Object Pool
        if (ObjectPoolManager.Instance != null)
        {
            GameObject enemy = ObjectPoolManager.Instance.GetFromPool(prefabToSpawn, randomPos, Quaternion.identity);
            
            // Đưa vào danh sách theo dõi
            activeEnemies.Add(enemy);
            spawnedCount++;
        }
    }

    // Vẽ vòng tròn màu đỏ trong Editor để bạn dễ dàng căn chỉnh bán kính đẻ quái
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}