using UnityEngine;

[RequireComponent(typeof(SpellController), typeof(SpellMovement))]
public class SpellChasingTrait : MonoBehaviour
{
    private SpellController controller;
    private SpellMovement movement;
    
    private Transform target;
    private float delayTimer;
    private bool isHomingActive = false;

    private void Awake()
    {
        controller = GetComponent<SpellController>();
        movement = GetComponent<SpellMovement>();
    }

    // OnEnable chạy mỗi khi lấy từ Object Pool ra
    private void OnEnable()
    {
        target = null;
        isHomingActive = false;
        
        // Trì hoãn việc đọc Data một chút xíu (đến Update) để đảm bảo SpellController đã Init xong
        delayTimer = 0f; 
    }

    private void Update()
    {
        // 1. Chốt chặn an toàn
        if (controller.baseSpellData == null || !controller.baseSpellData.isHoming) return;
        if (!controller.baseSpellData.isProjectile) return; // Chỉ áp dụng cho đạn bay

        // 2. Chờ đếm ngược Delay
        if (delayTimer < controller.baseSpellData.homingDelayTimer)
        {
            delayTimer += Time.deltaTime;
            return;
        }

        isHomingActive = true;

        // 3. Tìm mục tiêu nếu chưa có hoặc mục tiêu đã chết
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            FindNearestTarget();
        }

        // 4. Thực hiện bẻ lái (Homing)
        if (target != null)
        {
            SteerTowardsTarget();
        }
    }

    private void FindNearestTarget()
    {
        // Quét tìm quái vật và đồ vật có thể phá hủy trong bán kính
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, controller.baseSpellData.homingDetectionRadius, GameLayers.EnemyAndDamageableMask);
        
        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (var hit in hitColliders)
        {
            // Bỏ qua thùng gỗ nếu bạn chỉ muốn đạn đuổi theo Quái (bỏ comment dòng dưới)
            // if (hit.CompareTag("Destructible")) continue;

            float distanceToEnemy = Vector2.Distance(transform.position, hit.transform.position);
            
            // Tùy chọn: Thêm Raycast để đảm bảo mục tiêu không bị khuất sau tường
            RaycastHit2D wallCheck = Physics2D.Raycast(transform.position, (hit.transform.position - transform.position).normalized, distanceToEnemy, GameLayers.WallAndHighObstacleMask);
            
            if (wallCheck.collider == null && distanceToEnemy < closestDistance)
            {
                closestDistance = distanceToEnemy;
                closestEnemy = hit.transform;
            }
        }

        target = closestEnemy;
    }

    private void SteerTowardsTarget()
    {
        // Tính toán hướng bay mong muốn (chỉ thẳng vào mặt mục tiêu)
        Vector2 desiredDirection = ((Vector2)target.position - (Vector2)transform.position).normalized;

        // Lấy hướng bay hiện tại từ SpellMovement
        Vector2 currentDir = movement.currentDirection;

        // Dùng RotateTowards để xoay vector hiện tại về phía vector mong muốn từ từ theo TurnSpeed
        float maxRadiansDelta = controller.baseSpellData.homingTurnSpeed * Mathf.Deg2Rad * Time.deltaTime;
        Vector2 newDirection = Vector3.RotateTowards(currentDir, desiredDirection, maxRadiansDelta, 0f);

        // Trả hướng mới về cho SpellMovement để nó bay và bẻ hình ảnh viên đạn
        movement.ReflectTo(newDirection.normalized);
    }
    
    // Vẽ vòng tròn tầm nhìn trong Editor để dễ canh chỉnh
    private void OnDrawGizmosSelected()
    {
        if (controller != null && controller.baseSpellData != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, controller.baseSpellData.homingDetectionRadius);
        }
    }
}