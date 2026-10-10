using UnityEngine;

[RequireComponent(typeof(Animator))]
public class VFXAnimationDespawn : MonoBehaviour
{
    private GameObject originalPrefab;

    // Chỉ cần lưu Prefab gốc để biết đường trả về Pool
    public void Setup(GameObject prefab)
    {
        originalPrefab = prefab;
    }

    // HÀM NÀY SẼ ĐƯỢC GỌI BỞI ANIMATION EVENT TẠI FRAME CUỐI CÙNG
    public void OnAnimationComplete()
    {
        if (SpellPool.Instance != null && originalPrefab != null)
        {
            SpellPool.Instance.AddToPool(gameObject, originalPrefab);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}