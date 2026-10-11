using UnityEngine;

[RequireComponent(typeof(Animator))]
public class VFXAnimationDespawn : MonoBehaviour
{
    private GameObject originalPrefab;

    // HÀM NÀY SẼ ĐƯỢC GỌI BỞI ANIMATION EVENT TẠI FRAME CUỐI CÙNG
    public void OnAnimationComplete()
    {
        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.AddToPool(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}