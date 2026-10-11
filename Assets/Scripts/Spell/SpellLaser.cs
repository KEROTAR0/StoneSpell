using UnityEngine;

[RequireComponent(typeof(SpellController), typeof(BoxCollider2D))]
public class SpellLaser : MonoBehaviour
{
    private SpellController controller;
    private BoxCollider2D boxCol;

    private void Awake()
    {
        controller = GetComponent<SpellController>();
        boxCol = GetComponent<BoxCollider2D>();
    }

    public void Init(Vector2 direction)
    {
        if (Player.Instance == null) return;

        transform.SetParent(Player.Instance.transform);
        transform.localPosition = Vector3.zero;
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        float actualLength = controller.spellData.laserMaxLength;
        if (!controller.spellData.isPenetrating)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, controller.spellData.laserMaxLength, controller.wallLayer);
            if (hit.collider != null) actualLength = hit.distance;
        }

        boxCol.size = new Vector2(actualLength, controller.spellData.laserWidth);
        boxCol.offset = new Vector2(actualLength / 2f, 0);
        
        if (controller.visualHolder != null) 
            controller.visualHolder.localPosition = new Vector3(actualLength / 2f, 0, 0);
    }
}