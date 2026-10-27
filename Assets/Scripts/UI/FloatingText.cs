using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshPro))]
public class FloatingText : MonoBehaviour
{
    private TextMeshPro textMesh;
    private Color textColor;
    private float fadeTimer;
    
    [Header("Cấu hình Animation")]
    public float moveSpeed = 1.5f;
    public float fadeSpeed = 3f;
    public float lifeTime = 0.4f; // Đứng rõ nét trong 0.4 giây trước khi mờ

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    public void Setup(int damage, Color color)
    {
        textMesh.text = damage.ToString();
        textColor = color;
        textMesh.color = textColor;
        fadeTimer = lifeTime;
    }

    private void Update()
    {
        // Nhích dần lên trên
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        fadeTimer -= Time.deltaTime;
        if (fadeTimer <= 0)
        {
            // Mờ dần Alpha
            textColor.a -= fadeSpeed * Time.deltaTime;
            textMesh.color = textColor;

            // Thu hồi khi đã trong suốt
            if (textColor.a <= 0)
            {
                if (ObjectPoolManager.Instance != null)
                    ObjectPoolManager.Instance.AddToPool(gameObject);
                else
                    Destroy(gameObject);
            }
        }
    }
}