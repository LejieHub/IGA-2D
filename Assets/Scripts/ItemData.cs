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
    public GameObject handheldPrefab;  // �ֳ�Сģ��

    [Header("Effect Params (�ӿ�Ԥ��)")]
    public float valueA;
    public float valueB;
    // ��������չ

    // Ԥ����ͳһ��ڣ��������ʱ����Ҽ�Buff/�޸����Եȣ�
    public virtual void OnEquip(PlayerInventory inv) { }
    public virtual void OnUnequip(PlayerInventory inv) { }
}
