using UnityEngine;

public class PlayerAfterImageSprite : MonoBehaviour
{
    [SerializeField]
    private float activeTime = 0.1f;
    private float timeActivated;
    private float alpha;

    [SerializeField]
    private float 
        alphaSet = 0.8f,
        alphaMultiplier = 0.85f;

    private Transform player;
    
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer playerSR;

    private Color color;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
            playerSR = player.GetComponent<SpriteRenderer>();
        }
    }
    private void OnEnable()
    {
        alpha = alphaSet;
        // Gán hình ảnh của player vào bóng mờ
        spriteRenderer.sprite = playerSR.sprite;
        
        // Vị trí và góc xoay bây giờ đã được ObjectPoolManager truyền vào từ hàm GetFromPool
        // Nhưng nếu cẩn thận, ta có thể cập nhật lại:
        transform.position = player.position;
        transform.rotation = player.rotation;
        
        timeActivated = Time.time;
    }
    private void Update()
    {
        alpha *= alphaMultiplier; 

        color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;

        if(Time.time >= (timeActivated + activeTime))
        {
            ObjectPoolManager.Instance.AddToPool(gameObject);
        }
    }
}
