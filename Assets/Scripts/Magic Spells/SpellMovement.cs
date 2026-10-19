using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpellController))]
public class SpellMovement : MonoBehaviour
{
    private SpellController controller;
    private Rigidbody2D rb;
    
    public Vector2 currentDirection { get; private set; }
    public float currentSpeed { get; private set; }
    private bool hasFinishedAttackFrame = false; 

    private void Awake()
    {
        controller = GetComponent<SpellController>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(Vector2 direction)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (controller == null) controller = GetComponent<SpellController>();
        
        D_Spell baseData = controller.baseSpellData;
        RuntimeSpellStats stats = controller.currentStats;
        
        float randomAngle = Random.Range(-baseData.inaccuracy, baseData.inaccuracy);
        currentDirection = (Quaternion.Euler(0, 0, randomAngle) * direction.normalized).normalized;
        
        // ĐỌC TỐC ĐỘ TỪ BẢN SAO ĐỘNG
        currentSpeed = baseData.isPenetrating ? 0f : stats.moveSpeed;
        hasFinishedAttackFrame = false;

        if (baseData.isProjectile || baseData.isPenetrating) 
            SetVelocity(currentDirection);
        else rb.linearVelocity = Vector2.zero;
    }

    private void Update()
    {
        D_Spell baseData = controller.baseSpellData;
        RuntimeSpellStats stats = controller.currentStats;

        if (baseData.isPenetrating && !baseData.isLaser)
        {
            if (!hasFinishedAttackFrame) currentSpeed += stats.acceleration * Time.deltaTime;
            else currentSpeed -= stats.acceleration * Time.deltaTime;

            currentSpeed = Mathf.Max(0, currentSpeed);
            SetVelocity(currentDirection * currentSpeed);
        }

        if (baseData.isRotating && !baseData.isSpawnAtMouse)
        {
            Transform targetRotate = controller.visualHolder != null ? controller.visualHolder : transform;
            targetRotate.Rotate(Vector3.forward, stats.rotationSpeed * Time.deltaTime);
        }
    }
    private void FixedUpdate()
    {
        // CHỐNG XUYÊN TƯỜNG (RAYCAST AHEAD)
        if (rb != null && controller != null && controller.baseSpellData.isProjectile)
        {
            // 1. CHỐNG XUYÊN TƯỜNG (RAYCAST AHEAD)
            float distanceThisFrame = controller.currentStats.moveSpeed * Time.fixedDeltaTime;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, currentDirection, distanceThisFrame, GameLayers.WallMask.value | GameLayers.HighObstacleMask.value);
            
            if (hit.collider != null)
            {
                // Dời vị trí đến sát mặt tường nhưng LÙI LẠI MỘT CHÚT (0.05f) để không bị lún vào Collider
                transform.position = hit.point - currentDirection * 0.05f; 
                
                // TUYỆT ĐỐI KHÔNG gán rb.linearVelocity = Vector2.zero ở đây nữa!
                // Giữ nguyên vận tốc để OnTriggerEnter2D có dữ liệu tính toán góc phản xạ.
            }
            else
            {
                rb.linearVelocity = currentDirection * controller.currentStats.moveSpeed;
            }
        }
    }
    public void PerformBounce(Vector2 surfaceNormal)
    {
        // 1. Tính toán góc phản xạ (Góc tới = Góc nảy)
        // Vector2.Reflect sẽ lấy hướng bay hiện tại, đập vào mặt phẳng (surfaceNormal) và trả ra hướng nảy
        currentDirection = Vector2.Reflect(currentDirection, surfaceNormal).normalized;

        // 2. Gán lại vận tốc mới cho viên đạn
        if (rb != null)
        {
            rb.linearVelocity = currentDirection * controller.currentStats.moveSpeed;
        }

        // 3. (Tùy chọn) Xoay hình ảnh viên đạn hướng theo góc bay mới
        float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public void ReflectTo(Vector2 newDir)
    {
        currentDirection = newDir;
        SetVelocity(currentDirection);
    }

    private void SetVelocity(Vector2 dir)
    {
        float speedToUse = (controller.baseSpellData.isPenetrating && !controller.baseSpellData.isLaser) ? currentSpeed : controller.currentStats.moveSpeed;
        rb.linearVelocity = dir * speedToUse;
        
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    public void SetFinishedAttackFrame(bool state) => hasFinishedAttackFrame = state;
}