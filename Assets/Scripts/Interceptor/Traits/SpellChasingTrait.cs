using UnityEngine;

[RequireComponent(typeof(SpellController))]
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
        if (controller.baseSpellData == null || !controller.baseSpellData.isHoming) return;

        if (delayTimer < controller.baseSpellData.homingDelayTimer)
        {
            delayTimer += Time.deltaTime;
            return;
        }

        isHomingActive = true;

        if (target == null || !target.gameObject.activeInHierarchy)
        {
            FindNearestTarget();
        }

        if (target != null)
        {
            SteerTowardsTarget();
        }
    }

    private void FindNearestTarget()
    {
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
        // 1. PHÁ VỠ QUỸ ĐẠO (Dành riêng cho Quả cầu xoay quanh người)
        // Khi phát hiện mục tiêu, lập tức ngắt liên kết bám theo Player
        controller.isOrbitingOrb = false;

        Vector2 desiredDirection = ((Vector2)target.position - (Vector2)transform.position).normalized;

        // 2. NẾU LÀ ĐẠN BẮN BÌNH THƯỜNG (Có gia tốc, có Movement)
        // (Quả cầu Orbiting do chưa Init Movement nên speed = 0, sẽ tự rẽ xuống nhánh số 3)
        if (movement != null && movement.currentSpeed > 0)
        {
            Vector2 currentDir = movement.currentDirection;
            if (currentDir == Vector2.zero) currentDir = desiredDirection; 
            
            float maxRadiansDelta = controller.baseSpellData.homingTurnSpeed * Mathf.Deg2Rad * Time.deltaTime;
            Vector2 newDirection = Vector3.RotateTowards(currentDir, desiredDirection, maxRadiansDelta, 0f);
            
            movement.ReflectTo(newDirection.normalized);
        }
        // 3. NẾU LÀ QUẢ CẦU VỪA TÁCH KHỎI NGƯỜI, BÃI MÌN HOẶC CỘT SÉT
        else
        {
            // Lấy tốc độ: Nếu Data có tốc độ gốc thì dùng, không thì mượn Homing Turn Speed làm vận tốc
            float speed = controller.baseSpellData.moveSpeed > 0 
                ? controller.baseSpellData.moveSpeed 
                : (controller.baseSpellData.homingTurnSpeed / 50f);
                
            // NẾU LÀ BÃI MÌN (Mặc định moveSpeed = 0):
            // Ép cho nó một tốc độ mặc định (VD: 5f) để nó biết "bò" tới chỗ mục tiêu
            if (speed <= 0) speed = 5f; 

            // Dịch chuyển thẳng tới mục tiêu bất chấp nó là vật thể gì
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        }
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