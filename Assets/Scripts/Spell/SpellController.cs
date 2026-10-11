using System.Collections.Generic;
using UnityEngine;

public class SpellController : MonoBehaviour
{
    public D_Spell spellData;
    public Transform visualHolder;

    public LayerMask targetLayer { get; set; }
    public LayerMask wallLayer { get; private set; }
    public LayerMask highObstacleLayer { get; private set; }
    public LayerMask enemyProjectileLayer { get; private set; }

    public List<SpellEffect> activeEffects { get; private set; } = new List<SpellEffect>();
    private float timeActivated;
    
    // Biến dùng riêng cho Self-Target (Buff)
    private float auraTimer = 0f; 

    public void Initialize(LayerMask target, LayerMask wall, LayerMask highObs, LayerMask enemyProj, Vector2 direction)
    {
        this.targetLayer = target;
        this.wallLayer = wall;
        this.highObstacleLayer = highObs;
        this.enemyProjectileLayer = enemyProj;

        activeEffects.Clear();
        if (spellData.hitEffects != null)
        {
            activeEffects.AddRange(spellData.hitEffects);
        }

        timeActivated = Time.time;
        auraTimer = spellData.auraTickRate; // Đặt sẵn bằng tickRate để bơm ngay lập tức ở frame đầu tiên

        // KHÔI PHỤC LOGIC SELF-TARGET Ở ĐÂY
        if (spellData.isSelfTargeted && Player.Instance != null)
        {
            // Bám vào Player
            transform.SetParent(Player.Instance.transform);
            transform.localPosition = Vector3.zero;
            
            // Tắt vật lý (không cần bay hay va chạm)
            if (TryGetComponent(out Rigidbody2D rb))
            {
                rb.linearVelocity = Vector2.zero;
                rb.simulated = false;
            }
            if (TryGetComponent(out Collider2D col)) col.enabled = false;
            
            // Không cần báo thức cho Movement/Collision vì buff không cần bay
        }
        else
        {
            // Báo thức cho đạn bay bình thường
            if (TryGetComponent(out SpellMovement movement)) movement.Init(direction);
            if (TryGetComponent(out SpellCollision collision)) collision.Init();
            if (TryGetComponent(out SpellLaser laser)) laser.Init(direction);
            if (TryGetComponent(out SpellAura aura)) aura.Init();
        }
    }

    private void Update()
    {
        // LOGIC BƠM HIỆU ỨNG LIÊN TỤC (Dùng cho Buff / Mana Regen)
        if (spellData.isSelfTargeted && Player.Instance != null && spellData.auraTickRate > 0)
        {
            auraTimer += Time.deltaTime;
            if (auraTimer >= spellData.auraTickRate)
            {
                auraTimer = 0f;
                // Bơm lại toàn bộ các Effect (kể cả Mana Regen) lên người Player
                ApplyEffectsToTarget(Player.Instance.gameObject);
            }
        }

        if (Time.time >= timeActivated + spellData.lifeTime)
        {
            Despawn();
        }
    }

    public void ApplyEffectsToTarget(GameObject target)
    {
        foreach (var effect in activeEffects)
        {
            effect.ApplyEffect(target, transform.position);
        }
    }

    public void SpawnImpactVFX()
    {
        if (spellData.impactVFX != null)
            Instantiate(spellData.impactVFX, transform.position, Quaternion.identity);
    }

    public void ReflectedByPlayer(Vector2 currentDir)
    {
        // Tối ưu hóa: Đảo Layer cực nhanh
        gameObject.layer = GameLayers.PlayerAttackLayerIndex;
        targetLayer = GameLayers.EnemyAndDamageableMask; 
        
        if (TryGetComponent(out SpellMovement movement)) movement.ReflectTo(-currentDir);
    }    public void Despawn()
    {
        if (spellData.isSelfTargeted && Player.Instance != null)
        {
            foreach (var effect in activeEffects)
                effect.RemoveEffect(Player.Instance.gameObject);
        }

        transform.SetParent(null);
        if (TryGetComponent(out Rigidbody2D rb)) rb.simulated = true;

        if (ObjectPoolManager.Instance != null)
            ObjectPoolManager.Instance.AddToPool(gameObject);
        else
            Destroy(gameObject);
    }
}