using UnityEngine;
using System.Collections;

public class SpellGatlingTrait : MonoBehaviour
{
    private SpellController controller;
    public bool isClone = false;
    private bool hasTriggered = false;

    private void Awake()
    {
        controller = GetComponent<SpellController>();
    }

    // Hàm này tự động chạy MỖI LẦN viên đạn được lôi ra từ kho (Object Pool)
    private void OnEnable()
    {
        // Phải reset sạch sẽ cờ báo hiệu, để nó như một viên đạn mới tinh
        hasTriggered = false;
        isClone = false; 
    }

    private void Update()
    {
        // Chốt chặn: Nếu là đạn con, hoặc đã nhả đạn rồi thì bỏ qua
        if (isClone || hasTriggered) return;
        if( !controller.baseSpellData.isProjectile || 
            controller.baseSpellData.hasAura || 
            controller.baseSpellData.isSelfTargeted) return;

        hasTriggered = true; // Khóa chốt ngay lập tức để không nhả đạn ở frame tiếp theo

        if (RunManager.Instance != null && controller != null)
        {
            RunManager.Instance.StartCoroutine(FireGatlingRoutine(
                gameObject, 
                transform.position, 
                controller.initialDirection
            ));
        }
    }

    private IEnumerator FireGatlingRoutine(GameObject bulletPrefab, Vector2 spawnPosition, Vector2 rawDirection)
    {
        int maxProjectiles = 5; 
        float fireDelay = 0.12f; 

        for (int i = 0; i < maxProjectiles; i++)
        {
            yield return new WaitForSeconds(fireDelay);

            Vector2 currentFirePos = spawnPosition;
            if (PlayerController.Instance != null)
            {
                currentFirePos = PlayerController.Instance.transform.position;
            }

            GameObject cloneObj = ObjectPoolManager.Instance.GetFromPool(bulletPrefab, currentFirePos, Quaternion.identity);

            if (cloneObj != null)
            {
                cloneObj.name = cloneObj.name.Replace("(Clone)", "").Trim() + "(Clone)";
                cloneObj.SetActive(true);

                if (cloneObj.TryGetComponent(out SpellController cloneController))
                {
                    // Dán nhãn Clone để nó không đẻ tiếp
                    if (cloneObj.TryGetComponent(out SpellGatlingTrait trait))
                    {
                        trait.isClone = true;
                        trait.hasTriggered = true; 
                    }

                    // 2. Lúc này viên đạn đã sẵn sàng 100%, truyền lệnh sẽ không bao giờ lỗi
                    cloneController.Initialize(rawDirection);
                }
            }
        }
    }
}