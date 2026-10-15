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

        if (baseData.isRotating)
        {
            Transform targetRotate = controller.visualHolder != null ? controller.visualHolder : transform;
            targetRotate.Rotate(Vector3.forward, stats.rotationSpeed * Time.deltaTime);
        }
    }

    public void PerformBounce()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position - (Vector3)currentDirection * 0.5f, currentDirection, 1.5f, GameLayers.WallAndHighObstacleMask);
        controller.SpawnImpactVFXAtPosition(transform.position);
        
        if (hit.collider != null) currentDirection = Vector2.Reflect(currentDirection, hit.normal).normalized;
        else currentDirection = -currentDirection;
            
        SetVelocity(currentDirection);
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