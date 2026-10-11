using System.Collections;
using UnityEngine;

public class ManaShard : MonoBehaviour
{
    [SerializeField] private float manaAmount = 30f;
    [SerializeField] private float spinSpeed = 360f; 
    
    private bool isVacuuming = false;
    private bool isCollected = false;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        isVacuuming = false;
        isCollected = false;
        transform.localScale = Vector3.one;
        
        if (rb != null)
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            rb.AddForce(randomDir * 3f, ForceMode2D.Impulse);
        }
    }

    private void Update()
    {
        transform.Rotate(0, 0, spinSpeed * Time.deltaTime);

        if (Player.Instance == null || isCollected) return;

        if (!isVacuuming)
        {
            float distance = Vector2.Distance(transform.position, Player.Instance.transform.position);
            if (distance <= Player.Instance.playerData.manaCollectionRadius || Player.Instance.isGlobalVacuum)
            {
                isVacuuming = true;
                if (rb != null) rb.linearVelocity = Vector2.zero; 
            }
        }

        if (isVacuuming)
        {
            transform.position = Vector3.MoveTowards(transform.position, Player.Instance.transform.position, Player.Instance.playerData.manaVacuumSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isCollected)
        {
            isCollected = true;
            StartCoroutine(CollectRoutine());
        }
    }

    private IEnumerator CollectRoutine()
    {
        Player.Instance.AddMana(manaAmount);
        
        // Tăng tốc độ xoay và thu nhỏ dần trong 0.15s
        float timer = 0;
        float duration = 0.15f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            spinSpeed = 1500f;
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, timer / duration);
            yield return null;
        }

        spinSpeed = 360f;
        ObjectPoolManager.Instance.AddToPool(gameObject);
    }
}