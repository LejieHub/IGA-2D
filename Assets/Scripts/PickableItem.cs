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
            // ʰȡ�ɹ��������еĴ����ʧ
            gameObject.SetActive(false);
            // ���� Destroy(gameObject);
        }
    }
}
