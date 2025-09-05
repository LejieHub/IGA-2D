// ItemData.cs
using UnityEngine;

public enum ItemType { SocialMask, BasketBall, GameConsole, Abacus, Guitar }

[CreateAssetMenu(menuName = "Items/ItemData")]
public class ItemData : ScriptableObject
{
    public ItemType type;
    public string displayName;
    public Sprite icon;

    [Header("Handheld Prefab (mini)")]
    public GameObject handheldPrefab;  // 手持小模型

    [Header("Effect Params (接口预留)")]
    public float valueA;
    public float valueB;
    // …将来扩展

    // 预留：统一入口（比如捡起时给玩家加Buff/修改属性等）
    public virtual void OnEquip(PlayerInventory inv) { }
    public virtual void OnUnequip(PlayerInventory inv) { }
}
