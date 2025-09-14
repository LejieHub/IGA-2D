using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Hand Anchor")]
    public Transform handAnchor;         // 指向 Player 下的空物体
    public bool flipWithPlayer = true;   // 跟随朝向翻转

    [Header("Current")]
    public ItemData currentItem;         // 当前持有的数据
    public GameObject currentHandheld;   // 当前手持小模型实例

    [Header("Throwable Defaults")]
    public bool defaultUpgraded = false;  // 可选：给会丢掷的手持一个默认升级状态

    // 拾取入口（单持）
    public bool Pick(ItemData item)
    {
        if (item == null) return false;

        Unequip();            // 如果已有物品：先卸下
        currentItem = item;
        Equip(item);
        return true;
    }

    public void Equip(ItemData item)
    {
        // 生成手持模型
        if (item.handheldPrefab != null && handAnchor != null)
        {
            currentHandheld = Instantiate(item.handheldPrefab, handAnchor);
            currentHandheld.transform.localPosition = Vector3.zero;
            currentHandheld.transform.localRotation = Quaternion.identity;
            // 缩放可在 prefab 就设好

            // 如果这个手持支持丢掷，则注入玩家与锚点
            var throwable = currentHandheld.GetComponent<ThrowableHandheld>();
            if (throwable != null)
            {
                // 把“玩家 Transform”和“手持锚点”注入；第三个参数可由 ItemData 决定
                throwable.Setup(playerTransform: transform, handAnchor: handAnchor, upgraded: false);
            }
        }

        // 触发数据层效果（可为空实现）
        item.OnEquip(this);
    }

    public void Unequip()
    {
        if (currentHandheld != null)
        {
            Destroy(currentHandheld);
            currentHandheld = null;
        }

        if (currentItem != null)
        {
            currentItem.OnUnequip(this);
            currentItem = null;
        }
    }

    // 跟随角色朝向翻转手持道具（如果需要）
    public void SyncFlip(bool facingRight)
    {
        if (!flipWithPlayer || currentHandheld == null) return;
        var s = currentHandheld.transform.localScale;
        s.x = Mathf.Abs(s.x) * (facingRight ? 1 : -1);
        currentHandheld.transform.localScale = s;
    }
}
