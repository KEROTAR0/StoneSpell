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
        if (isClone || hasTriggered) return;
        
        // ====================================================
        // CHỐNG NHÂN BẢN: CHỈ VIÊN ĐẠN ĐẦU TIÊN MỚI KÍCH HOẠT GATLING
        // Nếu đạn này là đạn số 1, 2 (phụ) của chùm Spread, bỏ qua ngay!
        // ====================================================
        if (controller.spellIndex != 0) return; 

        if(!controller.baseSpellData.isProjectile) return;
        if(controller.baseSpellData.hasAura && !controller.baseSpellData.isProjectile) return;
        if(controller.baseSpellData.isSelfTargeted) return;

        hasTriggered = true; 

        if (RunManager.Instance != null && controller != null)
        {
            // Truyền hướng ngắm thẳng (rawBaseDirection) vào Gatling thay vì initialDirection (có thể bị xéo do Spread)
            RunManager.Instance.StartCoroutine(FireGatlingRoutine(
                gameObject, 
                transform.position, 
                controller.rawBaseDirection 
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
                // Gatling luôn bắn ra từ miệng súng (vị trí hiện tại của Player)
                currentFirePos = PlayerController.Instance.attackPosition.position;
            }

            GameObject cloneObj = ObjectPoolManager.Instance.GetFromPool(bulletPrefab, currentFirePos, Quaternion.identity);

            if (cloneObj != null)
            {
                cloneObj.name = cloneObj.name.Replace("(Clone)", "").Trim() + "(Clone)";
                cloneObj.SetActive(true);

                if (cloneObj.TryGetComponent(out SpellController cloneController))
                {
                    if (cloneObj.TryGetComponent(out SpellGatlingTrait trait))
                    {
                        trait.isClone = true;
                        trait.hasTriggered = true; 
                    }

                    // Gán index = -1 cho đạn Gatling để nó biết nó là đạn cộng thêm và truyền hướng bay THẲNG (rawDirection)
                    cloneController.Initialize(rawDirection, -1, 1, rawDirection);
                }
            }
        }
    }
}