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
        D_Spell data = controller.spellData;
        
        float randomAngle = Random.Range(-data.inaccuracy, data.inaccuracy);
        currentDirection = (Quaternion.Euler(0, 0, randomAngle) * direction.normalized).normalized;
        
        currentSpeed = data.isPenetrating ? 0f : data.moveSpeed;
        hasFinishedAttackFrame = false;

        if (data.isProjectile || data.isPenetrating) SetVelocity(currentDirection);
        else rb.linearVelocity = Vector2.zero;
    }

    private void Update()
    {
        D_Spell data = controller.spellData;

        if (data.isPenetrating && !data.isLaser)
        {
            // Thay vì Check isHitboxActive trực tiếp, ta dựa vào cờ FinishAttackFrame
            if (!hasFinishedAttackFrame) currentSpeed += data.acceleration * Time.deltaTime;
            else currentSpeed -= data.acceleration * Time.deltaTime;

            currentSpeed = Mathf.Max(0, currentSpeed);
            SetVelocity(currentDirection * currentSpeed);
        }

        if (data.isRotating)
        {
            Transform targetRotate = controller.visualHolder != null ? controller.visualHolder : transform;
            targetRotate.Rotate(Vector3.forward, data.rotationSpeed * Time.deltaTime);
        }
    }

    public void PerformBounce()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position - (Vector3)currentDirection * 0.5f, currentDirection, 1.5f, controller.wallLayer | controller.highObstacleLayer);
        controller.SpawnImpactVFX();
        
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
        float speedToUse = (controller.spellData.isPenetrating && !controller.spellData.isLaser) ? currentSpeed : controller.spellData.moveSpeed;
        rb.linearVelocity = dir * speedToUse;
        
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }
    
    public void SetFinishedAttackFrame(bool state) => hasFinishedAttackFrame = state;
}