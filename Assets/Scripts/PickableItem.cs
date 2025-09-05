// PickableItem.cs
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PickableItem : MonoBehaviour
{
    public ItemData data;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var inv = other.GetComponentInParent<PlayerInventory>();
        if (inv == null) return;

        if (inv.Pick(data))
        {
            // 拾取成功：场景中的大件消失
            gameObject.SetActive(false);
            // 或者 Destroy(gameObject);
        }
    }
}
